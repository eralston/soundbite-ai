namespace Soundbite.CmdLine
{
    /// <summary>
    /// Stores environment specific settings for the SPFX build process.
    /// </summary>
    public class SpfxDirectories
    {
        #region Fields

        private SpfxContext Context { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="SpfxDirectories"/> instance.
        /// </summary>
        /// <param name="context">Context used to build out directory names.</param>
        public SpfxDirectories(SpfxContext context)
        {
            Context = context;
        }

        #endregion

        #region Properties

        public string AppPackagesFolder => $"{Context.RootDirectory}\\appPackages";

        public string AutoVersionFolder => $"{AppPackagesFolder}\\autoVersion\\v{Context.AutoVersion}";

        public string AutoVersionStableFolder => $"{AutoVersionFolder}\\v{Context.StableVersion}";

        public string AutoVersionStableEnvFolder => $"{AutoVersionFolder}\\v{Context.StableVersion}\\{Context.Environment.EnvName}";

        public string AutoVersionCdnFolder => $"{AutoVersionStableEnvFolder}\\Client Assets (CDN)\\";

        public string AutoVersionPackageFile => $"{AutoVersionStableEnvFolder}\\{Context.Environment.ZippedPackageName}";

        public string BuildClientAssetsFolder => $"{ReleasesFolder}\\assets";

        public string BuildExtractFolder => $"{AppPackagesFolder}\\extract";

        public string ConfigFolder => $"{Context.RootDirectory}\\config";

        public string FeedWebPartAssetsFolder => $"{Context.RootDirectory}\\src\\webparts\\soundbiteFeed\\assets";

        public string SoundbiteFeedWebPartManifest => $"{Context.RootDirectory}\\src\\webparts\\soundbiteFeed\\SoundbiteFeedWebPart.manifest.json";

        public string PackageSolutionJsonFile => $"{ConfigFolder}\\package-solution.json";

        public string ReleasesFolder => $"{Context.RootDirectory}\\release";

        public string SharePointFolder => $"{Context.RootDirectory}\\sharepoint";

        public string SharePointImagesFolder => $"{SharePointFolder}\\images";

        public string StableVersionFolder => $"{AppPackagesFolder}\\v{Context.StableVersion}";

        public string StableVersionEnvFolder => $"{AppPackagesFolder}\\v{Context.StableVersion}\\{Context.Environment.EnvName}";

        public string StableVersionCdnFolder => $"{StableVersionEnvFolder}\\Client Assets (CDN)";

        public string SolutionIconTargetPath => $"{SharePointImagesFolder}\\SolutionIcon.png";

        public string SolutionIconSourcePath => $"{ConfigFolder}\\SolutionIcon.png";

        /// <summary>
        /// Gets or sets the name of the web part package file produced by the build.
        /// </summary>
        public string WebPartPackageBuildFile => $"{Context.RootDirectory}\\sharepoint\\solution\\{Context.Environment.ZippedPackageName}";

        /// <summary>
        /// Gets the name of the release-version of the web part package file.
        /// </summary>
        public string WebPartPackageReleaseFileName => Context.IsCdnBuild
            ? (Context.IsAutoVersionBuild
                ? "soundbite.webparts.cdn.sppkg"
                : "soundbite.webparts.cdn.static.sppkg")
            : "soundbite.webparts.sppkg";

        public string WebPartStoreFile => $"{Context.RootDirectory}/src/services/WebPartStore.ts";

        public string WriteManifestsFile => $"{Context.RootDirectory}/config/write-manifests.json";

        #endregion
    }
}