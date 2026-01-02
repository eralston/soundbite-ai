using Masticore.Models;
using Microsoft.Extensions.Logging;

namespace Masticore.DirectorySync.Tests
{
    public class MockSyncStrategyFactory : ISyncStrategyFactory
    {
        private IOrgSyncStrategy Strategy { get; }

        public MockSyncStrategyFactory(IOrgSyncStrategy strategy)
        {
            Strategy = strategy;
        }

        public IOrgSyncStrategy GetOrgStrategy(OrgSyncConfig config, ILogger logger, IOrgSyncStore orgSyncStore, OrgSyncConfig oldConfig = null)
        {
            return Strategy;
        }
    }
}
