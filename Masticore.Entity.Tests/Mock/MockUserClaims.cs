using Masticore.Security;

namespace Masticore.Entity.Tests
{
    public class MockUserClaims : SecurityContext
    {
        public MockUserClaims(UserEntity user)
        {
            Email = user.Email;
            UniversalId = user.UniversalId;
        }
    }
}
