using Masticore.Resources;
using Masticore.Security;

namespace Masticore.Entity.Tests
{
    public class MockClaims : IClaims
    {
        public MockClaims(bool randomUser = false, bool randomDirectory = false)
        {
            Email = $"{ResourceExtensions.NewRoute()}@{ResourceExtensions.NewRoute()}.com";

            if (randomUser)
            {
                UniversalId = ResourceExtensions.NewUniversalId();
            }

            if (randomDirectory)
            {
                DirectoryUniversalId = ResourceExtensions.NewUniversalId();
            }
        }

        public string Email { get; set; } = null;

        public string FullName { get; set; } = "Mock Claims";

        public string DirectoryUniversalId { get; set; } = "Mock Directory";

        public string UniversalId { get; set; } = IdentityDbSeed.UserOrgAdmin.UniversalId;

        public ProviderType ProviderType => ProviderType.AAD;
    }
}
