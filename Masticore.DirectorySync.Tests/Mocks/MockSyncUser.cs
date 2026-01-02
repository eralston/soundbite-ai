using Masticore.Models;
using Masticore.Resources;

namespace Masticore.DirectorySync.Tests.Mocks
{
    public class MockSyncUser : IUserFieldsWithAliases
    {
        public MockSyncUser(string token, IUserFields existingUser = null)
        {
            this.NewUniversalId();
            GivenName = token;
            FamilyName = $"{token}, esq";
            Phone = $"123-456-7890 #{token}";
            Title = $"Grand Admiral {token}";

            if (existingUser == null)
            {
                Email = $"{token}@geocities.com";
            }
            else
            {
                Email = existingUser.Email;
            }
        }

        public string Email { get; set; }
        public string FamilyName { get; set; }
        public string GivenName { get; set; }
        public string Phone { get; set; }
        public string Title { get; set; }
        public string UniversalId { get; set; }
        public string Upn { get; set; }
        public string[] AliasEmails { get; set; }
    }
}
