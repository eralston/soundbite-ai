namespace Soundbite.CmdLine
{
    /// <summary>
    /// Stores environment specific settings for the SPFX build process.
    /// </summary>
    public class SpfxEnv
    {
        #region Static Properties

        public static SpfxEnv[] AllEnvironments { get; set; }

        /// <summary>
        /// Gets the Local environment settings
        /// </summary>
        public static SpfxEnv Local { get; } = new SpfxEnv();

        /// <summary>
        /// Gets the Preview environment settings
        /// </summary>
        public static SpfxEnv Preview { get; } = new SpfxEnv();

        /// <summary>
        /// Gets the Test environment settings
        /// </summary>
        public static SpfxEnv Test { get; } = new SpfxEnv();

        /// <summary>
        /// Gets the Production USW environment settings
        /// </summary>
        public static SpfxEnv USW { get; } = new SpfxEnv();


        #endregion

        #region Static Constructor

        /// <summary>
        /// Static Constructor used to initialize static environment variables
        /// </summary>
        static SpfxEnv()
        {
            // Local Settings
            Local.EnvName = "Local";
            Local.ZippedPackageName = "soundbite-webparts-local.sppk";
            Local.SolutionName = "Soundbite for Microsoft SharePoint and Viva - Local";
            Local.SolutionId = "da177017-12c5-47e1-a0cf-f95d091f5ed1";
            Local.WebApiResourceName = "Soundbite™ Platform - Localhost";
            Local.AzureApiId = "api://soundbite.ai.ngrok.io/c1c6708c-2e71-434f-8dc4-b906bfbfbcfa";
            Local.ApiUrl = "https://localhost:44390";
            Local.CdnBasePath = "https://localhost:44777/cdn/spfx";
            Local.WebParts.Feed.Id = "da177017-12c5-474d-85e2-cec2aec19ed1";
            Local.WebParts.Feed.Alias = "SoundbiteFeedWebPartLocal";
            Local.WebParts.Feed.Title = "Soundbite Feed (Local)";

            // Preview Settings
            Preview.EnvName = "Preview";
            Preview.ZippedPackageName = "soundbite-webparts-preview.sppk";
            Preview.SolutionName = "Soundbite for Microsoft SharePoint and Viva - Preview";
            Preview.SolutionId = "da177017-12c5-47e1-a0cf-f95d091f5ed3";
            Preview.WebApiResourceName = "Soundbite™ Platform - Preview";
            Preview.AzureApiId = "api://preview.soundbite.cloud/34b24a2b-bc17-4014-a2b6-ba4d4c524848";
            Preview.ApiUrl = "https://preview.soundbite.cloud/api/v1";
            Preview.CdnBasePath = "https://storage.preview.soundbite.cloud/cdn/spfx";
            Preview.WebParts.Feed.Id = "da177017-12c5-474d-85e2-cec2aec19ed3";
            Preview.WebParts.Feed.Alias = "SoundbiteFeedWebPartPreview";
            Preview.WebParts.Feed.Title = "Soundbite Feed (Preview)";

            // Test Settings
            Test.EnvName = "Test";
            Test.ZippedPackageName = "soundbite-webparts-test.sppk";
            Test.SolutionName = "Soundbite for Microsoft SharePoint and Viva - Test";
            Test.SolutionId = "da177017-12c5-47e1-a0cf-f95d091f5ed2";
            Test.WebApiResourceName = "Soundbite™ Platform - Test";
            Test.AzureApiId = "api://test.soundbite.cloud/5b9f8382-d360-4309-a5a1-b3b6e5b53a3e";
            Test.ApiUrl = "https://test.soundbite.cloud/api/v1";
            Test.CdnBasePath = "https://storage.test.soundbite.cloud/cdn/spfx";
            Test.WebParts.Feed.Id = "da177017-12c5-474d-85e2-cec2aec19ed2";
            Test.WebParts.Feed.Alias = "SoundbiteFeedWebPartTest";
            Test.WebParts.Feed.Title = "Soundbite Feed (Test)";

            // USW Settings
            USW.EnvName = "USW";
            USW.ZippedPackageName = "soundbite-webparts.sppk";
            USW.SolutionName = "Soundbite for Microsoft SharePoint and Viva";
            USW.SolutionId = "da177017-12c5-47e1-a0cf-f95d091f5e01";
            USW.WebApiResourceName = "Soundbite™ Platform";
            USW.AzureApiId = "api://usw.soundbite.cloud/e852716e-f657-42f1-b81b-f3c06d2b37c9";
            USW.ApiUrl = "https://usw.soundbite.cloud/api/v1";
            USW.CdnBasePath = "https://storage.usw.soundbite.cloud/cdn/spfx";
            USW.WebParts.Feed.Id = "da177017-12c5-474d-85e2-cec2aec19e01";
            USW.WebParts.Feed.Alias = "SoundbiteFeedWebPart";
            USW.WebParts.Feed.Title = "Soundbite Feed";

            // All Environemnts List
            AllEnvironments = new[] { Local, Preview, Test, USW };
        }

        #endregion

        #region Fields

        /// <summary>
        /// Gets or sets the friendly name of the environment settings.
        /// </summary>
        public string EnvName { get; set; }
        public string ZippedPackageName { get; set; }
        public string SolutionName { get; set; }
        public string SolutionId { get; set; }
        public string WebApiResourceName { get; set; }
        public string AzureApiId { get; set; }
        public string ApiUrl { get; set; }
        public string CdnBasePath { get; set; }
        public SpfxEnvWebParts WebParts { get; set; } = new SpfxEnvWebParts();

        #endregion
    }

    /// <summary>
    /// Stores web part settings for the SPFX build process. There is currently a single web part
    /// but more are envisioned for later on.
    /// </summary>
    public class SpfxEnvWebParts
    {
        public SpfxEnvWebPart Feed { get; set; } = new SpfxEnvWebPart();
    }

    /// <summary>
    /// Stores basic web part information for the SPFX build process.
    /// </summary>
    public class SpfxEnvWebPart
    {
        public string Id { get; set; }
        public string Alias { get; set; }
        public string Title { get; set; }
    }

}