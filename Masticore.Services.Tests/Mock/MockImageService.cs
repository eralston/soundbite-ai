using Masticore.Models;
using Masticore.Resources;
using System.IO;
using System.Threading.Tasks;

namespace Masticore.Services.Tests
{
    public class MockImageService : IImageService
    {
        public int OrgImageCount { get; set; } = 0;
        public int UploadOrgImageCount { get; set; } = 0;
        public int UploadUserImageCount { get; set; } = 0;
        public int UserImageCount { get; set; } = 0;

        public bool NoCalls => OrgImageCount == 0
                    && UploadOrgImageCount == 0
                    && UploadUserImageCount == 0
                    && UserImageCount == 0;

        public Task<string> OrgImageAsync(Organization org)
        {
            OrgImageCount++;
            return Task.FromResult("SomeOrg.png");
        }

        public Task<string> UploadOrgImageAsync(Organization org, Stream stream)
        {
            UploadOrgImageCount++;
            return Task.FromResult("SomeOrg.png");
        }

        public Task<string> UploadUserImageAsync(IUniversal user, Stream stream)
        {
            UploadUserImageCount++;
            return Task.FromResult("SomeUser.png");
        }

        public Task<string> UserImageAsync(IUniversal user)
        {
            UserImageCount++;
            return Task.FromResult("SomeUser.png");
        }
    }
}
