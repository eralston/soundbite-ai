using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Media;
using Azure.ResourceManager.Media.Models;
using Azure.ResourceManager.Resources;
using MsAzure = Azure;

namespace Masticore.Azure.MediaServices
{
    // Permissions required: 
    //  Resource Group:
    //      Reader
    //  Media Service:
    //      Media Services Media Operator
    //      Media Services Policy Administrator

    // NOTE: I don't love how you make this and then have to set a connection on it.  I think it would make 
    // some sense to have a OrgAzureMediaServiceFactory that is reponsible for meshing up the org settings
    // with the environment settings and just handing back a ready-to-go instance.  Look at SbMediaService
    // with all the .SetConnectionInfo calls -- those could come out

    public class AzureMediaService : IAzureMediaService
    {
        #region Static Members

        /// <summary>
        /// Builds the organization's content policy key name for Azure Media Services (AMS).
        /// </summary>
        /// <param name="orgUniversalId">Univeral ID associated with the organization.</param>
        public static string BuildOrgContentPolicyKeyName(string orgUniversalId)
        {
            return $"Org-{orgUniversalId}";
        }

        /// <summary>
        /// Builds the streaming locator name for a clip.
        /// </summary>
        /// <param name="orgRoute">Route of the organization containing the session.</param>
        /// <param name="sessionRoute">Route of the session containing the prompt.</param>
        /// <param name="promptRoute">Route of the prompt containing the clip.</param>
        /// <param name="clipRoute">Route of the clip for which a streaming locator name is being built.</param>
        /// <returns>the name to use for the streaming locator for the specified clip.</returns>
        public static string BuildStreamingLocatorName(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            //NOTE: max length for a streaming locator name is 128 characters
            return $"{orgRoute}-{sessionRoute}-{promptRoute}-{clipRoute}";
        }

        /// <summary>
        /// Builds the transcription job name for a clip.
        /// </summary>
        /// <param name="orgRoute">Route of the organization containing the session.</param>
        /// <param name="sessionRoute">Route of the session containing the prompt.</param>
        /// <param name="promptRoute">Route of the prompt containing the clip.</param>
        /// <param name="clipRoute">Route of the clip for which a transcription job is being run.</param>
        /// <returns>the name to use for the transcription job name for the specified clip.</returns>
        public static string BuildTranscriptionJobName(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            return $"{orgRoute}-{sessionRoute}-{promptRoute}-{clipRoute}";
        }

        /// <summary>
        /// Builds the transcription asset name for a clip.
        /// </summary>
        /// <param name="orgRoute">Route of the organization containing the session.</param>
        /// <param name="sessionRoute">Route of the session containing the prompt.</param>
        /// <param name="promptRoute">Route of the prompt containing the clip.</param>
        /// <param name="clipRoute">Route of the clip for which a transcription job is being run.</param>
        /// <returns>the name to use for the transcription asset output name for the specified clip.</returns>
        public static string BuildTranscriptionAssetName(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            return $"Transcription-{orgRoute}-{sessionRoute}-{promptRoute}-{clipRoute}";
        }

        /// <summary>
        /// Builds the encoding job name for a clip.
        /// </summary>
        /// <param name="orgRoute">Route of the organization containing the session.</param>
        /// <param name="sessionRoute">Route of the session containing the prompt.</param>
        /// <param name="promptRoute">Route of the prompt containing the clip.</param>
        /// <param name="clipRoute">Route of the clip for which a transcription job is being run.</param>
        /// <returns>the name to use for the encoding job name for the specified clip.</returns>
        public static string BuildEncodingJobName(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            return $"Encode-{orgRoute}-{sessionRoute}-{promptRoute}-{clipRoute}";
        }

        /// <summary>
        /// Builds the encoding job asset output name for a clip.
        /// </summary>
        /// <param name="orgRoute">Route of the organization containing the session.</param>
        /// <param name="sessionRoute">Route of the session containing the prompt.</param>
        /// <param name="promptRoute">Route of the prompt containing the clip.</param>
        /// <param name="clipRoute">Route of the clip for which a transcription job is being run.</param>
        /// <returns>the name to use for the encoding job Asset output name for the specified clip.</returns>
        public static string BuildEncodingAssetOutputName(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            return $"Video-{orgRoute}-{sessionRoute}-{promptRoute}-{clipRoute}";
        }

        #endregion

        #region Fields

        ArmClient? _client;
        SubscriptionResource? _subscription;
        ResourceGroupResource? _resourceGroup;
        MediaServicesAccountResource? _mediaService;

        #endregion

        #region Properties

        private ArmClient Client => _client ?? throw new NullReferenceException("Azure Resource Manager (ARM) Client is null.");

        private SubscriptionResource Subscription => _subscription ?? throw new NullReferenceException("Azure Resource Manager (ARM) Subscription is null.");

        private ResourceGroupResource ResourceGroup => _resourceGroup ?? throw new NullReferenceException("Azure Resource Manager (ARM) ResourceGroup is null.");

        private MediaServicesAccountResource MediaService => _mediaService ?? throw new NullReferenceException("Azure Resource Manager (ARM) MediaService is null.");

        #endregion

        #region Methods - Setup

        public void SetConnectionInfo(string subscriptionId, string resourceGroupName, string mediaServiceAccountName)
        {
            UseSubscription(subscriptionId);
            UseResourceGroup(resourceGroupName);
            UseMediaServiceAccount(mediaServiceAccountName);
        }

        private static ArmClient GetArmClient(string? subscriptionId = null)
        {
            //NOTE: Connections to the Azure Resource Manager (ARM) client are established using the
            //      DefaultAzureCredential which runs as the current user principal. Visual Studio
            //      uses the azure account associated with the developer.  When running live Azure
            //      uses the service principal of the App Service / Azure Function / etc.  

            //NOTE: if you want to test custom permissions you can setup an app registration. At the
            //      time of this note there was an AmsPermsTest app registration setup for testing.
            //      You can connect with the permissions associated with that app registration using
            //      the following code (assuming it has not changed in Azure)
            //      ClientSecretCredential appCreds = new ClientSecretCredential("[AMS TENANT ID]", "17e2fa71-f483-42d4-abd6-ce970366b3bc", "lcM8Q~RLoXlYqwmA1LQjwowb4AxXavkCeIAlKcgS");

            TokenCredential creds = new DefaultAzureCredential();
            ArmClient client = string.IsNullOrEmpty(subscriptionId?.Trim())
                ? new ArmClient(creds)
                : new ArmClient(creds, subscriptionId);
            return client;
        }

        private void UseSubscription(string subscriptionId)
        {
            _client = _client ?? GetArmClient(subscriptionId);
            _subscription = _client.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));
            _resourceGroup = null;
            _mediaService = null;
        }

        private void UseResourceGroup(string resourceGroupName)
        {
            _resourceGroup = Client.GetResourceGroupResource(new ResourceIdentifier($"{Subscription.Id}/resourceGroups/{resourceGroupName}"));
            _mediaService = null;
        }

        private void UseMediaServiceAccount(string mediaServiceAccountName)
        {
            _mediaService = Client.GetMediaServicesAccountResource(new ResourceIdentifier($"{ResourceGroup.Id}/providers/Microsoft.Media/mediaservices/{mediaServiceAccountName}"));
        }

        #endregion

        #region Methods

        public Task<ContentKeyPolicyResource> AddOrUpdateOrgContentKeyPolicy(string name, string description, string issuer, string audience, byte[] orgEncryptionKey)
        {
            ContentKeyPolicyCollection contentKeyPolicies = MediaService.GetContentKeyPolicies();
            ContentKeyPolicySymmetricTokenKey tokenKey = new ContentKeyPolicySymmetricTokenKey(orgEncryptionKey);
            ContentKeyPolicyTokenRestriction restriction = new ContentKeyPolicyTokenRestriction(issuer, audience, tokenKey, ContentKeyPolicyRestrictionTokenType.Jwt);
            ContentKeyPolicyClearKeyConfiguration configuration = new ContentKeyPolicyClearKeyConfiguration() { };
            ContentKeyPolicyData contentKeyPolicyData = new ContentKeyPolicyData()
            {
                Description = description
            };
            contentKeyPolicyData.Options.Add(new ContentKeyPolicyOption(configuration, restriction) { Name = "Soundbite Org Token" });
            ArmOperation<ContentKeyPolicyResource> response = contentKeyPolicies.CreateOrUpdate(MsAzure.WaitUntil.Completed, name, contentKeyPolicyData);

            if (response.Value == null)
            {
                throw new Exception("Failed to add/update organization content key policy");
            }

            return Task.FromResult(response.Value);
        }

        public async Task<MediaAssetResource?> GetAsset(string assetName)
        {
            try
            {
                MsAzure.Response<MediaAssetResource> response = await MediaService.GetMediaAssetAsync(assetName);
                return response.Value;
            }
            catch (MsAzure.RequestFailedException reqFailedEx)
            {
                if (reqFailedEx.Status == 404)
                {
                    return null;
                }
                throw;
            }
        }

        public async Task<MediaAssetResource> AddOrUpdateAsset(string assetName, string? description = null, string? alternateId = null)
        {
            MediaAssetCollection assets = MediaService.GetMediaAssets();
            ArmOperation<MediaAssetResource> response = await assets.CreateOrUpdateAsync(MsAzure.WaitUntil.Completed, assetName, new MediaAssetData()
            {
                AlternateId = alternateId,
                Description = description
            });
            return response.Value;
        }

        public async Task DeleteAsset(string assetName)
        {
            MediaAssetResource? asset = await GetAsset(assetName);
            if (asset != null)
            {
                await asset.DeleteAsync(MsAzure.WaitUntil.Completed);
            }
        }

        public async Task<MediaTransformResource?> GetTransformAsync(string transformName)
        {
            try
            {
                MsAzure.Response<MediaTransformResource> response = await MediaService.GetMediaTransformAsync(transformName);
                return response.Value;
            }
            catch (MsAzure.RequestFailedException reqFailedEx)
            {
                if (reqFailedEx.Status == 404)
                {
                    return null;
                }
                throw;
            }

        }

        public async Task<MediaJobResource> CreateJob(string jobName, string transformName, string inputAssetName, string outputAssetName, string? description = null, bool createOutputAsset = true)
        {
            // Aquire and ensure the input asset exists
            MediaAssetResource? inputAsset = await GetAsset(inputAssetName);
            if (inputAsset?.Data == null)
            {
                throw new Exception($"Cannot create job named '{jobName}' for transform named '{transformName}' because the input asset does not exist.");
            }

            // Aquire and ensure the output asset exists
            MediaAssetResource? outputAsset = createOutputAsset
                ? await AddOrUpdateAsset(outputAssetName)
                : await GetAsset(outputAssetName);
            if (outputAsset?.Data == null)
            {
                throw new Exception($"Cannot create job named '{jobName}' for transform named '{transformName}' because the output asset does not exist.");
            }

            // Get the transform
            MediaTransformResource? transform = await GetTransformAsync(transformName);
            if (transform == null)
            {
                throw new Exception($"Transform '{transformName}' not found in Media Service '{MediaService.Data.Name}'");
            }

            // Create the job
            ArmOperation<MediaJobResource> result = await transform.GetMediaJobs().CreateOrUpdateAsync(MsAzure.WaitUntil.Completed, jobName, new MediaJobData()
            {
                Priority = MediaJobPriority.Normal,
                Description = description,
                Input = new MediaJobInputAsset(inputAsset.Data.Name),
                Outputs = { new MediaJobOutputAsset(outputAsset.Data.Name) }
            });

            // Verify the job was created successfully
            if (!result.HasValue || result.Value == null)
            {
                throw new Exception($"Failed to create job named '{jobName}' for transform named '{transformName}'.");
            }

            return result.Value;
        }

        public async Task<MediaJobResource?> GetJob(string transformName, string jobName)
        {
            try
            {
                MediaTransformResource? transform = await GetTransformAsync(transformName);
                if (transform == null)
                {
                    throw new Exception($"Cannot get job named '{jobName}' because transform named '{transformName}' was not found in media service named '{MediaService.Data.Name}'");
                }
                MsAzure.Response<MediaJobResource> response = await transform.GetMediaJobAsync(jobName);
                return response.Value;
            }
            catch (MsAzure.RequestFailedException reqFailedEx)
            {
                if (reqFailedEx.Status == 404)
                {
                    return null;
                }
                throw;
            }
        }

        public async Task DeleteJob(string transformName, string jobName)
        {
            MediaJobResource? job = await GetJob(transformName, jobName);
            if (job != null)
            {
                await job.DeleteAsync(MsAzure.WaitUntil.Completed);
            }
        }

        public async Task<StreamingLocatorResource> AddOrUpdateStreamingLocator(string streamingLocatorName, string assetName, string streamingPolicyName, string? contentKeyPolicyName)
        {
            ArmOperation<StreamingLocatorResource> response = await MediaService.GetStreamingLocators().CreateOrUpdateAsync(MsAzure.WaitUntil.Completed, streamingLocatorName,
                new StreamingLocatorData()
                {
                    AssetName = assetName,
                    StreamingPolicyName = streamingPolicyName,
                    DefaultContentKeyPolicyName = contentKeyPolicyName
                });

            if (!response.HasValue || response.Value == null)
            {
                throw new Exception($"Failed to create Streaming Locator named '{streamingLocatorName}' for asset '{assetName}'");
            };

            return response.Value;
        }

        public async Task<StreamingLocatorResource?> GetStreamingLocator(string streamingLocatorName)
        {
            try
            {
                MsAzure.Response<StreamingLocatorResource> response = await MediaService.GetStreamingLocatorAsync(streamingLocatorName);
                return response.Value;
            }
            catch (MsAzure.RequestFailedException reqFailedEx)
            {
                if (reqFailedEx.Status == 404)
                {
                    return null;
                }
                throw;
            }
        }

        public async Task DeleteStreamingLocator(string streamingLocatorName)
        {
            StreamingLocatorResource? streamingLocator = await GetStreamingLocator(streamingLocatorName);
            if (streamingLocator != null)
            {
                await streamingLocator.DeleteAsync(MsAzure.WaitUntil.Completed);
            }
        }

        #endregion
    }
}