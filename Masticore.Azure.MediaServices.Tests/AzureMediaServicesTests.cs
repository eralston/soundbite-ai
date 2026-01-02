using Azure.ResourceManager.Media;
using Masticore.Token;

namespace Masticore.Azure.MediaServices.Tests
{
    public class AzureMediaServicesTests
    {
        #region Constructor

        public IAzureMediaServiceConfig GetConfig()
        {
            // TEST
            //return new AzureMediaServiceConfig()
            //{
            //    TenantId = "[AMS TENANT ID]",
            //    SubscriptionId = "[AMS SUB ID]",
            //    ResourceGroupName = "sb-usw-tst",
            //    MediaServiceAccountName = "sbtstmedia",
            //    DefaultResourceGroupName = "sb-usw-test",
            //    ClientId = "[AMS CLIENT ID]",
            //    ClientSecret = "[AMS CLIENT SECRET]"
            //};

            // LOCALHOST
            return new AzureMediaServiceConfig()
            {
                TenantId = "[AMS TENANT ID]",
                SubscriptionId = "[AMS SUB ID]",
                DefaultPrimaryStorageAccountId = "/subscriptions/[AMS SUB ID]/resourceGroups/[AMS RESOURCE GROUP NAME]/providers/Microsoft.Storage/storageAccounts/sbdeverik",
                DefaultPrimaryStorageAccountLocation = "West US",
                MediaTokenAudience = "sbuser",
                MediaTokenIssuer = TokenServiceSettings.TokenIssuerClaimValue,
                ResourceGroupName = "[AMS RESOURCE GROUP NAME]"
            };
        }

        public AzureMediaService GetService()
        {
            AzureMediaService service = new AzureMediaService();
            IAzureMediaServiceConfig config = GetConfig();
            service.SetConnectionInfo(config.SubscriptionId, config.ResourceGroupName, "[AMS MEIDA ACCOUNT NAME]");
            return service;
        }

        #endregion

        //[Fact]
        //public async Task TestAssetDuration()
        //{
        //    NOTE: this wasn't testing asset duration so I commented out the code...
        //    IAzureMediaService service = GetService();
        //    MediaAssetResource? asset = await service.GetAsset([AMS MEDIA ASSET KEY]);
        //    Assert.NotNull(asset);
        //}


        [Fact]
        public async Task TestContentKeyPolicy()
        {
            IAzureMediaServiceConfig config = GetConfig();
            byte[] key = Convert.FromBase64String("[AMS CONTENT KEY]");
            AzureMediaService service = GetService();
            try
            {
                ContentKeyPolicyResource ckp = await service.AddOrUpdateOrgContentKeyPolicy(
                    "ContentKeyTest01",
                    "Created During Unit Tests",
                    config.MediaTokenIssuer,
                    config.MediaTokenAudience,
                    key);
            }
            catch (Exception ex)
            {
                Type type = ex.GetType();
                string msg = ex.Message;
                Exception x = ex;
            }
        }
    }
}