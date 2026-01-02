using Masticore.Graph;
using Masticore.Models;
using Masticore.Storage;
using System.IO;
using System.Threading.Tasks;

namespace Masticore.Tests
{
    public class MockGraph : IGraph
    {
        private bool ThrowException { get; set; }

        public MockGraph(bool throwException = false)
        {
            ThrowException = throwException;
        }

        public Task<Stream> MyPhotoAsync()
        {
            return Task.FromResult("Face".ToStream());
        }

        public Task<User> MyUserAsync()
        {
            if (ThrowException)
            {
                throw new System.Exception("Throw because ThrowException is true");
            }

            User fields = new User()
            {
                GivenName = "Test",
                FamilyName = "User",
                Email = "test@soundbite.ai",
                Phone = "123-456-7890",
                Title = "Mr. Manager",
            };
            return Task.FromResult(fields);
        }
    }
}