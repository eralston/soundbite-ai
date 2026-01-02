using AutoMapper;
using Masticore;
using Masticore.Entity;
using Masticore.Exceptions;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// EF Implementation of <see cref="ISessionCommentService"/>
    /// </summary>
    public class SessionCommentService : InfrastructureServiceBase<ISbInfrastructure>, ISessionCommentService
    {
        #region Properties

        protected IRbac Rbac { get; }

        protected IImageService Images { get; }

        #endregion

        #region Supporting Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rbac"></param>
        /// <param name="infrastructure"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        public SessionCommentService(
            IRbac rbac,
            ISbInfrastructure infrastructure,
            IMapper mapper,
            ILogger<SessionCommentService> logger,
            IImageService images)
            : base(infrastructure, logger, null, mapper)
        {
            Validator.NotNull(rbac, nameof(rbac));
            Validator.NotNull(images, nameof(images));

            Rbac = rbac;
            Images = images;
        }

        public async Task<SessionEntity> AssertHasSessionAccess(
            SbDb db,
            string orgRoute,
            string sessionRoute,
            string commentsDisallowedMsg = null)
        {
            // They must have access to the org itself
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Person);

            // Must have access to the session
            string userUniversalId = Rbac.UniversalIdForCurrentUser;

            int[] groupIds = userUniversalId == null
                ? Array.Empty<int>()
                : await db.GroupIdsAsync(orgRoute, userUniversalId);

            SessionEntity session = await db.SessionAsync(userUniversalId, orgRoute, sessionRoute, groupIds);
            session.AssertNotGone($"Session '{sessionRoute}' in org '{orgRoute}' has been removed");

            if (commentsDisallowedMsg != null)
            {
                // Check at the session level
                if (session.SessionCommentPolicy != SessionCommentPolicy.Allowed)
                {
                    throw new ConflictException(commentsDisallowedMsg);
                }

                // Check at the org level
                OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
                org.AssertFound();
                OrgSessionSettings settings = org.GetSessionSettings();
                // By default, orgs allow comments
                if (settings != null && !settings.SessionCommentsEnabled)
                {
                    throw new ForbiddenException(commentsDisallowedMsg);
                }
            }

            return session;
        }

        private static async Task<SessionCommentEntity> QueryComment(
            SbDb db,
            string orgRoute,
            string sessionRoute,
            string commentRoute,
            bool includeChildren)
        {
            IQueryable<SessionCommentEntity> query = db.SessionComments.Where(s =>
                                s.DeletedUtc == null &&
                                s.Route == commentRoute &&
                                s.Session.DeletedUtc == null &&
                                s.Session.Route == sessionRoute &&
                                s.Session.Organization.DeletedUtc == null &&
                                s.Session.Organization.Route == orgRoute &&
                                s.Session.Organization.Tenant.DeletedUtc == null);


            if (includeChildren)
            {
                query = query
                            .Include(s => s.Person)
                            .ThenInclude(p => p.User);
            }

            return await query.FirstOrDefaultAsync();
        }

        private async Task LoadImageUrls(IEnumerable<SessionComment> comments)
        {
            Dictionary<string, Task<string>> imageTasks = new Dictionary<string, Task<string>>();

            // Map comment users into image requests
            foreach (SessionComment comment in comments)
            {
                string userRoute = comment.Person?.User?.Route;
                if (userRoute == null || imageTasks.ContainsKey(userRoute))
                {
                    continue;
                }

                // Contributor image URL
                imageTasks[userRoute] = Images.UserImageAsync(comment.Person.User);
            }

            // Wait on the tasks to finish querying storage in parallel
            await Task.WhenAll(imageTasks.Values.ToArray());

            // Pull from the mapping the image for each user;
            foreach (SessionComment comment in comments)
            {
                string userRoute = comment.Person?.User?.Route;

                if (userRoute != null)
                {
                    comment.Person.User.ImageSrc = imageTasks[userRoute].Result;
                }
            }
        }

        #endregion

        #region ISessionCommentService

        /// <inheritdoc />
        public async Task<SessionComment> CreateAsync(
            string orgRoute,
            string sessionRoute,
            NewSessionComment newComment)
        {
            // Input validation
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(sessionRoute), sessionRoute);
            Validator.ArgNotNull(nameof(newComment), newComment);

            string newCommentText = newComment.Content.Trim();
            Validator.NotNullOrWhitespace(nameof(newComment.Content), newCommentText);
            Validator.IsTrue(newCommentText.Length > 0, "Cannot have empty comment content");

            // Read related objects
            SbDb db = await Infrastructure.DbAsync();
            SessionEntity session = await AssertHasSessionAccess(
                db,
                orgRoute,
                sessionRoute,
                $"Cannot create comment; Session '{sessionRoute}' in organization '{orgRoute}' does not allow comments");

            PersonEntity currentPerson = await db.PersonWhereUid(orgRoute, Rbac.UniversalIdForCurrentUser, PersonRole.Person);
            currentPerson.AssertFound();

            // Create new entity and load
            SessionCommentEntity comment = db.SessionComments.CreateResource(currentPerson.User);
            comment.Content = newCommentText;
            comment.Person = currentPerson;
            comment.Session = session;
            await db.SaveChangesAsync();

            SessionComment ret = Mapper.MapSafeWith<SessionComment>(
                comment,
                comment.Person,
                comment.Person.User);
            return ret;
        }

        /// <inheritdoc />
        public async Task<IndexPageResponse<SessionComment>> ReadAllAsync(
            string orgRoute,
            string sessionRoute,
            IndexPageRequest pageRequest = null)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(sessionRoute), sessionRoute);

            // Read related objects
            SbDb db = await Infrastructure.DbAsync();
            await AssertHasSessionAccess(db, orgRoute, sessionRoute);

            // Query all
            IQueryable<SessionComment> query =
                            from comment in db.SessionComments

                            where
                                comment.DeletedUtc == null &&
                                comment.Session.DeletedUtc == null &&
                                comment.Session.Route == sessionRoute &&
                                comment.Session.IsTemplate == false &&
                                comment.Session.Organization.DeletedUtc == null &&
                                comment.Session.Organization.Route == orgRoute &&
                                comment.Session.Organization.Tenant.DeletedUtc == null &&
                                comment.Person.DeletedUtc == null &&
                                comment.Person.User.DeletedUtc == null
                            // Most recent on bottom
                            orderby comment.Id ascending
                            select new SessionComment
                            {
                                // Resource
                                Route = comment.Route,
                                CreatedUtc = comment.CreatedUtc,
                                UpdatedUtc = comment.UpdatedUtc,
                                DeletedUtc = comment.DeletedUtc,

                                // SessionComment
                                Content = comment.Content,
                                Person = Mapper.MapSafeWith<Person>(comment.Person, comment.Person.User),
                            };

            // Must be tracked; otherwise, the mapper won't complete pulling Person.User
            IndexPageResponse<SessionComment> ret = await query.ReadPageResponseAsync(pageRequest, null, false);

            await LoadImageUrls(ret.Result);

            return ret;
        }

        /// <inheritdoc />
        public async Task<SessionComment> UpdateAsync(
            string orgRoute,
            string sessionRoute,
            string commentRoute,
            NewSessionComment updatedComment)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(sessionRoute), sessionRoute);
            Validator.ArgNotNull(nameof(commentRoute), commentRoute);
            Validator.ArgNotNull(nameof(updatedComment), updatedComment);

            // Read related objects
            SbDb db = await Infrastructure.DbAsync();
            SessionEntity session = await AssertHasSessionAccess(
                db,
                orgRoute,
                sessionRoute,
                $"Cannot update comment '{commentRoute}'; Session '{sessionRoute}' in organization '{orgRoute}' does not allow comments");

            SessionCommentEntity commentEntity = await QueryComment(db, orgRoute, sessionRoute, commentRoute, true);

            commentEntity.AssertFound();

            // Update
            commentEntity.Content = updatedComment.Content;
            commentEntity.Timestamp();
            await db.SaveChangesAsync();

            SessionComment ret = Mapper.MapSafeWith<SessionComment>(
                commentEntity,
                commentEntity.Person,
                commentEntity.Person.User);
            return ret;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(
            string orgRoute,
            string sessionRoute,
            string commentRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(sessionRoute), sessionRoute);
            Validator.ArgNotNull(nameof(commentRoute), commentRoute);

            // Read related objects
            SbDb db = await Infrastructure.DbAsync();

            await AssertHasSessionAccess(
                db,
                orgRoute,
                sessionRoute,
                $"Cannot delete comment '{commentRoute}'; Session '{sessionRoute}' in organization '{orgRoute}' does not allow comments");

            SessionCommentEntity commentEntity = await QueryComment(db, orgRoute, sessionRoute, commentRoute, false);
            commentEntity.AssertFound();

            commentEntity.SoftDelete();
            await db.SaveChangesAsync();
        }

        #endregion
    }
}
