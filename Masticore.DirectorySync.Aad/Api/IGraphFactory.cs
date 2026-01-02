using Masticore.Ad;
using Masticore.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Aad
{
    /// <summary>
    /// Factory that generates <see cref="IGraphClient"/> instances given an <see cref="OrgSyncConfig"/>
    /// </summary>
    public interface IGraphFactory
    {
        Task<IGraphClient> ClientAsync(ILogger logger, OrgSyncConfig orgConfig);
    }
}