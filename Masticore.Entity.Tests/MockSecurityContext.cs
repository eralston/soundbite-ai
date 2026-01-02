using Masticore.Models;
using Masticore.Security;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.Entity.Tests
{
    public class MockSecurityContext : SecurityContext
    {
        #region Static Methods

        /// <summary>
        /// Creates a <see cref="MockSecurityContext"/> based on the default user in the DB
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static MockSecurityContext CreateAndLoad(IIdentityDb db)
        {
            // Created context MUST have a real user in the DB
            MockSecurityContext ctx = new MockSecurityContext();
            UserEntity userEnt = db.Users.Where(u => u.UniversalId == ctx.Claims.UniversalId).Single();
            User user = new User();
            MockMapper.Instance.Map(userEnt, user);
            ctx.Init(user, userEnt.Id);
            return ctx;
        }

        #endregion

        #region Constructor(s)
        /// <summary>
        /// Creates a new <see cref="MockSecurityContext"/> instance. The base class only needs
        /// service references to handle the current tenant value, which can easily be assigned
        /// in a unit test.
        /// </summary>
        public MockSecurityContext() : base()
        {
            AllowChanges = true;
        }

        public MockSecurityContext(bool randomUser = false, bool randomDirectory = false)
            : this()
        {
            CurrentUserId = int.MinValue;
            FullName = "Mock Claims";
            DirectoryUniversalId = "Mock Directory";
            Claims = new MockClaims(randomUser, randomDirectory);
            UniversalId = Claims.UniversalId;
            DirectoryUniversalId = Claims.DirectoryUniversalId;
        }

        public MockSecurityContext(User user, int userId)
            : this()
        {
            Init(user, userId);
        }

        #endregion

        public void Init(User user, int userId)
        {
            CurrentUser = user;
            CurrentUserId = userId;
            Email = user.Email;
            UniversalId = user.UniversalId;
        }

        public override Task<Tenant> CurrentTenant()
        {
            return Task.FromResult(_currentTenant);
        }
    }
}
