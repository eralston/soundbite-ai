using AutoMapper;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// <see cref="IPersonService"/> over Entity Framework Core
    /// </summary>
    public class PersonService : IdentityServiceBase, IPersonService
    {
        /// <summary>
        /// The maximum total people returned by <see cref="ReadAllAsync(string, IndexPageRequest)"/>
        /// </summary>
        public const int MaxResultsReadAllAsync = int.MaxValue;

        protected INotificationService Notifications { get; }
        protected IImageService Images { get; }
        protected IRbac Rbac { get; }

        #region Methods

        public PersonService(
            IRbac rbac,
            INotificationService notifications,
            IIdentityInfrastructure infrastructure,
            IMapper mapper,
            IImageService imageService,
            ILogger<PersonService> logger)
            : base(infrastructure, logger, mapper)
        {
            Notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
            Rbac = rbac ?? throw new ArgumentNullException(nameof(rbac));
            Images = imageService ?? throw new ArgumentNullException(nameof(imageService));
        }

        private async Task LoadUserImagesAsync(IEnumerable<Person> people)
        {
            // If we're returning more than a regular page worth, then assuming this is some kind of batch processing and skip images
            if (people.Count() > PageRequestBase.DefaultMaxTake)
            {
                return;
            }

            Dictionary<string, Task<string>> tasks = new Dictionary<string, Task<string>>();
            foreach (Person person in people)
            {
                string key = person.User.Route;
                if (tasks.ContainsKey(key) || person.User.ImageSrc != null)
                {
                    continue;
                }
                Task<string> task = Images.UserImageAsync(person.User);
                tasks.Add(key, task);
            }

            await Task.WhenAll(tasks.Values);

            foreach (Person person in people)
            {
                string key = person.User.Route;
                if (!tasks.ContainsKey(key) || person.User.ImageSrc != null)
                {
                    continue;
                }
                string imageSrc = tasks[key].Result;
                person.User.ImageSrc = imageSrc;
            }
        }

        private async Task<PersonEntity> PersonEntityAsync(IIdentityDb db, string orgRoute, string personRoute)
        {
            IQueryable<PersonEntity> query = from
                                                person in db.People
                                             where
                                                person.DeletedUtc == null &&
                                                person.Route == personRoute &&
                                                person.User.DeletedUtc == null &&
                                                person.Organization.DeletedUtc == null &&
                                                person.Organization.Route == orgRoute &&
                                                person.Organization.Tenant.DeletedUtc == null
                                             select person;

            PersonEntity ret = await query.FirstOrDefaultAsync();
            ret.AssertFound($"Could not find Person with id {personRoute}");

            return ret;
        }

        protected virtual async Task<Person> PersonWhereUid(string orgRoute, string personRoute, IIdentityDb db)
        {
            PersonEntity person = await PersonEntityAsync(db, orgRoute, personRoute);
            Person result = Mapper.MapSafe<Person>(person);
            return result;
        }


        #endregion

        #region IPersonService

        public virtual async Task<Person> ReadMeAsync(string orgRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Guest);

            // TODO: Caching

            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            Person person = await db.PersonWhereUid(Mapper, Rbac.UniversalIdForCurrentUser, orgRoute);

            // TODO: Hydrate?

            return person;
        }

        public virtual async Task<Person> ReadAsync(string orgRoute, string personRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(personRoute), personRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Guest);

            // TODO: Caching

            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            IQueryable<Person> query = from p in db.People
                                       where p.DeletedUtc == null &&
                                             p.Route == personRoute &&
                                             p.Organization.Route == orgRoute &&
                                             p.Organization.DeletedUtc == null &&
                                             p.Organization.Tenant.DeletedUtc == null &&
                                             p.Organization.People.Any(p =>
                                                             p.DeletedUtc == null
                                                             && p.User.UniversalId == Rbac.UniversalIdForCurrentUser)
                                       select Mapper.MapSafeWith<Person>(p, p.User);


            Person me = (await query.FirstOrDefaultAsync()).AssertFound($"Could not find Person with id {personRoute}");
            me.User.ImageSrc = await Images.UserImageAsync(me.User);

            return me;
        }

        public virtual async Task<InviteResult[]> InviteAsync(string orgRoute, Models.Invite[] invites)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(invites), invites);

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Person);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            // TODO: Make person invite role requirement configurable
            UserEntity user = await db.UserWhereUid(orgRoute, Rbac.UniversalIdForCurrentUser, PersonRole.Person);
            user.AssertFound("Could not find user for claims");

            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Could not find org '{orgRoute}'");

            Invite inviter = new Invite(db, Mapper, user, org);
            List<InviteResult> ret = new List<InviteResult>();
            foreach (Models.Invite invite in invites)
            {
                InviteResult result = await inviter.InviteUserWhereUid(invite.Token, Notifications);
                ret.Add(result);
            }
            await db.SaveChangesAsync();

            return ret.ToArray();
        }

        public virtual async Task DeleteAsync(string orgRoute, string personRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(personRoute), personRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            PersonEntity personToDelete = await PersonEntityAsync(db, orgRoute, personRoute);

            personToDelete.SoftDelete();
            await db.SaveChangesAsync();
        }

        public virtual async Task<Person> UpdateAsync(string orgRoute, string personRoute, Person personFields)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(personRoute), personRoute);
            Validator.ArgNotNull(nameof(personFields), personFields);

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Person);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            PersonEntity personToUpdate = await PersonEntityAsync(db, orgRoute, personRoute);
            personToUpdate.PersonRole = personFields.PersonRole;
            await db.SaveChangesAsync();

            return await ReadAsync(orgRoute, personRoute);
        }

        public async Task<IEnumerable<Person>> ReadAllAsync_Obsolete(string orgRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Person);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            IEnumerable<Person> people =
               await db.QueryPeopleWithUsers(orgRoute)
               .MapReadOnlyAsync(e => Mapper.MapSafeWith<Person>(e, e.User));

            await LoadUserImagesAsync(people);

            return people;
        }

        public async Task<IndexPageResponse<Person>> ReadAllAsync(string orgRoute, IndexPageRequest page = null, PersonRole? personRole = null)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Person);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            IQueryable<PersonEntity> entities = db.QueryPeopleWithUsers(orgRoute);

            if (personRole != null)
            {
                entities = entities.Where(p => p.PersonRole == personRole);
            }

            // People allows for a different max result, which means we use a PageQuery
            IndexPageQuery query = new IndexPageQuery(page).SetMaxResults(MaxResultsReadAllAsync);
            IndexPageResponse<PersonEntity> entityPage = await entities.ReadPageAsync(query, (p) => p.User.Email.Contains(page.Filter.ToLower()));
            IndexPageResponse<Person> ret = entityPage.Map((p) => Mapper.MapSafeWith<Person>(p, p.User));
            ret.BasedOn(query);

            await LoadUserImagesAsync(ret.Result);

            return ret;
        }

        #endregion
    }
}
