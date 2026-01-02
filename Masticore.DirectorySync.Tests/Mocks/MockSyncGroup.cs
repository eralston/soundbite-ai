using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;

namespace Masticore.DirectorySync.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of <see cref="IGroupFields"/> for testing <see cref="ISyncStore"/> et al
    /// </summary>
    public class MockSyncGroup : IGroupFields
    {
        public MockSyncGroup()
        {
            this.NewUniversalId();
        }

        public MockSyncGroup(GroupEntity grp)
        {
            UniversalId = grp.UniversalId;
            Name = grp.Name;
            Description = grp.Description;
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public string UniversalId { get; set; }
    }
}
