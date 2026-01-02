using AutoMapper;
using Masticore;
using Masticore.Entity;
using Masticore.Security;
using Masticore.Services;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// <see cref="MemberService"/> with additional configuration specific to Soundbite
    /// </summary>
    public class SbMemberService : MemberService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="securityContext"></param>
        /// <param name="notifications"></param>
        /// <param name="images"></param>
        /// <param name="infrastructure"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        public SbMemberService(
            ISecurityContext securityContext,
            INotificationService notifications,
            IImageService images,
            IIdentityInfrastructure infrastructure,
            IMapper mapper,
            ILogger<SbMemberService> logger) :
            base(
                securityContext,
                notifications,
                images,
                infrastructure,
                mapper,
                logger
            )
        {
        }

        /// <summary>
        /// Reads the setting of the relevant org to determine its minimum <see cref="MemberRole"/> for inviting someone into the group
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        protected override async Task<MemberRole> MinRoleForInviteAsync(string orgRoute, string groupRoute)
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            OrgSettings settings = org.GetSettings() ?? new OrgSettings();
            return settings.Permissions.MinRoleForTeamInvite;
        }
    }
}
