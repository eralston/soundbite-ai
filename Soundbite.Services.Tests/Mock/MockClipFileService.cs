using Masticore;
using Masticore.Models;
using System.IO;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests.Mock
{
    public class MockClipFileService : IClipFileService
    {
        public int UploadCount { get; protected set; }
        public int DownloadCount { get; protected set; }
        public int DownloadUrlCount { get; protected set; }
        public int UploadUrlCount { get; protected set; }
        public int ExistCount { get; protected set; }
        public bool DoesClipExist { get; set; } = false;
        public int SizeCount { get; protected set; }
        public long SizeBytes { get; set; } = 1;
        public Stream File { get; set; }

        public Task<string> UploadAsync(Organization org, string sessionRoute, string promptRoute, string clipRoute, FileType fileType, Stream stream)
        {
            File = stream;
            UploadCount++;
            return Task.FromResult(nameof(UploadAsync));
        }

        public Task<string> UploadUrlAsync(Organization org, string sessionRoute, string promptRoute, string clipRoute, FileType fileType)
        {
            UploadUrlCount++;
            return Task.FromResult(nameof(UploadUrlAsync));
        }

        public Task<string> DownloadUrlAsync(Organization org, string sessionRoute, string promptRoute, string clipRoute, FileType fileType)
        {
            DownloadUrlCount++;
            return Task.FromResult(nameof(DownloadUrlAsync));
        }

        public Task<bool> ExistsAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            ExistCount++;
            return Task.FromResult(DoesClipExist);
        }

        public Task<long> SizeAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            SizeCount++;
            return Task.FromResult(SizeBytes);
        }

        public Task<Stream> DownloadAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            DownloadCount++;
            File?.Seek(0, SeekOrigin.Begin);
            return Task.FromResult(File ?? "Download".ToMemoryStream() as Stream);
        }

        public Task<Stream> DownloadAudioExtractedFromVideoClipAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> SbMediaManifestDownloadUrlAsync(string orgRoute, string clipRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> SbMediaManifestDownloadUrlAsync(string orgUid, string sessionRoute, string promptRoute, string clipRoute)
        {
            throw new System.NotImplementedException();
        }
    }
}
