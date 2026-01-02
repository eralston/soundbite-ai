using Masticore.Storage;
using System.IO;
using System.Threading.Tasks;

namespace Masticore.Tests
{
    public class MockBlobs : IBlobs
    {
        public Stream Stream = "Hello World".ToStream();

        public string AccountName { set; get; }

        public int DownloadCount { get; set; } = 0;
        public int DownloadUrlCount { get; set; } = 0;
        public int UploadCount { get; set; } = 0;
        public int UploadUrlCount { get; set; } = 0;
        public bool IsPublic { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        /// <summary>
        /// Set the value that download will return
        /// </summary>
        /// <param name="val"></param>
        public void SetStream(string val)
        {
            Stream = val.ToStream();
        }

        /// <summary>
        /// Set the stream value from the given file; be sure to sets its properties to copy to output directory
        /// </summary>
        /// <param name="executableRelativePath"></param>
        public void SetStreamFromFile(string executableRelativePath)
        {
            string variantPlaylist = File.ReadAllText(executableRelativePath);
            SetStream(variantPlaylist);
        }

        public Task DeleteAsync(string containerName)
        {
            // DO NOTHING
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string containerName, string blobName)
        {
            // DO NOTHING
            return Task.CompletedTask;
        }

        public Task<Stream> DownloadAsync(string containerName, string fileName)
        {
            ++DownloadCount;
            Stream.Seek(0, SeekOrigin.Begin);
            return Task.FromResult(Stream);
        }

        public Task<string> UploadAsync(string containerName, string fileName, Stream stream)
        {
            ++UploadCount;
            return Task.FromResult(nameof(UploadAsync));
        }

        public Task<string> DownloadUrlAsync(string containerName, string fileName)
        {
            ++DownloadUrlCount;
            return Task.FromResult(nameof(DownloadUrlAsync));
        }

        public Task<string> UploadUrlAsync(string containerName, string blobName)
        {
            ++UploadUrlCount;
            return Task.FromResult(nameof(UploadUrlAsync));
        }

        public Task<bool> ExistsAsync(string containerName, string blobName)
        {
            throw new System.NotImplementedException();
        }

        public Task<long> SizeAsync(string containerName, string blobName)
        {
            throw new System.NotImplementedException();
        }

        public Task CopyAsync(string sourceContainerName, string sourceBlobName, string destinationContainerName, string destinationBlobName)
        {
            throw new System.NotImplementedException();
        }

        public Task RenameAsync(string sourceContainerName, string currentBlobName, string newBlobName)
        {
            throw new System.NotImplementedException();
        }


        public Task<string> TokenAsync(string containerName, string blobName, bool isWrite = false, int durationInMinutes = 60)
        {
            throw new System.NotImplementedException();
        }
    }
}
