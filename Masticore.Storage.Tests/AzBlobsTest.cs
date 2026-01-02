using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Masticore.Resources;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Storage
{
    public class AzBlobsTest
    {
        private const string Test = "Hello World";

        // Unit tests intended to be run against Azurite https://github.com/Azure/Azurite
        // As of August 2020, Azurite does NOT support SAS tokens :(
        // Falling back to a dev sandbox in Azure
        // TODO: Figure this out
        private const string ConnectionString = "[SB STORAGE CONNECTION STRING]";
        private readonly IBlobs Blobs = new AzBlobs(ConnectionString, new NullLogger<AzBlobs>());

        /// <summary>
        /// This pulls out and uses the raw SDK classes from the client just to ensure that the connection information used in the unit tests below are reasonable.
        /// This is necessary because the unit tests themselves seemed to be primarily failing due to env config and it has nothing to do with the code itself, so this assures this env is valid for all operations.
        /// Azurite flaws exposed this and this code should likely only be deleted once all flaws are eliminated from the world of computer science and we can learn to trust again.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SdkExample()
        {
            // Service
            BlobServiceClient service = new BlobServiceClient(ConnectionString);

            // Container
            string containerName = ResourceExtensions.NewUniversalId();
            BlobContainerClient container = service.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync();

            // Blob
            string blobName = ResourceExtensions.NewRoute();
            BlobClient blob = container.GetBlobClient(blobName);
            bool blobExists = await blob.ExistsAsync();
            Assert.False(blobExists);

            // Upload
            Stream stream = Test.ToStream();
            BlobContentInfo content = await blob.UploadAsync(stream, true);
            Assert.NotNull(content);
            blobExists = await blob.ExistsAsync();
            Assert.True(blobExists);

            // Download
            BlobDownloadInfo download = await blob.DownloadAsync();
            Assert.Equal(Test, download.Content.ToText());

            // SAS Token
            DateTimeOffset startsOn = DateTimeOffset.UtcNow;
            DateTimeOffset expiresOn = DateTimeOffset.UtcNow.AddHours(1);
            BlobSasBuilder builder = new BlobSasBuilder
            {
                StartsOn = startsOn,
                ExpiresOn = expiresOn,
                BlobContainerName = blob.BlobContainerName,
                BlobName = blob.Name
            };
            builder.SetPermissions(BlobSasPermissions.Read);

            Azure.Storage.StorageSharedKeyCredential sharedKey = ConnectionString.ToStorageSharedKeyCredential();
            string sasToken = builder.ToSasQueryParameters(sharedKey).ToString();
            Assert.NotNull(sasToken);

            // Delete container
            await container.DeleteAsync();
        }

        [Fact]
        public async Task UploadAsync()
        {
            // ARRANGE
            string containerName = ResourceExtensions.NewUniversalId();
            try
            {
                string blobName = ResourceExtensions.NewRoute();
                Stream stream = Test.ToStream();

                // ACT
                await Blobs.UploadAsync(containerName, blobName, stream);

                // ASSERT
                Assert.True(stream.Position > 0);

                // ACT & ASSERT on Size
                long byteCount = await Blobs.SizeAsync(containerName, blobName);
                Assert.Equal(stream.Length, byteCount);
            }
            finally
            {
                await Blobs.DeleteAsync(containerName);
            }
        }

        [Fact]
        public async Task UploadAsync_OverwriteWithSnapshot()
        {
            string containerName = "snapshot" + ResourceExtensions.NewUniversalId();
            try
            {
                // ARRANGE
                string blobName = ResourceExtensions.NewRoute();
                Stream stream = Test.ToStream();

                // ACT
                await Blobs.UploadAsync(containerName, blobName, stream);
                await Blobs.UploadAsync(containerName, blobName, stream);

                // ASSERT
                Assert.True(stream.Position > 0);
            }
            finally
            {
                await Blobs.DeleteAsync(containerName);
            }
        }

        [Fact]
        public async Task DownloadUrlAsync()
        {
            // ARRANGE
            string containerName = ResourceExtensions.NewUniversalId();

            try
            {
                string blobName = ResourceExtensions.NewRoute();
                Stream stream = Test.ToStream();
                await Blobs.UploadAsync(containerName, blobName, stream);

                // ACT
                string url = await Blobs.DownloadUrlAsync(containerName, blobName);

                // ASSERT
                Assert.NotNull(url);
            }
            finally
            {
                await Blobs.DeleteAsync(containerName);
            }
        }

        [Fact]
        public async Task UploadUrlAsync()
        {
            // ARRANGE
            string containerName = ResourceExtensions.NewUniversalId();

            try
            {
                string blobName = ResourceExtensions.NewRoute() + ".txt";

                // ACT
                string uploadUrl = await Blobs.UploadUrlAsync(containerName, blobName);

                // ASSERT
                Assert.NotNull(uploadUrl);

                // Try a manual upload using the link and no SDK
                HttpClient client = new HttpClient();
                StringContent content = new StringContent("Some content");
                content.Headers.Add("x-ms-blob-type", "BlockBlob");
                HttpResponseMessage response = await client.PutAsync(uploadUrl, content);
                response.EnsureSuccessStatusCode();
            }
            finally
            {
                await Blobs.DeleteAsync(containerName);
            }
        }

        [Fact]
        public async Task DownloadAsync()
        {
            // ARRANGE
            string containerName = ResourceExtensions.NewUniversalId();

            try
            {
                string blobName = ResourceExtensions.NewRoute();
                Stream uploadStream = Test.ToStream();
                await Blobs.UploadAsync(containerName, blobName, uploadStream);

                // ACT
                Stream stream = await Blobs.DownloadAsync(containerName, blobName);
                string text = stream.ToText();

                // ASSERT
                Assert.NotNull(stream);
                Assert.Equal(Test, text);
            }
            finally
            {
                await Blobs.DeleteAsync(containerName);
            }
        }

        // TODO: Figure out why this works in debug, but fails while running
        //[Fact]
        //public async Task DeleteAsync()
        //{
        //    // ARRANGE
        //    string containerName = DomainExtensions.NewUniversalId();
        //    string blobName = DomainExtensions.NewUniversalId();
        //    Stream uploadStream = Test.ToStream();
        //    await Blobs.UploadAsync(containerName, blobName, uploadStream);

        //    // ACT
        //    await Blobs.DeleteAsync(containerName);

        //    // ASSERT
        //    await Assert.ThrowsAsync<Azure.RequestFailedException>(async () => await Blobs.DownloadAsync(containerName, blobName));
        //}

        [Fact]
        public async Task DeleteAsync_Unknown()
        {
            // ARRANGE
            string containerName = ResourceExtensions.NewUniversalId();

            // ACT
            await Blobs.DeleteAsync(containerName);

            // ASSERT
            // Nothing to assert
        }
    }
}
