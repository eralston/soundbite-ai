using AutoMapper;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// Extension methods for <see cref="Masticore.Services"/>
    /// </summary>
    public static class DbExtensions
    {
        /// <summary>
        /// Executes the given query, mapping
        /// </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <typeparam name="ResourceType"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static async Task<ResourceType[]> MapReadOnlyAsync<EntityType, ResourceType>(
            this IQueryable<EntityType> query,
            Func<EntityType, ResourceType> mapper)
            where EntityType : class, IResource
            where ResourceType : class
        {
            EntityType[] entities = await query.ReadOnly();
            entities.AssertFound();
            ResourceType[] resources = entities.Select(e => mapper(e)).ToArray();
            return resources;
        }

        /// <summary>
        /// <see cref="IQueryable{OrganizationDetails}"/> for the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        public static Task<OrganizationDetails> OrganizationDetails(this IIdentityDb db, string orgRoute)
        {
            IQueryable<OrganizationDetails> query
                = from
                    org in db.Organizations

                  where
                      org.DeletedUtc == null &&
                      org.Route == orgRoute &&
                      org.Tenant.DeletedUtc == null

                  select
                      new OrganizationDetails
                      {
                          // IResource
                          Route = org.Route,
                          CreatedUtc = org.CreatedUtc,
                          UpdatedUtc = org.UpdatedUtc,
                          // IOrganization
                          Name = org.Name,
                          Description = org.Description,
                          UniversalId = org.UniversalId,
                          ConfigJson = org.ConfigJson
                      };

            return query.ReadOnlyOne();
        }

        /// <summary>
        /// TODO: REMOVE WHEN ABLE
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Obsolete("Deprecated for perf reasons; use OrganizationDetails")]
        public static Task<OrganizationDetails_Obsolete> OrganizationDetails_Obsolete(this IIdentityDb db, string orgRoute)
        {
            IQueryable<OrganizationDetails_Obsolete> query
                = from
                    org in db.Organizations

                  where
                      org.DeletedUtc == null &&
                      org.Route == orgRoute &&
                      org.Tenant.DeletedUtc == null

                  select
                      new OrganizationDetails_Obsolete
                      {
                          // IResource
                          Route = org.Route,
                          CreatedUtc = org.CreatedUtc,
                          UpdatedUtc = org.UpdatedUtc,
                          // IOrganization
                          Name = org.Name,
                          Description = org.Description,
                          UniversalId = org.UniversalId,
                          ConfigJson = org.ConfigJson
                      };

            return query.ReadOnlyOne();
        }

        /// <summary>
        /// Read-only async retrieve <see cref="GroupDetails"/> for the given group in the given org
        /// </summary>
        /// <remarks>Does NOT load the <see cref="GroupDetails.MemberRole"/> role field, that must be done separately</remarks>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        public static async Task<GroupDetails> GroupDetailsAsync(this IIdentityDb db, string orgRoute, string groupRoute)
        {
            // Get the team
            IQueryable<GroupDetails> groupQuery =
                from
                    t in db.Groups

                where
                     // Active team by route
                     t.Route == groupRoute &&
                     t.DeletedUtc == null &&
                     t.Organization.Route == orgRoute &&
                     t.Organization.DeletedUtc == null &&
                     t.Organization.Tenant.DeletedUtc == null

                select new GroupDetails
                {
                    Route = t.Route,
                    CreatedUtc = t.CreatedUtc,
                    UpdatedUtc = t.UpdatedUtc,
                    Description = t.Description,
                    Name = t.Name
                };

            GroupDetails group = await groupQuery.ReadOnlyOne();
            Validator.EnsureFound(group, "Could not find group '{0}' in org '{1}'", groupRoute, orgRoute);

            return group;
        }


        /// <summary>
        /// Async get the given <see cref="Person"/> for the given user in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="mapper"></param>
        /// <param name="claims"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        public static async Task<Person> PersonWhereUid(this IIdentityDb db, IMapper mapper, string userUniversalId, string orgRoute)
        {
            IQueryable<Person> query =
                db.People
                    .Where(
                        p =>
                        p.DeletedUtc == null &&
                        p.User.DeletedUtc == null &&
                        p.User.UniversalId == userUniversalId &&
                        p.Organization.DeletedUtc == null &&
                        p.Organization.Route == orgRoute &&
                        p.Organization.Tenant.DeletedUtc == null
                        )
                    .Include(p => p.User)
                    .Select(p => mapper.MapSafeWith<Person>(p, p.User));

            Person person = (await query.ReadOnlyOne()).AssertFound();
            return person;
        }

        /// <summary>
        /// Async get all active orgs for the given user
        /// </summary>
        /// <param name="db"></param>
        /// <param name="claims"></param>
        /// <returns></returns>
        public static async Task<OrganizationExtended[]> MyOrganizations(this IIdentityDb db, IClaims claims)
        {
            IQueryable<OrganizationExtended> query =
                                        from
                                            person in db.People
                                        where
                                            person.DeletedUtc == null
                                            && person.User.DeletedUtc == null
                                            && person.User.UniversalId == claims.UniversalId
                                            && person.Organization.DeletedUtc == null
                                            && person.Organization.Tenant.DeletedUtc == null
                                        orderby person.Organization.Name // TODO
                                        select new OrganizationExtended
                                        {
                                            // IResource
                                            Route = person.Organization.Route,
                                            CreatedUtc = person.Organization.CreatedUtc,
                                            UpdatedUtc = person.Organization.UpdatedUtc,
                                            // IOrganizationFields
                                            UniversalId = person.Organization.UniversalId,
                                            Name = person.Organization.Name,
                                            Description = person.Organization.Description,
                                            IsAccepting = !person.IsAccepted()
                                        };

            OrganizationExtended[] orgs = await query.ReadOnly();
            return orgs;
        }

        /// <summary>
        /// Async get all active organizations
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static async Task<OrganizationExtended[]> ActiveOrganizations(this IIdentityDb db)
        {
            IQueryable<OrganizationExtended> query =
                                        from
                                            org in db.Organizations
                                        where
                                            org.DeletedUtc == null
                                            && org.Tenant.DeletedUtc == null
                                        orderby org.Name // TODO
                                        select new OrganizationExtended
                                        {
                                            // IResource
                                            Route = org.Route,
                                            CreatedUtc = org.CreatedUtc,
                                            UpdatedUtc = org.UpdatedUtc,
                                            // IOrganizationFields
                                            UniversalId = org.UniversalId,
                                            Name = org.Name,
                                            Description = org.Description,
                                            IsAccepting = false
                                        };

            OrganizationExtended[] orgs = await query.ReadOnly();
            return orgs;
        }

        /// <summary>
        /// Async return all soft deleted orgs for the given user
        /// </summary>
        /// <param name="db"></param>
        /// <param name="claims"></param>
        /// <returns></returns>
        public static async Task<OrganizationExtended[]> ArchivedOrganizations(this IIdentityDb db, IClaims claims)
        {
            IQueryable<OrganizationExtended> query =
                            from
                                person in db.People
                            where
                                person.DeletedUtc == null
                                && person.User.DeletedUtc == null
                                && person.User.UniversalId == claims.UniversalId
                                && person.Organization.DeletedUtc != null
                                && person.Organization.Tenant.DeletedUtc == null
                            orderby person.Organization.Name // TODO
                            select new OrganizationExtended
                            {
                                // IResource
                                Route = person.Organization.Route,
                                CreatedUtc = person.Organization.CreatedUtc,
                                UpdatedUtc = person.Organization.UpdatedUtc,
                                // IOrganizationFields
                                UniversalId = person.Organization.UniversalId,
                                Name = person.Organization.Name,
                                Description = person.Organization.Description,
                                IsAccepting = !person.IsAccepted()
                            };
            OrganizationExtended[] orgs = await query.ReadOnly();
            return orgs;
        }
    }
}
