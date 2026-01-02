using Masticore.Ad;
using Masticore.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Aad.Tests
{
    public class MockGraphFactory : IGraphFactory
    {
        public MockGraphFactory(MockGraphClient client = null)
        {
            Client = client ?? new MockGraphClient();
        }

        public MockGraphClient Client { get; }

        public Task<IGraphClient> ClientAsync(ILogger logger, OrgSyncConfig orgConfig)
        {
            return Task.FromResult(Client as IGraphClient);
        }
    }
}