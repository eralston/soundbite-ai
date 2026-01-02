using AutoMapper;
using Masticore.Ad;
using Masticore.Entity;
using Masticore.Exceptions;
using Masticore.Graph;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Token;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// User profile implementation using IGraph and IDirectory to read user claims and automatically build a User and Directory in the database
    /// </summary>
    public class UserService : IdentityServiceBase, IUserService
    {
        #region Properties

        protected IImageService Images { get; }
        protected IGraph Graph { get; }
        protected INotificationService Notifications { get; set; }

        protected IRbac Rbac { get; }

        #endregion

        #region Constructor

        public UserService(
            ISecurityContext securityContext,
            IRbac rbac,
            IImageService images,
            IGraph graph,
            INotificationService notifications,
            IIdentityInfrastructure infrastructure,
            IMapper mapper,
            ILogger<UserService> logger
            )
            : base(infrastructure, logger, mapper, securityContext)
        {
            // Graph is allowed to be null
            Graph = graph;

            Rbac = rbac;
            Images = images ?? throw new ArgumentNullException(nameof(images));
            Notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
        }

        #endregion

        #region Support Methods

        protected virtual async Task<User> DtoAsync(UserEntity user, bool includeImage = true)
        {
            User dto = new User
            {
                // IResource
                Route = user.Route,
                CreatedUtc = user.CreatedUtc,
                UpdatedUtc = user.UpdatedUtc,
                UniversalId = user.UniversalId,
                // IUserFields
                Email = user.Email,
                GivenName = user.GivenName,
                FamilyName = user.FamilyName,
                Phone = user.Phone,
                Title = user.Title,
                AllowNews = user.AllowNews,
                AllowMarketing = user.AllowMarketing,
                AllowEmail = user.AllowEmail,
                AllowSms = user.AllowSms,
                UserRole = user.UserRole,
                InviteAcceptUtc = user.InviteAcceptUtc,
                // IUser
                ImageSrc = includeImage ? await ImageSrcAsync(Mapper.MapSafe<User>(user)) : null,
            };

            return dto;
        }

        protected virtual async Task<string> ImageSrcAsync(User user)
        {
            try
            {
                string ret = await Images.UserImageAsync(user);
                if (ret != null)
                {
                    return ret;
                }

                // Not having graph means there is no fallback
                if (Graph == null)
                {
                    return null;
                }

                Stream graphPhoto = await Graph.MyPhotoAsync();
                if (graphPhoto == null || graphPhoto.Length == 0)
                {
                    return null;
                }

                return await Images.UploadUserImageAsync(user, graphPhoto);
            }
            catch (Exception exc)
            {
                Logger.LogWarning(exc, "Exception trying to pull url of image for {UserId}", user.UniversalId);
                return null;
            }
        }

        /// <summary>
        /// First check the <see cref="ISecurityContext"/> for the current user info, then fallback to the given <see cref="IUserFields"/> to find the current user's <see cref="UserEntity"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="me"></param>
        /// <returns></returns>
        protected async Task<UserEntity> MyUserEntityAsync(IIdentityDb db, IUserFields me, bool trustUser)
        {
            // Suggested fields could come from a raw REST call, in which case we should ignore them
            if (!trustUser)
            {
                me.Email = null;
                me.UniversalId = null;
            }

            // TODO: Handle cases where user is deleted; right now this will likely find null in all cases, then subsequent attempts to insert the same e-mail will fail

            // UID is more specific than e-mail
            // SecurityContext is more trustworthy than userFields
            // E-mail from graph is more accurate than the one from claims

            // Current User in security context rules all
            UserEntity userEntity = SecurityContext.CurrentUser == null ? null : await db.UserWhereRoute(SecurityContext.CurrentUser.Route);

            if (userEntity != null)
            {
                Logger.LogDebug("Found user by route {Route}", userEntity.Route);
                return userEntity;
            }

            return await DisambiguateUserEntity(db, me);
        }

        protected async Task<UserEntity> DisambiguateUserEntity(IIdentityDb db, IUserFields suggestedUser)
        {
            // Earlier in the list means higher priority
            List<UserEntity> candidateUsers = new List<UserEntity>();

            // By Security context UID
            if (SecurityContext.UniversalId != null)
            {
                UserEntity contextUidUser = await db.UserWhereUid(SecurityContext.UniversalId);
                if (contextUidUser != null)
                {
                    Logger.LogDebug("Found user by SecurityContext UID {UniversalId}", contextUidUser.UniversalId);
                    candidateUsers.Add(contextUidUser);
                }
            }

            // By argument UID
            if (suggestedUser.UniversalId != null)
            {
                UserEntity suggestedUidUser = await db.UserWhereUid(suggestedUser.UniversalId);
                if (suggestedUidUser != null)
                {
                    Logger.LogDebug("Found user by IUserFields UID {UniversalId}", suggestedUidUser.UniversalId);
                    candidateUsers.Add(suggestedUidUser);
                }
            }

            // By Security Context E-Mail
            if (SecurityContext.Email != null)
            {
                UserEntity contextEmailUser = await db.UserWhereEmail(SecurityContext.Email, SecurityContext.ProviderType);
                if (contextEmailUser != null)
                {
                    Logger.LogDebug("Found user by SecurityContext E-Mail {Email}", contextEmailUser.Email);
                    candidateUsers.Add(contextEmailUser);
                }
            }

            // By argument e-mail
            if (suggestedUser.Email != null)
            {
                UserEntity suggestedEmailUser = await db.UserWhereEmail(suggestedUser.Email, SecurityContext.ProviderType);
                if (suggestedEmailUser != null)
                {
                    Logger.LogDebug("Found user by IUserFields E-Mail {Email}", suggestedEmailUser.Email);
                    candidateUsers.Add(suggestedEmailUser);
                }
            }

            if (candidateUsers.Count == 0)
            {
                Logger.LogDebug("Could not find user; will need to make one for {UniversalId}", SecurityContext.UniversalId);
                return null;
            }
            else if (candidateUsers.Count == 1)
            {

                Logger.LogDebug("Found one candidate userEntity; returning it");
                return candidateUsers[0];
            }
            else
            {
                return await MergeUsers(db, candidateUsers);
            }
        }

        private async Task<UserEntity> MergeUsers(IIdentityDb db, List<UserEntity> candidateUsers)
        {
            // TODO: This should now use the UserIdentity aliasing system instead of merging
            // Must merge?
            Logger.LogWarning("Multiple users found for UID and e-mail combo; considering merge");
            Dictionary<string, UserEntity> candidateUsersByRoute =
                candidateUsers
                .GroupBy(u => u.Route)   // Group objects by Route
                .Select(grp => grp.First())  // Select the first object from each group
                .ToDictionary(u => u.Route, u => u);
            if (candidateUsersByRoute.Count == 1)
            {
                Logger.LogWarning("Multiple users found, but no merge necessary");
                return candidateUsers[0];
            }

            Logger.LogWarning("Multiple users found for UID and e-mail combo; must merge");
            foreach (UserEntity candidateUser in candidateUsers)
            {
                Logger.LogWarning("Candidate user {Route} with e-mail {Email}", candidateUser.Route, candidateUser.Email.RedactEmail());
            }
            UserEntity mainUserEntity = candidateUsers[0];

            PersonEntity[] mainUserPeople = await db.People
                    .Where(p => p.User.Route == mainUserEntity.Route)
                    .Include(p => p.Organization)
                    .Include(p => p.User)
                    .ToArrayAsync();

            Dictionary<string, PersonEntity> mainUserPeopleByOrgRoute = mainUserPeople.ToDictionary(p => p.Organization.Route, p => p);

            // For each user in the dictionary, if it's not the main user, then we must merge it
            // Merging means reassigning each PersonEntity pointing to the UserEntity to the main user
            foreach (UserEntity mergeCandidate in candidateUsersByRoute.Values)
            {
                if (mergeCandidate.Route == mainUserEntity.Route)
                {
                    continue;
                }

                Logger.LogWarning("Merging user {Route} into {MainRoute}", mergeCandidate.Route, mainUserEntity.Route);
                // Merge process
                // 1) Query all PersonEntity records associated with the given user
                PersonEntity[] personEntities = await db.People
                    .Where(p => p.User.Route == mergeCandidate.Route)
                    .Include(p => p.Organization)
                    .Include(p => p.User)
                    .ToArrayAsync();
                // 2) Update each PersonEntity to point to the main user
                foreach (PersonEntity existingPerson in personEntities)
                {
                    // Does the main user entity already have a person entity for this organization?
                    if (mainUserPeopleByOrgRoute.TryGetValue(existingPerson.Organization.Route, out PersonEntity mainUserPerson))
                    {
                        // If so, then we must remove the person entity because we can't have two people for the same user in the same organization
                        Logger.LogWarning("Removing person {Route} into {MainRoute}", existingPerson.Route, mainUserPerson.Route);
                        existingPerson.SoftDelete();
                    }
                    else
                    {
                        // If not, then we can just reassign the person entity to the main user
                        Logger.LogWarning("Reassigning person {Route} to {MainRoute}", existingPerson.Route, mainUserEntity.Route);
                        existingPerson.User = mainUserEntity;
                    }

                }
                mergeCandidate.MergedToUser = mainUserEntity;
                if (mergeCandidate.Email != null)
                {
                    string newId = ResourceExtensions.NewRoute();
                    mergeCandidate.Email = $"merged-{newId}-{mergeCandidate.Email}";
                }

                if (mergeCandidate.UniversalId != null)
                {
                    string newId = ResourceExtensions.NewRoute();
                    mergeCandidate.UniversalId = $"merged-{newId}-{mergeCandidate.UniversalId}";
                }

                mergeCandidate.Timestamp();
            }
            // Save changes to DB
            await db.SaveChangesAsync();
            return mainUserEntity;
        }

        private async Task<(UserEntity, bool)> UpsertUserEntity(IIdentityDb db, UserEntity userEntity, User suggestedUserFields)
        {
            // If we're missing UID or email in the fields, but have them in the context, then fallback
            suggestedUserFields.UniversalId ??= SecurityContext.UniversalId;
            suggestedUserFields.Email ??= SecurityContext.Email;

            // We need e-mail AND universal ID to proceed because this either makes a fully viable user OR not, it cannot leave with only an invite
            if (!suggestedUserFields.Email.IsEmail() || string.IsNullOrEmpty(suggestedUserFields.UniversalId))
            {
                throw new BadRequestException("Unable to determine current user e-mail or UID; cannot upsert record");
            }

            //TODO: Revisit this --- I had to remove this because a user comming in from Teams but who is imported from OKTA
            //      will have a mismatch here so we need to skip it. This might be fine to remove but it should be re-looked 
            //      at when implementing the Universal ID fixes for one-to-many third-party directories per user.
            /*
            // If we already have a UID and they're suggesting a new one, then this is problematic and we abort
            if (userEntity != null && !string.IsNullOrEmpty(userEntity.UniversalId) && userEntity.UniversalId != suggestedUserFields.UniversalId)
            {
                throw new NotFoundException($"Unable to match credentials");
            }
            */

            bool isAccepting = false;

            // If we never found a record, then we must make one
            if (userEntity == null)
            {
                Logger.LogDebug("Creating new user for {UniversalId}", suggestedUserFields.UniversalId);
                userEntity = db.Users.CreateResource();
                userEntity.UserRole = UserRole.User;
                userEntity.AllowMarketing = true;
                userEntity.AllowNews = true;
                userEntity.AllowEmail = true;
                userEntity.AllowSms = true;
                isAccepting = true;
            }

            // UID should not change once set, but it may not exist yet due to invites
            if (userEntity.UniversalId == null)
            {
                Logger.LogDebug("Accepting invite for user {UniversalId}", suggestedUserFields.UniversalId);
                isAccepting = true;
            }

            // UniversalId should be read-only once set
            userEntity.UniversalId ??= suggestedUserFields.UniversalId;
            userEntity.Email = suggestedUserFields.Email.ToEmail();

            // Everything else we favor userFields, but preserve anything that we already have with no new value
            userEntity.Phone = suggestedUserFields.Phone != null ? suggestedUserFields.Phone : userEntity.Phone;
            userEntity.GivenName = suggestedUserFields.GivenName != null ? suggestedUserFields.GivenName : userEntity.GivenName;
            userEntity.FamilyName = suggestedUserFields.FamilyName != null ? suggestedUserFields.FamilyName : userEntity.FamilyName;
            userEntity.Title = suggestedUserFields.Title != null ? suggestedUserFields.Title : userEntity.Title;

            // Fixed value based on latest claims
            userEntity.ProviderType = suggestedUserFields.ProviderType;

            userEntity.Timestamp();
            isAccepting = userEntity.Accept() || isAccepting;
            await db.SaveChangesAsync();

            if (isAccepting)
            {
                Logger.LogDebug("Sending welcome notification to {UniversalId}", userEntity.UniversalId);
                User user = Mapper.MapSafe<User>(userEntity);
                await Notifications.WelcomeAsync(user);
            }

            return (userEntity, isAccepting);
        }

        protected virtual void ApplyUserFields(User userFields, UserEntity user)
        {
            if (!string.IsNullOrEmpty(userFields.UniversalId))
            {
                user.UniversalId = userFields.UniversalId;
            }
            if (!string.IsNullOrEmpty(userFields.GivenName))
            {
                user.GivenName = userFields.GivenName;
            }
            else if (string.IsNullOrEmpty(user.GivenName))
            {
                user.GivenName = SecurityContext.FullName;
            }

            if (!string.IsNullOrEmpty(userFields.FamilyName))
            {
                user.FamilyName = userFields.FamilyName;
            }

            if (!string.IsNullOrEmpty(userFields.Phone))
            {
                user.Phone = userFields.Phone;
            }

            if (!string.IsNullOrEmpty(userFields.Title))
            {
                user.Title = userFields.Title;
            }
        }

        protected virtual async Task<bool> ApplyUserGraphFieldsAsync(IClaims claims, User user)
        {
            try
            {
                // Read from Graph
                string userUrlByEmail = GraphUser.UserUrlByEmail(claims.Email);
                if (userUrlByEmail == null)
                {
                    return false;
                }

                GraphClient graphClient = new GraphClient(Logger, AdAppSettings.Instance, claims.DirectoryUniversalId);
                GraphRequest graphRequest = new GraphRequest(graphClient, userUrlByEmail)
                {
                    Select = GraphUser.SelectFields,
                };
                string url = graphRequest.Url();
                GraphUser graphUser = await graphRequest.Get<GraphUser>();
                if (graphUser != null)
                {
                    user.GivenName = graphUser.GivenName != null ? graphUser.GivenName : user.GivenName;
                    user.FamilyName = graphUser.FamilyName != null ? graphUser.FamilyName : user.FamilyName;
                    user.Phone = graphUser.Phone != null ? graphUser.Phone : user.Phone;
                    user.Email = graphUser.Email != null ? graphUser.Email : user.Email;
                    user.Title = graphUser.Title != null ? graphUser.Title : user.Title;
                }

                return graphUser != null;

            }
            catch (Exception exc)
            {
                Logger.LogWarning(exc, "Exception trying to apply graph data for {UserId}", user.UniversalId);
                return false;
            }
        }

        public virtual async Task<User> DoUpsertMeAsync(User suggestedUserFields, bool trustUser)
        {
            Validator.NotNull(SecurityContext, $"{nameof(SecurityContext)} cannot be null");
            Validator.ArgNotNull(nameof(suggestedUserFields), suggestedUserFields);

            // TODO: Logic to only update periodically?

            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            UserEntity userEntity = await MyUserEntityAsync(db, suggestedUserFields, trustUser);
            (UserEntity finalUserEntity, bool isAccepting) = await UpsertUserEntity(db, userEntity, suggestedUserFields);

            User dto = await DtoAsync(finalUserEntity);
            dto.IsAccepting = isAccepting;
            return dto;
        }

        #endregion

        #region IUserService

        public virtual async Task<User> UpsertMeAsync(User suggestedUserFields)
        {
            return await DoUpsertMeAsync(suggestedUserFields, false);
        }

        public async Task<User> UpsertMeAsync(IClaims claims)
        {
            // To make a new valid user, one needs both an e-mail and a universalId
            if (claims.Email == null || claims.UniversalId == null)
            {
                return null;
            }

            // Create record
            User user = new User
            {
                Email = claims.Email,
                UniversalId = claims.UniversalId,
                ProviderType = claims.ProviderType,
            };
            claims.ApplyWesternName(user);


            await ApplyUserGraphFieldsAsync(claims, user);
            return await DoUpsertMeAsync(user, true);
        }

        public virtual async Task<User> UpdateMeAsync(User suggestedUserFields)
        {
            if (suggestedUserFields == null)
            {
                // Return the current user from the security context
                return SecurityContext.CurrentUser;
            }

            Validator.NotNull(SecurityContext, "SecurityContext cannot be nulll");
            Validator.NotNull(SecurityContext.CurrentUser, "SecurityContext must have a valid current user.");

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            UserEntity userEntity = SecurityContext.CurrentUser == null ? null
                : await db.UserWhereRoute(SecurityContext.CurrentUser.Route);

            userEntity.AssertFound();

            ApplyUserFields(suggestedUserFields, userEntity);
            userEntity.Timestamp();
            await db.SaveChangesAsync();

            User result = await DtoAsync(userEntity);
            return result;
        }

        public virtual async Task<User> ReadMeAsync(bool includeImage = true)
        {
            if (SecurityContext.UniversalId is null)
            {
                throw new ArgumentNullException(nameof(SecurityContext.UniversalId));
            }

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            UserEntity activeUser = (await db.UserWhereUid(SecurityContext.UniversalId)).AssertFound();

            User dto = await DtoAsync(activeUser, includeImage);
            return dto;
        }

        public virtual async Task<bool> SetSecurityContextFromRefreshToken(string refreshToken)
        {
            bool result = false;
            RefreshToken refreshTokenParsed = RefreshToken.Parse(refreshToken);

            // Make sure the token is valid based on syntax and expiration
            if (refreshTokenParsed != null && !refreshTokenParsed.IsExpired)
            {
                IIdentityDb db = await Infrastructure.IdentityDbAsync();
                UserEntity userEntity = await db.UserWhereRoute(refreshTokenParsed.UserRoute);

                // Make sure refresh token matches refresh token on file for the user
                if (userEntity != null && userEntity.RefreshToken == refreshToken)
                {
                    User user = Mapper.MapSafe<User>(userEntity);
                    SecurityContext.CurrentUser = user;
                    SecurityContext.UniversalId = user.UniversalId;
                    result = true;
                }
            }

            return result;
        }

        public virtual async Task<User> UpsertMyImageAsync(Stream image)
        {
            if (SecurityContext.UniversalId is null)
            {
                throw new ArgumentNullException(nameof(SecurityContext.UniversalId));
            }

            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            await Images.UploadUserImageAsync(SecurityContext, image);
            return await ReadMeAsync();
        }

        public async Task<string> ReadRouteFromUniversalId(string universalId)
        {
            //Validate parameters
            Validator.ArgNotNull(nameof(universalId), universalId);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            string result = await db.Users
                    .Where(i => i.UniversalId == universalId
                        && i.DeletedUtc == null)
                    .Select(i => i.Route)
                    .FirstOrDefaultAsync();

            return result;
        }

        public virtual async Task<InviteResult[]> InviteAsync(Masticore.Models.Invite[] invites)
        {
            Validator.NotNull(SecurityContext, "SecurityContext cannot be null.");
            Validator.ArgNotNull(nameof(invites), invites);
            Validator.SecurityContextUniversalIdRequired(SecurityContext);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            UserEntity user = (await db.UserWhereUid(SecurityContext.UniversalId)).AssertFound();

            Invite inviter = new Invite(db, Mapper, user);
            List<InviteResult> ret = new List<InviteResult>();
            foreach (Masticore.Models.Invite invite in invites)
            {
                InviteResult result = await inviter.InviteUserWhereUid(invite.Token, Notifications);
                ret.Add(result);
            }
            await db.SaveChangesAsync();

            return ret.ToArray();
        }

        public async Task<User> ReadByOrgRouteAndUniversalId(string orgRoute, string universalId)
        {
            //TODO: implement tests for this method
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute);
            Validator.ArgNotNullOrEmpty(nameof(universalId), universalId);

            // Validate User
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Person);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            UserEntity userEntity = await db.People
                .Where(i => i.User.UniversalId == universalId
                    && i.User.DeletedUtc == null
                    && i.Organization.Route == orgRoute
                    && i.Organization.DeletedUtc == null
                    && i.Organization.Tenant.DeletedUtc == null)
                .Select(i => i.User)
                .FirstOrDefaultAsync();

            User result = Mapper.Map<User>(userEntity);
            return result;
        }

        public async Task<User> ReadByOrgRouteAndEmail(string orgRoute, string email, bool includeImage = true)
        {
            //TODO: implement tests for this method
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute);
            Validator.ArgNotNullOrEmpty(nameof(email), email);

            // Validate User
            await Rbac.AssertCurrentUser();

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            UserEntity userEnt = await db.People
                .Where(i => i.User.Email == email
                    && i.User.DeletedUtc == null
                    && i.Organization.Route == orgRoute
                    && i.Organization.DeletedUtc == null
                    && i.Organization.Tenant.DeletedUtc == null)
                .Select(i => i.User)
                .FirstOrDefaultAsync();

            if (userEnt == null)
            {
                return null;
            }

            User ret = await DtoAsync(userEnt, includeImage);
            return ret;
        }

        /// <inheritdoc />
        public async Task<User> ReadByEmail(string email, bool includeImage = true)
        {
            Validator.NotNullOrEmpty(nameof(email), email);
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            UserEntity userEnt = await db.Users
                .Where(i => i.Email == email && i.DeletedUtc == null)
                .FirstOrDefaultAsync();
            if (userEnt != null)
            {
                return await DtoAsync(userEnt, includeImage);
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc />
        public async Task UpdateMyNotifications(IUserNotifications myNotificationSettings)
        {
            Validator.NotNull(nameof(myNotificationSettings), myNotificationSettings);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            UserEntity userEntity = await db.UserWhereUid(SecurityContext.UniversalId);
            userEntity.AssertFound();

            userEntity.AllowEmail = myNotificationSettings.AllowEmail;
            userEntity.AllowSms = myNotificationSettings.AllowSms;
            userEntity.AllowNews = myNotificationSettings.AllowNews;
            userEntity.AllowMarketing = myNotificationSettings.AllowMarketing;

            await db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<User> ReadAsync(string userRoute, bool includeImage = true)
        {
            Validator.NotNullOrEmpty(nameof(userRoute), userRoute);
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            UserEntity userEnt = await db.Users
                .Where(u => u.Route == userRoute && u.DeletedUtc == null)
                .FirstOrDefaultAsync();
            userEnt.AssertFound();
            return await DtoAsync(userEnt, includeImage);
        }

        #endregion
    }
}
