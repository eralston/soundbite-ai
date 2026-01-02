using Azure.ResourceManager.Media;

namespace Masticore.Azure.MediaServices
{
    public interface IAzureMediaService
    {
        void SetConnectionInfo(string subscriptionId, string resourceGroupName, string mediaServiceAccountName);
        Task<ContentKeyPolicyResource> AddOrUpdateOrgContentKeyPolicy(string name, string description, string issuer, string audience, byte[] orgEncryptionKey);
        Task<MediaAssetResource?> GetAsset(string assetName);
        Task<MediaAssetResource> AddOrUpdateAsset(string assetName, string? description = null, string? alternateId = null);
        Task DeleteAsset(string assetName);
        Task<MediaTransformResource?> GetTransformAsync(string transformName);
        Task<MediaJobResource> CreateJob(string jobName, string transformName, string inputAssetName, string outputAssetName, string? description = null, bool createOutputAsset = true);
        Task<MediaJobResource?> GetJob(string transformName, string jobName);
        Task DeleteJob(string transformName, string jobName);
        Task<StreamingLocatorResource> AddOrUpdateStreamingLocator(string streamingLocatorName, string assetName, string streamingPolicyName, string? contentKeyPolicyName);
        Task<StreamingLocatorResource?> GetStreamingLocator(string streamingLocatorName);
        Task DeleteStreamingLocator(string streamingLocatorName);
    }
}