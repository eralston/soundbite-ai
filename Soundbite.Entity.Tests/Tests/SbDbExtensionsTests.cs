using Masticore.Entity;
using Masticore.Entity.Tests;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Entity.Tests
{
    public class SbDbExtensionsTests : ResourceTestBase<SessionEntity, SbDb, MockSbDbInfrastructure>
    {
        [Fact]
        public async Task PeopleInSessionGroupsAsync()
        {
            SbDb db = Builder.DbContext.Value;

            IEnumerable<PersonEntity> result = await db.PeopleInSessionGroupsAsync(SbDbSeed.SessionRoute);
            Assert.Equal(5, result.Count());

            result = await db.PeopleInSessionGroupsAsync(SbDbSeed.NonMemberSessionRoute);
            Assert.Equal(2, result.Count());
        }
    }
}
