using AutoMapper;
using Masticore.Models;
using Masticore.Security;
using Masticore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// EF Implementation of <see cref="ISessionService"/>
    /// </summary>
    public class OktaDataService : InfrastructureServiceBase<ISbInfrastructure>, IOktaDataService
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="OktaDataService"/> instance.
        /// </summary>
        /// <param name="securityContext">DI reference to the security context.</param>
        /// <param name="infrastructure">DI reference to the soundbite infrastructure.</param>
        /// <param name="mapper">DI reference to an object mapper.</param>
        /// <param name="logger">DI reference to a logger.</param>
        public OktaDataService(
            ISecurityContext securityContext,
            ISbInfrastructure infrastructure,
            IMapper mapper,
            ILogger<SessionService> logger)
            : base(infrastructure, logger, securityContext, mapper)
        {
        }

        #endregion

        #region IOktaDataService Implementation

        /// <inheritdoc />
        public async Task<OrgOktaSettingsWithOrgRoute> GetOktaSettingsByEmail(string email)
        {
            OrgOktaSettingsWithOrgRoute result = null;
            SbDb db = await Infrastructure.DbAsync();

            // Acquire any organizations with which the user is associated that are not deleted
            // and have a non-empty organization JSON configuration
            var associatedOrgs = await db.People
                .Where(i => i.User.Email == email
                    && i.User.DeletedUtc == null
                    && i.DeletedUtc == null
                    && i.Organization.DeletedUtc == null
                    && i.Organization.Tenant.DeletedUtc == null)
                .Select(i => new { i.Organization.Route, i.Organization.ConfigJson })
                .ToListAsync();

            // Iterate over the associated organizations and parse out the configuration to
            // determine if there are any valid OKTA settings to return
            foreach (var org in associatedOrgs)
            {
                if (result == null && !string.IsNullOrEmpty(org.ConfigJson))
                {
                    try
                    {
                        OrgSettings orgSettings = Newtonsoft.Json.JsonConvert
                            .DeserializeObject<OrgSettings>(org.ConfigJson);
                        if (!string.IsNullOrEmpty(orgSettings.Okta?.ClientId) && !string.IsNullOrEmpty(orgSettings.Okta?.IssuerUrl))
                        {
                            result = new OrgOktaSettingsWithOrgRoute()
                            {
                                ClientId = orgSettings.Okta.ClientId,
                                IssuerUrl = orgSettings.Okta.IssuerUrl,
                                OrgRoute = org.Route
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"Failed to parse Organization settings for OrgRoute '{org.Route}'");
                    }
                }
            }

            // Return the OKTA settings if they were found (or null if not)
            return result;
        }

        /// <inheritdoc />
        public async Task<OrgOktaSettingsWithOrgRoute> GetOktaSettingsByOrgRoute(string orgRoute)
        {
            OrgOktaSettingsWithOrgRoute result = null;
            SbDb db = await Infrastructure.DbAsync();

            // Acquire any organizations with which the user is associated that are not deleted
            // and have a non-empty organization JSON configuration
            var org = await db.Organizations
                .Where(i => i.Route == orgRoute
                    && i.DeletedUtc == null
                    && i.Tenant.DeletedUtc == null)
                .Select(i => new { i.Route, i.ConfigJson })
                .FirstOrDefaultAsync();


            if (!string.IsNullOrEmpty(org.ConfigJson))
            {
                try
                {
                    OrgSettings orgSettings = Newtonsoft.Json.JsonConvert
                        .DeserializeObject<OrgSettings>(org.ConfigJson);
                    if (!string.IsNullOrEmpty(orgSettings.Okta?.ClientId) && !string.IsNullOrEmpty(orgSettings.Okta?.IssuerUrl))
                    {
                        result = new OrgOktaSettingsWithOrgRoute()
                        {
                            ClientId = orgSettings.Okta.ClientId,
                            IssuerUrl = orgSettings.Okta.IssuerUrl,
                            OrgRoute = org.Route
                        };
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Failed to parse Organization settings for OrgRoute '{org.Route}'");
                }
            }

            // Return the OKTA settings if they were found (or null if not)
            return result;
        }

        //TODO: Refactor stuff so we do not need this in the future
        public async Task REFACTOR_SetRequiredInfoOnUser(User user, string universalId)
        {
            SbDb db = await Infrastructure.DbAsync();
            Masticore.Entity.UserEntity userEntity = await db.Users.Where(i => i.Route == user.Route).FirstOrDefaultAsync();
            if (userEntity != null)
            {
                userEntity.UniversalId = userEntity.UniversalId ?? universalId;
                userEntity.InviteAcceptUtc = userEntity.InviteAcceptUtc ?? DateTime.UtcNow;
                db.Update(userEntity);
                await db.SaveChangesAsync();
            }
        }

        #endregion
    }
}