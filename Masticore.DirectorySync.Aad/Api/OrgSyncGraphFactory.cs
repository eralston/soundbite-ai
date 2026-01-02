using Masticore.Ad;
using Masticore.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Aad
{
    /// <summary>
    /// Implementation of <see cref="IGraphFactory"/> transforming an <see cref="OrgSyncConfig"/> into credentials
    /// </summary>
    public class OrgSyncGraphFactory : IGraphFactory
    {
        public Task<IGraphClient> ClientAsync(ILogger logger, OrgSyncConfig orgConfig)
        {
            AadOrgConfig aadConfig = orgConfig.GetSyncConfig<AadOrgConfig>();
            GraphClient client = new GraphClient(logger, aadConfig.TenantId, aadConfig.GetAppId(), aadConfig.GetAppSecret());
            return Task.FromResult(client as IGraphClient);
        }
    }
}
