using Masticore.Exceptions;
using Masticore.Resources;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Masticore.Entity
{
    /// <summary>
    /// Static class for extensions with the system database
    /// Queries in here DO obey soft delete
    /// Queries in here do NOT obey RBAC
    /// </summary>
    public static partial class DbExtensions
    {
        /// <summary>
        /// Starts the given query in a read-only mode using <see cref="EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}(IQueryable{TEntity})"/>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Task<TEntity[]> ReadOnly<TEntity>(this IQueryable<TEntity> query)
            where TEntity : class
        {
            return query.AsNoTracking().ToArrayAsync();
        }

        /// <summary>
        /// SingleOrDefaultAsync for read-only data
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Task<TEntity> ReadOnlyOne<TEntity>(this IQueryable<TEntity> query)
            where TEntity : class
        {
            return query.AsNoTracking().SingleOrDefaultAsync();
        }

        /// <summary>
        /// Creates an entity of the given <see cref="ResourceEntityBase"/> child type and adds it to the given DbSet, optionally assigning the given creator
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="dbSet"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public static TResource CreateResource<TResource>(this DbSet<TResource> dbSet, UserEntity creator = null)
             where TResource : ResourceEntityBase, new()
        {
            TResource entity = ResourceEntityBase.Create<TResource>(creator);
            dbSet.Add(entity);
            return entity;
        }

        /// <summary>
        /// Gets the user associated with the given email
        /// </summary>
        /// <param name="db"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static async Task<UserEntity> UserWhereEmail(this IIdentityDb db, string email, ProviderType providerType = ProviderType.Unknown, bool allowDeleted = false)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            // Null email is allowed here
            email = email.ToEmail();
            if (email is null)
            {
                return null;
            }

            IQueryable<UserEntity> users = db.Users.Where(u => u.Email == email);

            // Default filter by NOT deleted
            if (!allowDeleted)
            {
                users.Where(u => u.DeletedUtc == null);
            }

            // E-mail is unique in the table, so this can only return one record
            UserEntity mainUser = await users.FirstOrDefaultAsync();

            // We found a user with the email and the right provider type or perhaps an unknown one that we just need to discover and hopefully set soon
            if (mainUser != null && (providerType == ProviderType.Unknown || mainUser.ProviderType == ProviderType.Unknown || mainUser.ProviderType == providerType))
            {
                return mainUser;
            }

            // Take all the unique users that have the matching email
            UserIdentityEntity[] aliasUsers = await db.UserIdentities
                .Where(u => u.Identifier == email)
                .Include(u => u.User)
                .ToArrayAsync();

            // If we still didn't find anything then we have nothing to return
            if (aliasUsers.Length == 0)
            {
                return null;
            }

            // If we have a type to consider, then we look at those, plus any unknowns
            if (providerType != ProviderType.Unknown)
            {
                aliasUsers = aliasUsers.Where(u => u.User.ProviderType == providerType || u.ProviderType == ProviderType.Unknown).ToArray();
            }

            // If we found too many then we need to disambiguate with manual data correction of some kind; this shouldn't happen :(
            if (aliasUsers.Length > 1)
            {
                throw new NotFoundException($"Multiple users found for email {email.RedactEmail()}");
            }

            return aliasUsers[0].User;
        }

        /// <summary>
        /// Async read the <see cref="UserIdentityEntity"/> objects for the given user by id; returning empty if the ID is not set
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userId"></param>
        /// <param name="providerType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<UserIdentityEntity[]> UserIdentitiesWhereUserId(this IIdentityDb db, int userId)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            // If ID is zero, then that indicates the user is not yet in the database so they can't have any records
            if (userId == 0)
            {
                return new UserIdentityEntity[] { };
            }

            UserIdentityEntity[] ret = await db.UserIdentities.Where(u => u.User.Id == userId).ToArrayAsync();
            return ret;
        }

        /// <summary>
        /// Query <see cref="UserIdentityEntity"/> from the given database for the given identifier (email)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="identitifer"></param>
        /// <param name="providerType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<UserIdentityEntity> UserIdentityForIdentifier(this IIdentityDb db, string identitifer, ProviderType providerType = ProviderType.Unknown)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }
            if (string.IsNullOrEmpty(identitifer))
            {
                throw new ArgumentException($"'{nameof(identitifer)}' cannot be null or empty.", nameof(identitifer));
            }
            identitifer = identitifer.ToEmail();
            if (identitifer is null)
            {
                return null;
            }
            IQueryable<UserIdentityEntity> query = db.UserIdentities.Where(u => u.Identifier == identitifer);
            if (providerType != ProviderType.Unknown)
            {
                query = query.Where(u => u.ProviderType == providerType);
            }
            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets the user associated with the given email
        /// </summary>
        /// <param name="db"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static async Task<UserEntity> UserWhereRoute(this IIdentityDb db, string userRoute, bool allowDeleted = false)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(userRoute))
            {
                throw new ArgumentException($"'{nameof(userRoute)}' cannot be null or empty.", nameof(userRoute));
            }

            IQueryable<UserEntity> users = db.Users.Where(u => u.Route == userRoute);

            if (!allowDeleted)
            {
                users.Where(u => u.DeletedUtc == null);
            }

            UserEntity result = await users.FirstOrDefaultAsync();
            return result;
        }


        /// <summary>
        /// Queries the database for an invited user matching the given claims
        /// </summary>
        /// <param name="db"></param>
        /// <param name="claims"></param>
        /// <returns></returns>
        public static async Task<UserEntity> InvitedUserWhereUid(this IIdentityDb db, string userUniversalId, string email)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            // TODO: Resolve issues where we find more than one here - Need user merging
            return await db.Users
                                .Where(u => u.UniversalId == userUniversalId
                                            || u.Email == email)
                                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a person, as long as they have at least the given role on the org (system admins also return as true, but still need a person record on the org)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="claimsOrUser"></param>
        /// <param name="orgRoute"></param>
        /// <param name="personRole"></param>
        /// <returns></returns>
        public static async Task<PersonEntity> PersonWhereUid(this IIdentityDb db, string orgRoute, string userUniversalId, PersonRole personRole, bool allowDeleted = false, bool includeRelations = false)
        {

            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (userUniversalId is null)
            {
                throw new ArgumentNullException(nameof(userUniversalId));
            }

            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty", nameof(orgRoute));
            }

            IQueryable<PersonEntity> query = from person in db.People
                                             let hasOrgAccess =
                                                       // Has access as org admin
                                                       (from admin in db.People
                                                        where admin.DeletedUtc == null &&
                                                            admin.User.DeletedUtc == null &&
                                                            admin.Organization.DeletedUtc == null &&
                                                            admin.Organization.Tenant.DeletedUtc == null &&
                                                            admin.PersonRole >= personRole &&
                                                            admin.User.UniversalId == userUniversalId &&
                                                            admin.Organization.Route == orgRoute
                                                        select admin).Any()

                                             let isSystemAdmin =
                                                        // Has access as system admin
                                                        (from user in db.Users
                                                         where user.DeletedUtc == null &&
                                                             user.UserRole >= UserRole.God &&
                                                             user.UniversalId == userUniversalId
                                                         select user).Any()

                                             where
                                                   person.DeletedUtc == null &&
                                                   person.User.DeletedUtc == null &&
                                                   person.Organization.DeletedUtc == null &&
                                                   person.Organization.Tenant.DeletedUtc == null &&
                                                   person.Organization.Route == orgRoute &&
                                                   person.User.UniversalId == userUniversalId &&
                                                   (hasOrgAccess || isSystemAdmin)
                                             select person;
            // Include their user and org
            query = FilterAndIncludePerson(query, allowDeleted, includeRelations);

            return await query.FirstOrDefaultAsync();


        }

        /// <summary>
        /// Async queries for the person record connected to the given claims in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userUniversalId"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        public static async Task<PersonEntity> PersonWhereUid(this IIdentityDb db, string orgRoute, string userUniversalId, bool allowDeleted = false, bool includeRelations = false)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (userUniversalId is null)
            {
                throw new ArgumentNullException(nameof(userUniversalId));
            }

            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty", nameof(orgRoute));
            }

            IQueryable<PersonEntity> query = QueryPerson(db, orgRoute, userUniversalId, allowDeleted, includeRelations);

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Async queries for the given person in the given org
        /// NOTE: This does NOT do any RBAC querying
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="personRoute"></param>
        /// <returns></returns>
        public static async Task<PersonEntity> PersonWhereRoute(this IIdentityDb db, string orgRoute, string personRoute, bool allowDeleted = false, bool includeRelations = false)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            if (string.IsNullOrEmpty(personRoute))
            {
                throw new ArgumentException($"'{nameof(personRoute)}' cannot be null or empty.", nameof(personRoute));
            }

            IQueryable<PersonEntity> query = from person in db.People
                                             where
                                                   person.Route == personRoute &&
                                                   person.Organization.DeletedUtc == null &&
                                                   person.Organization.Route == orgRoute &&
                                                   person.Organization.Tenant.DeletedUtc == null
                                             select person;

            query = FilterAndIncludePerson(query, allowDeleted, includeRelations);

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Async gets the given Person by orgRoute and userRoute
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="userRoute"></param>
        /// <param name="includeRelations"></param>
        /// <returns></returns>
        public static async Task<PersonEntity> PersonWhereUserRoute(this IIdentityDb db, string orgRoute, string userRoute, bool allowDeleted = false, bool includeRelations = false)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            if (string.IsNullOrEmpty(userRoute))
            {
                throw new ArgumentException($"'{nameof(userRoute)}' cannot be null or empty.", nameof(userRoute));
            }

            // Query for user and org
            IQueryable<PersonEntity> query = from person in db.People
                                             where
                                                   person.User.Route == userRoute &&
                                                   person.Organization.DeletedUtc == null &&
                                                   person.Organization.Route == orgRoute &&
                                                   person.Organization.Tenant.DeletedUtc == null
                                             select person;

            query = FilterAndIncludePerson(query, allowDeleted, includeRelations);

            return await query.FirstOrDefaultAsync();
        }

        private static IQueryable<PersonEntity> FilterAndIncludePerson(IQueryable<PersonEntity> query, bool allowDeleted, bool includeRelations)
        {
            // Optionally filter deleted
            if (!allowDeleted)
            {
                query = query.Where(p => p.DeletedUtc == null && p.User.DeletedUtc == null);
            }

            // Optionally include relations
            if (includeRelations)
            {
                query = query.Include(p => p.User).Include(p => p.Organization);
            }

            return query;
        }

        /// <summary>
        /// Async queries for all of the active people in an organization
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="includeChildren"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<PersonEntity>> PeopleAsync(this IIdentityDb db, string orgRoute, bool includeChildren = false)
        {
            // TODO: Paging
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            IQueryable<PersonEntity> query = from person in db.People
                                             where
                                                   person.DeletedUtc == null
                                                   && person.Organization.DeletedUtc == null
                                                   && person.Organization.Route == orgRoute
                                                   && person.Organization.Tenant.DeletedUtc == null
                                                   && person.User.DeletedUtc == null
                                             select person;
            if (includeChildren)
            {
                query = query.Include(p => p.User).Include(p => p.Organization);
            }

            return await query.ToArrayAsync();
        }

        /// <summary>
        /// Async queries for a user by their universalId
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="personRoute"></param>
        /// <returns></returns>
        public static async Task<PersonEntity> PersonWhereEmail(this IIdentityDb db, string orgRoute, string userEmail, bool allowDeleted = false, bool includeRelations = false, ProviderType userIdentityProviderType = ProviderType.Unknown)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            if (string.IsNullOrEmpty(userEmail))
            {
                throw new ArgumentException($"'{nameof(userEmail)}' cannot be null or empty.", nameof(userEmail));
            }

            userEmail = userEmail.ToEmail();
            IQueryable<PersonEntity> query = from person in db.People
                                             where
                                                   person.Organization.DeletedUtc == null
                                                   && person.Organization.Tenant.DeletedUtc == null
                                                   && person.Organization.Route == orgRoute
                                                   && person.User.Email == userEmail
                                             select person;

            query = FilterAndIncludePerson(query, allowDeleted, includeRelations);

            PersonEntity ret = await query.FirstOrDefaultAsync();

            // If we didn't find anything then we try again based on alias emails
            if (ret == null && userIdentityProviderType != ProviderType.Unknown)
            {
                query = from person in db.People
                        where
                              person.Organization.DeletedUtc == null
                              && person.Organization.Tenant.DeletedUtc == null
                              && person.Organization.Route == orgRoute
                              // Instead of checking by just User.Email, we check by all the emails for one connected to this provider
                              && person.User.UserIdentities.Any(u => u.Identifier == userEmail && u.ProviderType == userIdentityProviderType)
                        select person;

                query = FilterAndIncludePerson(query, allowDeleted, includeRelations);
                ret = await query.FirstOrDefaultAsync();
            }
            return ret;
        }

        /// <summary>
        /// Async queries for the user tied to the given claims, but only if they have the given role or higher
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userUniversalId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static async Task<UserEntity> UserWhereUid(this IIdentityDb db, string userUniversalId, UserRole role = UserRole.User, bool allowDeleted = false)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (userUniversalId is null)
            {
                throw new ArgumentNullException(nameof(userUniversalId));
            }

            IQueryable<UserEntity> query = QueryUser(db, userUniversalId, role);

            if (!allowDeleted)
            {
                query = query.Where(u => u.DeletedUtc == null);
            }

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Async queries the database for the user connected to the given claims with the given minimum role
        /// NOTE: This will also return the claims user if they have God role and no association with the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="userUniversalId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static async Task<UserEntity> UserWhereUid(this IIdentityDb db, string orgRoute, string userUniversalId, PersonRole role = PersonRole.Person)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (userUniversalId is null)
            {
                throw new ArgumentNullException(nameof(userUniversalId));
            }

            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            UserEntity godUser = await db.UserWhereUid(userUniversalId, UserRole.God);
            if (godUser != null)
            {
                OrganizationEntity org = await OrgWhereRoute(db, orgRoute);
                if (org != null)
                {
                    return godUser;
                }
                else
                {
                    return null;
                }
            }

            IQueryable<UserEntity> query = from person in db.People
                                           where
                                                 person.DeletedUtc == null
                                                 && person.Organization.DeletedUtc == null
                                                 && person.Organization.Route == orgRoute
                                                 && person.Organization.Tenant.DeletedUtc == null
                                                 && person.User.DeletedUtc == null
                                                 && person.User.UniversalId == userUniversalId
                                                 && person.PersonRole >= role
                                           select person.User;
            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets the user entity for the given org and team as long as it meets or exceeds the given role
        /// NOTE: This will also return a user entity if claims are god on the system OR admin in the org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="userUniversalId"></param>
        /// <param name="memberRole"></param>
        /// <returns></returns>
        public static async Task<UserEntity> UserWhereUid(this IIdentityDb db, string orgRoute, string groupRoute, string userUniversalId, MemberRole memberRole = MemberRole.Member)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (userUniversalId is null)
            {
                throw new ArgumentNullException(nameof(userUniversalId));
            }

            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            if (string.IsNullOrEmpty(groupRoute))
            {
                throw new ArgumentException($"'{nameof(groupRoute)}' cannot be null or empty.", nameof(groupRoute));
            }

            UserEntity adminUser = await db.UserWhereUid(orgRoute, userUniversalId, PersonRole.Admin);
            if (adminUser != null)
            {
                GroupEntity grp = await GroupWhereRoute(db, orgRoute, groupRoute);
                if (grp != null)
                {
                    return adminUser;
                }
                else
                {
                    return null;
                }
            }

            IQueryable<UserEntity> query = from member in db.Members
                                           where
                                                 member.DeletedUtc == null
                                                 && member.MemberRole >= memberRole
                                                 && member.Group.DeletedUtc == null
                                                 && member.Group.Route == groupRoute
                                                 && member.Person.DeletedUtc == null
                                                 && member.Person.Organization.DeletedUtc == null
                                                 && member.Person.Organization.Route == orgRoute
                                                 && member.Person.Organization.Tenant.DeletedUtc == null
                                                 && member.Person.User.DeletedUtc == null
                                                 && member.Person.User.UniversalId == userUniversalId
                                           select member.Person.User;
            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Build a query for accessing <see cref="UserEntity"/> by <see cref="UserEntity.UniversalId"/> and optionally <see cref="UserEntity.UserRole"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userUniversalId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static IQueryable<UserEntity> QueryUser(this IIdentityDb db, string userUniversalId, UserRole role = UserRole.User)
        {
            return from user in db.Users
                   where
                         user.UniversalId == userUniversalId
                         && user.UserRole >= role
                   select user;
        }

        /// <summary>
        /// Build a query for accessing <see cref="PersonEntity"/> by the <see cref="UserEntity.UniversalId"/> field on the <see cref="PersonEntity.User"/> object; 
        /// optionally allowing deleted people and including the <see cref="PersonEntity.User"/> and <see cref="PersonEntity.Organization"/> objects
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="userUniversalId"></param>
        /// <param name="allowDeleted"></param>
        /// <param name="includeRelations"></param>
        /// <returns></returns>
        public static IQueryable<PersonEntity> QueryPerson(this IIdentityDb db, string orgRoute, string userUniversalId, bool allowDeleted = false, bool includeRelations = false)
        {
            IQueryable<PersonEntity> query = from person in db.People
                                             where
                                                   person.Organization.DeletedUtc == null &&
                                                   person.Organization.Route == orgRoute &&
                                                   person.Organization.Tenant.DeletedUtc == null &&
                                                   person.User.DeletedUtc == null &&
                                                   person.User.UniversalId == userUniversalId
                                             select person;

            query = FilterAndIncludePerson(query, allowDeleted, includeRelations);

            return query;
        }

        /// <summary>
        /// Build a <see cref="IQueryable{MemberEntity}"/> for finding the given user in the given group and given route; useful for finding arbitrary users from a group
        /// </summary>
        /// <remarks>
        /// If the given user does NOT have am authentic <see cref="PersonEntity"/> record and <see cref="MemberEntity"/> record, then this will return an empty set
        /// </remarks>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="userRoute"></param>
        /// <param name="includeMembers"></param>
        /// <returns></returns>
        public static IQueryable<MemberEntity> QueryMemberByRoute(this IIdentityDb db, string orgRoute, string groupRoute, string userRoute, bool includeMembers = false)
        {
            IQueryable<MemberEntity> query =
                from
                    m in db.Members
                where
                    m.DeletedUtc == null &&
                    m.Person.User.Route == userRoute &&
                    m.Person.DeletedUtc == null &&
                    m.Person.Organization.DeletedUtc == null &&
                    m.Person.Organization.Route == orgRoute &&
                    m.Person.Organization.Tenant.DeletedUtc == null &&
                    m.Group.DeletedUtc == null &&
                    m.Group.Route == groupRoute
                select m;

            if (includeMembers)
            {
                query = query
                    .Include(m => m.Person)
                    .ThenInclude(p => p.User);
            }

            return query;
        }

        /// <summary>
        /// Build a query for <see cref="MemberEntity"/> in the given org and group, based on the <see cref="UserEntity.UniversalId"/> field
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="userUniversalId"></param>
        /// <returns></returns>
        public static IQueryable<MemberEntity> QueryMemberByUserUniversalId(this IIdentityDb db, string orgRoute, string groupRoute, string userUniversalId)
        {
            IQueryable<MemberEntity> query =
                from
                    m in db.Members
                where
                    m.DeletedUtc == null &&
                    m.Person.User.UniversalId == userUniversalId &&
                    m.Person.DeletedUtc == null &&
                    m.Person.Organization.DeletedUtc == null &&
                    m.Person.Organization.Route == orgRoute &&
                    m.Person.Organization.Tenant.DeletedUtc == null &&
                    m.Group.DeletedUtc == null &&
                    m.Group.Route == groupRoute
                select m;

            return query;
        }

        /// <summary>
        /// Builds a query for getting all the <see cref="MemberEntity"/> objects in the given group in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="includeChildren"></param>
        /// <returns></returns>
        public static IQueryable<MemberEntity> QueryMembers(this IIdentityDb db, string orgRoute, string groupRoute, bool includeChildren = false)
        {
            IQueryable<MemberEntity> query = from
                                                m in db.Members
                                             where
                                                m.DeletedUtc == null &&
                                                m.Group.DeletedUtc == null &&
                                                m.Group.Route == groupRoute &&
                                                m.Person.DeletedUtc == null &&
                                                m.Person.Organization.DeletedUtc == null &&
                                                m.Person.Organization.Route == orgRoute &&
                                                m.Person.Organization.Tenant.DeletedUtc == null
                                             orderby
                                                m.Person.User.GivenName,
                                                m.Person.User.FamilyName
                                             select
                                                m;

            if (includeChildren)
            {
                query = query.Include(m => m.Person).ThenInclude(p => p.User);
            }

            return query;
        }

        /// <summary>
        /// Async queries the database for the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        public static async Task<OrganizationEntity> OrgWhereRoute(this IIdentityDb db, string orgRoute, bool allowDeleted = false)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            IQueryable<OrganizationEntity> query = from org in db.Organizations
                                                   where
                                                         org.Route == orgRoute
                                                   select org;

            if (!allowDeleted)
            {
                query = query.Where(o => o.DeletedUtc == null && o.Tenant.DeletedUtc == null);
            }

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Async queries for given team in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="grpRoute"></param>
        /// <returns></returns>
        public static async Task<GroupEntity> GroupWhereRoute(this IIdentityDb db, string orgRoute, string grpRoute, bool allowDeleted = false)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            if (string.IsNullOrEmpty(grpRoute))
            {
                throw new ArgumentException($"'{nameof(grpRoute)}' cannot be null or empty.", nameof(grpRoute));
            }

            IQueryable<GroupEntity> query = from
                                                grp in db.Groups
                                            where
                                                grp.Route == grpRoute &&
                                                grp.Organization.Route == orgRoute
                                            select
                                                grp;

            if (!allowDeleted)
            {
                query = query = query.Where(
                     g =>
                     g.DeletedUtc == null &&
                     g.Organization.DeletedUtc == null &&
                     g.Organization.Tenant.DeletedUtc == null);
            }

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Simplistic org lookup by route
        /// </summary>
        /// <remarks>
        /// This should only be used if <see cref="GroupWhereRoute(IIdentityDb, string, string, bool)"/> is somehow missing context; EG, a unit test
        /// </remarks>
        /// <param name="db"></param>
        /// <param name="grpRoute"></param>
        /// <returns></returns>
        public static async Task<GroupEntity> GroupWhereRoute(this IIdentityDb db, string grpRoute)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(grpRoute))
            {
                throw new ArgumentException($"'{nameof(grpRoute)}' cannot be null or empty.", nameof(grpRoute));
            }

            IQueryable<GroupEntity> query = from
                                                grp in db.Groups
                                            where
                                                grp.Route == grpRoute
                                            select
                                                grp;

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Async gets the member matching the given UniveralIds in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="userEmail"></param>
        /// <param name="groupUniversalId"></param>
        /// <param name="includeRelations">If true, then include <see cref="MemberEntity.Group"/>, <see cref="MemberEntity.Person"/>, and the person's <see cref="UserEntity"/></param>
        /// <returns></returns>
        public static async Task<MemberEntity> MemberWhereEmail(this IIdentityDb db, string orgRoute, string groupUniversalId, string userEmail, bool allowDeleted = false, bool includeRelations = false)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            if (string.IsNullOrEmpty(groupUniversalId))
            {
                throw new ArgumentException($"'{nameof(groupUniversalId)}' cannot be null or empty.", nameof(groupUniversalId));
            }

            if (string.IsNullOrEmpty(userEmail))
            {
                throw new ArgumentException($"'{nameof(userEmail)}' cannot be null or empty.", nameof(userEmail));
            }

            IQueryable<MemberEntity> query = from
                            member in db.Members
                                             where
                                                 member.Person.Organization.DeletedUtc == null &&
                                                 member.Person.Organization.Tenant.DeletedUtc == null &&
                                                 member.Person.Organization.Route == orgRoute &&
                                                 (member.Person.User.Email == userEmail || member.Person.User.UserIdentities.Any(ui => ui.Identifier == userEmail)) &&
                                                 member.Group.UniversalId == groupUniversalId
                                             select member;

            if (!allowDeleted)
            {
                query = query.Where(m => m.DeletedUtc == null);
            }

            if (includeRelations)
            {
                query = query
                    .Include(m => m.Person)
                    .ThenInclude(p => p.User)
                    .Include(m => m.Group);
            }

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Async gets a <see cref="GroupEntity"/> for the given org route and UniversalId
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupUniversalId"></param>
        /// <param name="allowDeleted"></param>
        /// <returns></returns>
        public static async Task<GroupEntity> GroupWhereUid(this IIdentityDb db, string orgRoute, string groupUniversalId, bool allowDeleted = false)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            if (string.IsNullOrEmpty(groupUniversalId))
            {
                throw new ArgumentException($"'{nameof(groupUniversalId)}' cannot be null or empty.", nameof(groupUniversalId));
            }

            IQueryable<GroupEntity> query = from grp in db.Groups
                                            where
                                                  grp.UniversalId == groupUniversalId
                                                  && grp.Organization.DeletedUtc == null
                                                  && grp.Organization.Route == orgRoute
                                                  && grp.Organization.Tenant.DeletedUtc == null
                                            select grp;
            if (!allowDeleted)
            {
                query = query.Where(g => g.DeletedUtc == null);
            }

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Async queries for all of the group IDs associated with the given user in the given org
        /// </summary>
        /// <param name="userUniversalId"></param>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        public static async Task<int[]> GroupIdsAsync(this IIdentityDb db, string orgRoute, string userUniversalId)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (userUniversalId is null)
            {
                throw new ArgumentNullException(nameof(userUniversalId));
            }

            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            IQueryable<int> groupIdQuery = from
                                            grp in db.Groups.AsNoTracking()

                                           where
                                              grp.DeletedUtc == null &&
                                              grp.Organization.DeletedUtc == null &&
                                              grp.Organization.Route == orgRoute &&
                                              grp.Organization.Tenant.DeletedUtc == null &&
                                              grp.Members.Any(
                                                  m =>
                                                     m.DeletedUtc == null &&
                                                     m.Person.DeletedUtc == null &&
                                                     m.Person.User.DeletedUtc == null &&
                                                     m.Person.User.UniversalId == userUniversalId &&
                                                     m.Person.Organization.DeletedUtc == null &&
                                                     m.Person.Organization.Tenant.DeletedUtc == null
                                                     )

                                           select grp.Id;

            int[] groupIds = await groupIdQuery.ToArrayAsync();
            return groupIds;
        }

        /// <summary>
        /// Creates an in-memory builder instance, good for feeding to test databases
        /// </summary>
        /// <returns></returns>
        public static DbContextOptionsBuilder<TDbContext> InMemoryBuilder<TDbContext>()
            where TDbContext : DbContext
        {
            SqliteConnection connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            DbContextOptionsBuilder<TDbContext> builder = new DbContextOptionsBuilder<TDbContext>().UseSqlite(connection);
            return builder;
        }

        /// <summary>
        /// Builds an <see cref="IQueryable{PersonEntity}"/> including <see cref="UserEntity"/> records for the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        public static IQueryable<PersonEntity> QueryPeopleWithUsers(this IIdentityDb db, string orgRoute)
        {
            // TODO: Paging
            IQueryable<PersonEntity> query = db.People
                        .Where(
                            p =>
                            p.DeletedUtc == null &&
                            p.User.DeletedUtc == null &&
                            p.Organization.DeletedUtc == null &&
                            p.Organization.Route == orgRoute &&
                            p.Organization.Tenant.DeletedUtc == null
                        )
                        .Include(p => p.User)
                        .OrderBy(p => p.User.GivenName);
            return query;
        }

        /// <summary>
        /// Queries all active groups in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        public static IQueryable<GroupEntity> QueryGroups(this IIdentityDb db, string orgRoute)
        {
            IQueryable<GroupEntity> query = db.Groups
                        .Where(
                            grp =>
                            grp.DeletedUtc == null &&
                            grp.Organization.DeletedUtc == null &&
                            grp.Organization.Route == orgRoute &&
                            grp.Organization.Tenant.DeletedUtc == null
                        )
                         .Distinct()
                         .OrderBy(g => g.Name);
            return query;
        }

        /// <summary>
        /// Returns a query for all group objects in the given org for the given user where their role is greater than or equal to the given role (default to unknown; or query all)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="userUniversalId"></param>
        /// <param name="minRole"></param>
        /// <returns></returns>
        public static IQueryable<GroupEntity> QueryMyGroups(this IIdentityDb db, string orgRoute, string userUniversalId, MemberRole minRole = MemberRole.Unknown)
        {
            IQueryable<GroupEntity> query = db.Members
                        .Where(
                            m =>
                            m.DeletedUtc == null &&
                            m.MemberRole >= minRole &&
                            m.Person.DeletedUtc == null &&
                            m.Person.User.DeletedUtc == null &&
                            m.Person.User.UniversalId == userUniversalId &&
                            m.Group.DeletedUtc == null &&
                            m.Group.Organization.DeletedUtc == null &&
                            m.Group.Organization.Route == orgRoute &&
                            m.Group.Organization.Tenant.DeletedUtc == null
                        )
                        .Select(m => m.Group)
                        .Distinct()
                        .OrderBy(g => g.Name);
            return query;
        }

        /// <summary>
        /// Checks the given <see cref="DbContext"/> for an entity of the given type using the given predicate, falling back to local entries if not found in the database proper
        /// </summary>
        /// <remarks>
        /// This is for situations where your target entity may have been added before calling <see cref="DbContext.SaveChanges"/> to persist it to the database (EG, when adding events versus a new object)
        /// </remarks>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="db"></param>
        /// <param name="remotePredicate"></param>
        /// <param name="localPredicate"></param>
        /// <returns></returns>
        public static async Task<TEntity> SingleLocalOrRemoteAsync<TEntity>(
            this DbContext db,
            Expression<Func<TEntity, bool>> remotePredicate,
            Func<TEntity, bool> localPredicate)
            where TEntity : class
        {
            TEntity ret = await db.Set<TEntity>().Where(remotePredicate).SingleOrDefaultAsync();
            ret ??= db.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added && e.Entity is TEntity)
                    .Select(e => e.Entity as TEntity)
                    .Where(localPredicate)
                    .SingleOrDefault();

            return ret;
        }

        /// <summary>
        /// Checks the given <see cref="DbContext"/> for an entity of the given type using the given predicate, falling back to local entries if not found in the database proper
        /// </summary>
        /// <remarks>
        /// This is for situations where your target entity may have been added before calling <see cref="DbContext.SaveChanges"/> to persist it to the database (EG, when adding events versus a new object)
        /// </remarks>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="db"></param>
        /// <param name="remotePredicate"></param>
        /// <param name="localPredicate"></param>
        /// <returns></returns>
        public static async Task<TEntity> FirstLocalOrRemoteAsync<TEntity>(
           this DbContext db,
           Expression<Func<TEntity, bool>> remotePredicate,
           Func<TEntity, bool> localPredicate)
           where TEntity : class
        {
            TEntity ret = await db.Set<TEntity>().Where(remotePredicate).FirstOrDefaultAsync();
            ret ??= db.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added && e.Entity is TEntity)
                    .Select(e => e.Entity as TEntity)
                    .Where(localPredicate)
                    .FirstOrDefault();

            return ret;
        }


        /// <summary>
        /// Async read-only get all of the orgs for the given user
        /// </summary>
        /// <param name="db"></param>
        /// <param name="claims"></param>
        /// <returns></returns>
        public static IQueryable<OrganizationEntity> QueryOrgsForUser(this IIdentityDb db, string userUniversalId)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (userUniversalId is null)
            {
                throw new ArgumentNullException(nameof(userUniversalId));
            }

            IQueryable<OrganizationEntity> query =
                db.People
                    .Where(p =>
                        p.DeletedUtc == null &&
                        p.Organization.DeletedUtc == null &&
                        p.Organization.Tenant.DeletedUtc == null &&
                        p.User.DeletedUtc == null &&
                        p.User.UniversalId == userUniversalId)
                    .Select(p => p.Organization);
            return query;
        }
    }
}
