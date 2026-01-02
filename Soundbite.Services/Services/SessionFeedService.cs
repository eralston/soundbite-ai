using AutoMapper;
using Masticore;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Security;
using Masticore.Services;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// <see cref="ISessionFeedService"/> over EF
    /// </summary>
    public class SessionFeedService : InfrastructureServiceBase<ISbInfrastructure>, ISessionFeedService
    {
        #region Support Methods

        /// <summary>
        /// Gets the <see cref="IRbac"/> object for this service, which controls access
        /// </summary>
        protected IRbac Rbac { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SessionFeedService(
            IRbac rbac,
            ISbInfrastructure infrastructure,
            ILogger<SessionFeedService> logger,
            IMapper mapper
            )
            : base(infrastructure, logger, null, mapper)
        {
            Rbac = rbac ?? throw new System.ArgumentNullException(nameof(rbac));
        }

        #endregion

        #region ISessionFeedService

        public async Task<IndexPageResponse<SessionPreview>> ReadPendingAsync(string orgRoute, IndexPageRequest page = null)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Person);
            string userUniversalId = Rbac.UniversalIdForCurrentUser;

            SbDb db = await Infrastructure.DbAsync();
            IQueryable<SessionPreview> query = db.QueryPendingOrgSessions(orgRoute, userUniversalId, Mapper);
            IndexPageResponse<SessionPreview> ret = await query.ReadPageResponseAsync(page, (p) => p.Name.Contains(page.Filter.ToLower()));
            return ret;
        }

        public async Task<IndexPageResponse<SessionPreview>> ReadPendingAsync(string orgRoute, string groupRoute, IndexPageRequest page = null)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), groupRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, groupRoute, MemberRole.Member);
            string userUniversalId = Rbac.UniversalIdForCurrentUser;

            SbDb db = await Infrastructure.DbAsync();
            IQueryable<SessionPreview> query = db.QueryPendingGroupSessions(orgRoute, groupRoute, userUniversalId, Mapper);
            IndexPageResponse<SessionPreview> ret = await query.ReadPageResponseAsync(page, (p) => p.Name.Contains(page.Filter.ToLower()));
            return ret;
        }

        public async Task<IndexPageResponse<SessionPreview>> ReadPastAsync(string orgRoute, IndexPageRequest page = null)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Person);
            string userUniversalId = Rbac.UniversalIdForCurrentUser;

            SbDb db = await Infrastructure.DbAsync();
            IQueryable<SessionPreview> query = db.QueryPastOrgSessions(orgRoute, userUniversalId, Mapper);
            IndexPageResponse<SessionPreview> ret = await query.ReadPageResponseAsync(page, (p) => p.Name.Contains(page.Filter.ToLower()));
            return ret;
        }

        public async Task<IndexPageResponse<SessionPreview>> ReadPublishedAsync(string orgRoute, string groupRoute, IndexPageRequest page = null)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), groupRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, groupRoute, MemberRole.Member);

            SbDb db = await Infrastructure.DbAsync();
            IQueryable<SessionPreview> query = db.QueryPublishedGroupSessions(orgRoute, groupRoute, Rbac.UniversalIdForCurrentUser, Mapper);
            IndexPageResponse<SessionPreview> ret = await query.ReadPageResponseAsync(page, (p) => p.Name.Contains(page.Filter.ToLower()));
            return ret;
        }

        #endregion
    }
}
