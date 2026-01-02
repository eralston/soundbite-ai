using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Soundbite.CmdLine
{
    /// <summary>
    /// Command line action that updates the version number in all Soundbite NPM packages.
    /// </summary>
    [CmdLineActionInfo(Name, Description)]
    public class BuildSpFxWebParts : ICmdLineAction
    {
        #region Constants

        private const string Name = "Build SPFX WebParts";
        private const string Description = "Builds all SPFX WebParts for multiple environments and CDN vs locally hosted";

        /// <summary>
        /// Stores an array of client-side asset files that are given a checksum hash by the 
        /// bundling process to help with cache-busting operations.  While this approach to cache
        /// busting is helpful in "normal" SharePoint scenarios, the files in the case of Soundite
        /// are served from an Azure CDN that can force a refresh, and in our case we want changes
        /// to be picked up without redploying a new version of the web part package to the app
        /// catalog.  This requires our file names to NOT change after a new build, so we have to 
        /// remove the hash from the file name.
        /// </summary>
        private readonly List<HashedFileDefinition> HashedFiles = new List<HashedFileDefinition>()
        {
            new HashedFileDefinition("soundbite-feed-web-part", "js"),
            new HashedFileDefinition("SoundbiteFeedWebPartStrings_en-us", "js")
        };

        #endregion

        #region Classes

        /// <summary>
        /// Class used to store information about hashed files in the build. A hashed file is a file
        /// whose name includes a checksum hash in the file name for cache busting purposes.
        /// </summary>
        private class HashedFileDefinition
        {
            public HashedFileDefinition(string startsWith, string extension)
            {
                StartsWith = startsWith;
                Extension = extension;
            }

            /// <summary>
            /// Gets or sets the starting text of the file name.
            /// </summary>
            public string StartsWith { get; set; }

            /// <summary>
            /// Gets or sets the extension of the file.
            /// </summary>
            public string Extension { get; set; }

            /// <summary>
            /// Gets or sets the actual file name (with hash) found in the build.
            /// </summary>
            public string HashedFileName { get; set; }
        }

        #endregion

        #region Parameters

        private readonly Parameter<Version> VersionStable = new Parameter<Version>("VersionStable", "Version of the stable build. This value should increment each time.");
        private readonly Parameter<Version> VersionAuto = new Parameter<Version>("VersionAuto", "Version of the auto build. This value should be at or below the stable build version number.");
        private readonly Parameter<string> TargetEnv = new Parameter<string>("TargetEnv", $"Specifies the target environment ({SpfxEnv.Local.EnvName}, {SpfxEnv.Test.EnvName}, {SpfxEnv.Preview.EnvName}, {SpfxEnv.USW.EnvName})");

        #endregion

        #region ICmdLineAction Implementation

        public Parameter[] Parameters => new Parameter[] {
            VersionStable,
            VersionAuto,
            TargetEnv
        };

        public void Run()
        {
            FindSharePointAppFolder();
            SpfxEnv.AllEnvironments.ToList().ForEach(env =>
            {
                if (env.EnvName == TargetEnv.Value || TargetEnv.Value?.ToLower() == "all")
                {
                    BuildWebPartPackageForEnv(env, false, false); // On-Premise
                    BuildWebPartPackageForEnv(env, true, false);  // CDN (Static)
                    BuildWebPartPackageForEnv(env, true, true);   // CDN (Auto)                
                }
            });
        }

        #endregion

        #region Properties

        /// <summary>
        /// Stores the run context information for the build process.
        /// </summary>
        private SpfxContext Context { get; } = new SpfxContext();

        /// <summary>
        /// Gets a reference to the SpfxDirecotires instance on the context.
        /// </summary>
        private SpfxDirectories Dirs => Context.Dirs;

        #endregion

        #region Methods

        /// <summary>
        /// Responsible for managing the SPFX build for the specified environment
        /// </summary>
        /// <param name="env">Environment settings used for the SPFX build</param>
        private void BuildWebPartPackageForEnv(SpfxEnv env, bool useCdn, bool autoVersion)
        {
            Context.Environment = env;
            Context.StableVersion = VersionStable.Value.ToString();
            Context.AutoVersion = VersionAuto.Value.ToString();
            Context.IsCdnBuild = useCdn;
            Context.IsAutoVersionBuild = autoVersion;

            bool doit = true;

            if (doit)
            {
                CleanSharePointBuildFolders();
                ReplaceSolutionIconFile();
            }

            UpdateProjectForBuild();
            ConfigureCdnSettings();
            if (doit)
            {
                GulpClean();
            }

            if (doit)
            {
                GulpBundle();
            }

            if (doit)
            {
                GulpPackageSolution();
            }

            SaveAppPackage();
            SaveFeedWebPartAssets();
            SaveBuildAssets();


            // Auto-Versioning is dependent on using CDN files so only run auto-version
            // operations if the CDN is enabled for the build
            if (Context.IsAutoVersionBuild)
            {
                // Create Auto-Version
                RemoveAutoUpdateExtractDir();
                CreateAutoUpdateExtractDir();
                ExtractAppPackageFiles();
                UpdatedHashedFileReferences();
                RemoveAutoVersionCdnFileHashes();
                if (Context.IsNewAutoVersion)
                {
                    PackageAutoVersion();
                }
                RemoveAutoUpdateExtractDir();
            }
        }

        /// <summary>
        /// Responsible for building out a path to the SharePoint web part project folder. All of
        /// the operations in this build process are based relatively pathed from this directory.
        /// </summary>
        private void FindSharePointAppFolder()
        {
            bool done = false;
            DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (!done)
            {
                if (directory.Name.ToLower() == "soundbite")
                {
                    Context.RootDirectory = Path.Combine(directory.FullName, "Soundbite.Client\\packages\\widgets.sharepoint.app");
                    done = true;
                }
                if (directory.Parent == null)
                {
                    throw new Exception("Failed to locate the SharePoint application folder.");
                }
                directory = directory.Parent;
            }
        }

        /// <summary>
        // Deletes the SharePoint build folders. These folders are considered temporary, but don't
        // normally get deleted, and it seems like build artifacts from past builds get picked up
        // accidentally in the current build. Specifically, we change the web part IDs in the
        // manifest and those files linger and get automatically wrapped up in every build. Deleting
        // the folder before running the build process alleviates the issue.
        /// </summary>
        private void CleanSharePointBuildFolders()
        {
            Context.WrapAction("Deleting SharePoint Folder", () =>
            {
                if (Directory.Exists(Dirs.SharePointFolder))
                {
                    Directory.Delete(Dirs.SharePointFolder, true);
                }
            });

            Context.WrapAction("Deleting Release Folder", () =>
            {
                if (Directory.Exists(Dirs.ReleasesFolder))
                {
                    Directory.Delete(Dirs.ReleasesFolder, true);
                }
            });
        }

        /// <summary>
        /// Responsible for copying the solution icon file into the SharePoint images file.  This 
        /// file gets deleted when clearing out the build folders and it is currently easier to 
        /// just copy it back than working to avoid deleting it in the first place.
        /// </summary>
        private void ReplaceSolutionIconFile()
        {
            // Make sure the SharePoint images folder exists
            if (!Directory.Exists(Dirs.SharePointImagesFolder))
            {
                Directory.CreateDirectory(Dirs.SharePointImagesFolder);
            }

            // Copy the solution icon file into the directory
            if (!File.Exists(Dirs.SolutionIconTargetPath))
            {
                File.Copy(Dirs.SolutionIconSourcePath, Dirs.SolutionIconTargetPath);
            }
        }

        /// <summary>
        /// Responsible for updating project files with environmental settings before the build.
        /// </summary>
        private void UpdateProjectForBuild()
        {
            UpdatePackageSolution();
            UpdateWebPartManifest();
            UpdateWebPartStoreFile();
        }

        private void UpdatePackageSolution()
        {
            Context.WrapAction("Updating package version in package-solution.json", () =>
            {
                try
                {
                    string filePath = Dirs.PackageSolutionJsonFile;
                    dynamic json = Utils.CreateDynamicFromJsonFile(filePath);
                    json.solution.name = Context.Environment.SolutionName;
                    json.solution.id = Context.Environment.SolutionId;
                    json.solution.webApiPermissionRequests[0].resource = Context.Environment.WebApiResourceName;
                    json.solution.version = Context.StableVersion;
                    json.paths.zippedPackage = $"solution/{Context.Environment.ZippedPackageName}";
                    Utils.SaveJsonFile(json, filePath);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to update SP Manifest Solution Version", ex);
                }
            });
        }

        private void UpdateWebPartManifest()
        {
            Context.WrapAction("Updating Webpart Manifest", () =>
            {
                try
                {
                    string filePath = Dirs.SoundbiteFeedWebPartManifest;
                    dynamic json = Utils.CreateDynamicFromJsonFile(filePath);
                    json.id = Context.Environment.WebParts.Feed.Id;
                    json.alias = Context.Environment.WebParts.Feed.Alias;
                    json.preconfiguredEntries[0].title.@default = Context.Environment.WebParts.Feed.Title;
                    Utils.SaveJsonFile(json, filePath);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to update SP Manifest Solution Version", ex);
                }
            });
        }

        /// <summary>
        /// Updates the Soundbite environment information in the WebPartStore.ts file.
        /// </summary>
        private void UpdateWebPartStoreFile()
        {
            Context.WrapAction("Updating WebPartStore.ts File", () =>
            {
                string replacementText =
                    $"const sbEnv = {{\r\n" +
                    $"  azureApiId: \"{Context.Environment.AzureApiId}\",\r\n" +
                    $"  apiUrl: \"{Context.Environment.ApiUrl}\"\r\n" +
                    $"}};\r\n" +
                    $"const sbwpverValue:string=\"sv{Context.StableVersion}\";";

                Utils.ReplaceSectionInFile(Dirs.WebPartStoreFile,
                    "//////////[ BEGIN - API Settings ]//////////////////////////////////////////////////////////////////",
                    "//////////[ END - API Settings ]////////////////////////////////////////////////////////////////////",
                    replacementText);
            });
        }

        /// <summary>
        /// Responsible for updating aspects of the project when the CDN is enabled or disabled.
        /// </summary>
        /// <param name="cdnEnabled">Flag indicating whether the CDN is enabled or disabled.</param>
        private void ConfigureCdnSettings()
        {
            Context.WriteLine($"Setting CDN to {(Context.IsCdnBuild ? "ON" : "OFF")}");
            Context.Indent(() =>
            {
                // Update the CDN Base Path
                Context.WrapAction("Updating './config/write-manifests.json'", () =>
                {
                    dynamic json = Utils.CreateDynamicFromJsonFile(Dirs.WriteManifestsFile);
                    json.cdnBasePath = Context.IsCdnBuild
                    ? (Context.IsAutoVersionBuild ? Context.CdnPathAutoVersion : Context.CdnPathStableVersion)
                    : "<!-- PATH TO CDN -->";
                    Utils.SaveJsonFile(json, Dirs.WriteManifestsFile);
                });

                // Update whether to include Client-Side Assets in ./config/package-solution.json
                Context.WrapAction("Updating './config/package-solution.json'", () =>
                {
                    dynamic json = Utils.CreateDynamicFromJsonFile(Dirs.PackageSolutionJsonFile);
                    json.solution.includeClientSideAssets = !Context.IsCdnBuild;
                    Utils.SaveJsonFile(json, Dirs.PackageSolutionJsonFile);
                });

                // Update WebPartStore.ts Image References
                Context.WrapAction("Updating 'WebPartStore.ts' Image References", () =>
                {
                    string replacementText =
                        "//NOTE: Do not modify anything in this section without accounting for the change in the web part\r\n" +
                        "//      script.  Content in this section changes when CDN settings are updated.\r\n" +
                        "SoundbiteApiConfig.imgAvatarUrl = <AVATAR>;\r\n" +
                        "SoundbiteApiConfig.imgEmptyCardArtUrl = <EMPTYCARDART>;\r\n";

                    string avatarReplacement = Context.IsCdnBuild
                        ? (Context.IsAutoVersionBuild
                            ? $"\"{Context.Environment.CdnBasePath}/autoVersion/v{Context.AutoVersion}/default-avatar.png\""
                            : $"\"{Context.Environment.CdnBasePath}/v{Context.StableVersion}/default-avatar.png\"")
                        : $"require(\"../webparts/soundbiteFeed/assets/default-avatar.png\")";

                    string emptyCardArtReplacement = Context.IsCdnBuild
                        ? (Context.IsAutoVersionBuild
                            ? $"\"{Context.Environment.CdnBasePath}/autoVersion/v{Context.AutoVersion}/empty-card-art.png\""
                            : $"\"{Context.Environment.CdnBasePath}/v{Context.StableVersion}/empty-card-art.png\"")
                        : $"require(\"../webparts/soundbiteFeed/assets/empty-card-art.png\")";

                    replacementText = replacementText
                        .Replace("<AVATAR>", avatarReplacement)
                        .Replace("<EMPTYCARDART>", emptyCardArtReplacement);

                    Utils.ReplaceSectionInFile(Dirs.WebPartStoreFile,
                        "//////////[ BEGIN - Image Locations ]///////////////////////////////////////////////////////////////",
                        "//////////[ END - Image Locations ]/////////////////////////////////////////////////////////////////",
                        replacementText);
                });
            });
        }

        /// <summary>
        /// Executes the Gulp Clean command which ensures a clean build environment.
        /// </summary>
        private void GulpClean()
        {
            try
            {
                Context.WriteLine(new string('-', 70));
                Context.WriteLine("Running Gulp Clean...");
                Context.WriteLine(new string('-', 70));
                ProcessStartInfo gulpClean = new ProcessStartInfo("cmd.exe", $"/c gulp clean && gulp clean --ship")
                {
                    WorkingDirectory = Context.RootDirectory
                };
                Process process = Process.Start(gulpClean);
                process.WaitForExit();
                Context.WriteLine(new string('-', 70));
                Context.Write("Gulp Clean...");
                Context.WriteDots();
                Context.WriteLineInColor("OK", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Context.Write("Gulp Clean");
                Context.WriteDots();
                Context.WriteLineInColor("Failed", ConsoleColor.Red);
                throw new Exception("Failed to run Gulp Clean commamnd", ex);
            }
        }

        /// <summary>
        /// Responsible for running the gulp bundle command which bundles the SPFX webpat project.
        /// </summary>
        private void GulpBundle()
        {
            try
            {
                Context.WriteLine(new string('-', 70));
                Context.WriteLine("Running Gulp Build...");
                Context.WriteLine(new string('-', 70));
                ProcessStartInfo gulpBundle = new ProcessStartInfo("cmd.exe", $"/c gulp bundle --ship")
                {
                    WorkingDirectory = Context.RootDirectory
                };
                Process process = Process.Start(gulpBundle);
                process.WaitForExit();
                Context.WriteLine(new string('-', 70));
                Context.Write("Gulp Bundle");
                Context.WriteDots();
                Context.WriteLineInColor("OK", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Context.Write("Gulp Bundle...");
                Context.WriteDots();
                Context.WriteLineInColor("Failed", ConsoleColor.Red);
                throw new Exception("Failed to run Gulp Clean commamnd", ex);
            }
        }

        /// <summary>
        /// Responsible for running the gulp bundle command which bundles the SPFX webpat project.
        /// </summary>
        private void GulpPackageSolution()
        {
            try
            {
                Context.WriteLine(new string('-', 70));
                Context.WriteLine("Running Gulp Package-Solution...");
                Context.WriteLine(new string('-', 70));
                ProcessStartInfo gulpPackageSolution = new ProcessStartInfo("cmd.exe", $"/c gulp package-solution --ship")
                {
                    WorkingDirectory = Context.RootDirectory
                };
                Process process = Process.Start(gulpPackageSolution);
                process.WaitForExit();
                Context.WriteLine(new string('-', 70));
                Context.Write("Gulp Bundle");
                Context.WriteDots();
                Context.WriteInColor("OK", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Context.Write("Gulp Bundle...");
                Context.WriteDots();
                Context.WriteInColor("Failed", ConsoleColor.Red);
                throw new Exception("Failed to run Gulp Clean commamnd", ex);
            }
        }

        public void SaveAppPackage()
        {
            // Only save off the application package if this is NOT an autobuild.  The autobuild
            // extracts the build package, modifies files, and repackages so it does not need to
            // copy over the original application package.
            if (!Context.IsAutoVersionBuild)
            {
                Context.WriteLine("Saving Application Package File...");
                Context.Indent(() =>
                {
                    try
                    {

                        string appPackagesDir = Dirs.AppPackagesFolder;
                        string sourceFile = Dirs.WebPartPackageBuildFile;
                        string targetDir = Dirs.StableVersionEnvFolder;
                        string targetFile = $"{targetDir}\\{Dirs.WebPartPackageReleaseFileName}";

                        // Make sure the application package was actually generated
                        Context.WrapAction("Verifying app package file exists", () =>
                            {
                                if (!File.Exists(sourceFile))
                                {
                                    throw new FileNotFoundException("Application package was not found.");
                                }
                            });

                        // Create the App Packages folder if it does not exist
                        Context.WrapAction("Verifying app packages folder exists", () =>
                            {
                                if (!Directory.Exists(appPackagesDir))
                                {
                                    Utils.Try(() => Directory.CreateDirectory(appPackagesDir),
                                        (ex) => new Exception("Failed to create 'appPackages' directory", ex));
                                }
                            });

                        // Create the Version folder if it does not exist
                        Context.WrapAction("Creating Release Folder", () =>
                            {
                                if (!Directory.Exists(targetDir))
                                {
                                    Utils.Try(() => Directory.CreateDirectory(targetDir),
                                        (ex) => new Exception($"Failed to create 'appPackages/{Context.StableVersion}' folder"));
                                }
                            });

                        // Copy the package file to the release folder
                        Context.WrapAction("Copying Application Package File", () =>
                            {
                                Utils.Try(() => File.Copy(sourceFile, targetFile, true),
                                (ex) => new Exception("Failed to copy the app package to 'appPackages' folder"));
                            });

                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to save the Application Package", ex);
                    }
                });
            }
        }

        public void SaveFeedWebPartAssets()
        {
            if (Context.IsCdnBuild)
            {
                string sourceDir = Dirs.FeedWebPartAssetsFolder;
                string targetDir = Context.IsAutoVersionBuild
                    ? Dirs.AutoVersionCdnFolder
                    : Dirs.StableVersionCdnFolder;

                Context.WrapAction("Copying Feed Web Part Assets to CDN Release Folder", () =>
                {
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    List<string> files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories).ToList();
                    foreach (string fileSource in files)
                    {
                        string fileTarget = fileSource.Replace(sourceDir, targetDir);
                        File.Copy(fileSource, fileTarget, true);
                    }
                });
            }
        }

        public void SaveBuildAssets()
        {
            if (Context.IsCdnBuild)
            {
                string sourceDir = Dirs.BuildClientAssetsFolder;
                string targetDir = Context.IsAutoVersionBuild
                    ? Dirs.AutoVersionCdnFolder
                    : Dirs.StableVersionCdnFolder;

                Context.WrapAction("Copying Build Assets to CDN Release Folder", () =>
                {
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    List<string> files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories).ToList();
                    foreach (string fileSource in files)
                    {
                        string fileTarget = fileSource.Replace(sourceDir, targetDir);
                        File.Copy(fileSource, fileTarget, true);
                    }
                });
            }
        }

        /// <summary>
        /// Responsible for copying the loose client assets in the Soundbite Feed webpart into the
        /// CDN folder for the release.  These files do not contain a hash because the build does 
        /// not include client assets.
        /// </summary>
        public void CopyFeedWebPartAssetsToCdnFolder()
        {
            // Only copy assets if it is a CDN build. The local build bundles files in the package.
            if (Context.IsCdnBuild)
            {
                string sourceDir = Dirs.FeedWebPartAssetsFolder;
                string targetDir = Context.IsAutoVersionBuild ? Dirs.AutoVersionCdnFolder : Dirs.StableVersionCdnFolder;

                Context.WrapAction("Copying Images to CDN Release Folder", () =>
                {
                    List<string> files = Directory.GetFiles(sourceDir).ToList();
                    foreach (string fileSource in files)
                    {
                        string fileTarget = fileSource.Replace(sourceDir, targetDir);
                        File.Copy(fileSource, fileTarget, true);
                    }
                });
            }
        }

        public void RemoveAutoUpdateExtractDir()
        {
            Context.WrapAction("Removing Auto Update Extract Directory", () =>
            {
                string autoUpdateExtractDir = Dirs.BuildExtractFolder;
                if (Directory.Exists(autoUpdateExtractDir))
                {
                    Directory.Delete(autoUpdateExtractDir, true);
                }
            });
        }

        public void CreateAutoUpdateExtractDir()
        {
            Context.WrapAction("Creating Auto Update Extract Directory", () =>
            {
                string autoUpdateExtractDir = Dirs.BuildExtractFolder;
                Directory.CreateDirectory(autoUpdateExtractDir);
            });
        }


        public void ExtractAppPackageFiles()
        {
            string sourceFile = Dirs.WebPartPackageBuildFile;
            string targetDir = Dirs.BuildExtractFolder;

            Context.WrapAction("Extracting App Package Files", () =>
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(sourceFile, targetDir);
            });
        }

        /// <summary>
        /// Updates the hashed file references for client-side assets in the package.  The bundling
        /// process emits and references files with checksum hashes to help with cache-busting. 
        /// While this approach to cache busting is helpful in "normal" SharePoint scenarios, the 
        /// files in the case of Soundite are served from an Azure CDN that can force its own cache
        /// refres. We also want changes to be picked up without redploying a new version of the 
        /// web part package to the app catalog.  This requires our file names to NOT change after a
        /// new build, so we have to remove the hash from the file name.
        /// </summary>
        public void UpdatedHashedFileReferences()
        {
            Context.WriteLine("Updating Hashed File References...");
            Context.Indent(() =>
            {
                Context.WrapAction("Locating Client Assets", () =>
                {
                    PopulateHashedFileNames();
                });

                string[] directories = new string[] {
                    Dirs.BuildExtractFolder,
                    Dirs.BuildClientAssetsFolder
                };

                foreach (string directory in directories)
                {
                    if (Directory.Exists(directory))
                    {
                        string[] filesToProcess = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
                        foreach (string file in filesToProcess)
                        {
                            UpdateHashedFileReferenceInFile(file);
                        }
                    }
                    else
                    {
                        throw new DirectoryNotFoundException($"Hashed file content directory '{directory}' does not exist.");
                    }
                }
            });
        }

        /// <summary>
        /// Responsible for looking through the specified client asset files for references to
        /// hashed file references and updated those hashed references to be direct references.
        /// </summary>
        /// <param name="filePath">Path to the file to update.</param>
        private void UpdateHashedFileReferenceInFile(string filePath)
        {
            var exclude = new[] { ".png" };

            Context.WrapAction(Path.GetFileName(filePath), (ctx) =>
            {
                if (!exclude.Contains(Path.GetExtension(filePath)))
                {
                    string content = File.ReadAllText(filePath);
                    int origLength = content.Length;
                    foreach (HashedFileDefinition asset in HashedFiles)
                    {
                        content = content.Replace(asset.HashedFileName, $"{asset.StartsWith}.{asset.Extension}");
                    }
                    File.WriteAllText(filePath, content);
                    bool isUpdated = content.Length != origLength;
                    ctx.SuccessMsg = isUpdated ? "Updated" : "Not Updated";
                    ctx.SuccessColor = isUpdated ? ConsoleColor.DarkYellow : ConsoleColor.White;
                }
                else
                {
                    ctx.SuccessMsg = "Skipped";
                    ctx.SuccessColor = ConsoleColor.DarkYellow;
                }
            });
        }

        /// <summary>
        /// Responsible for locating client asset files with their hashes.
        /// </summary>
        /// <returns>a list of the hashed file names</returns>
        private void PopulateHashedFileNames()
        {
            try
            {
                string sourceDir = Dirs.AutoVersionCdnFolder;
                string[] sourceFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);

                HashedFiles.ForEach(asset =>
                {
                    List<string> matchedFiles = sourceFiles.Where(i =>
                    {
                        string fileName = Path.GetFileName(i);
                        return !fileName.Equals($"{asset.StartsWith}.{asset.Extension}", StringComparison.InvariantCultureIgnoreCase)
                            && fileName.StartsWith(asset.StartsWith, StringComparison.InvariantCultureIgnoreCase)
                            && fileName.EndsWith(asset.Extension, StringComparison.InvariantCultureIgnoreCase);
                    }).ToList();

                    if (matchedFiles.Count == 0)
                    {
                        throw new Exception($"Failed to locate client asset file match for '{asset.StartsWith}*{asset.Extension}'");
                    }
                    if (matchedFiles.Count > 1)
                    {
                        throw new Exception($"Found more than one client asset file match was found for '{asset.StartsWith}*{asset.Extension}'");
                    }
                    asset.HashedFileName = Path.GetFileName(matchedFiles[0]);
                });

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to populate hashed file names for client assets", ex);
            }
        }

        /// <summary>
        /// Responsible for renaming files in the AutoVersion CDN folder so they no longer have 
        /// hashed values in the file name.
        /// </summary>
        /// <exception cref="FileNotFoundException">Thrown when an expected client asset file is not found.</exception>
        private void RemoveAutoVersionCdnFileHashes()
        {
            Context.WriteLine("Removing Auto Version CDN File Hashes...");
            Context.Indent(() =>
            {
                HashedFiles.ForEach(asset =>
                {
                    Context.WrapAction($"{asset.StartsWith}.{asset.Extension}", (ctx) =>
                    {
                        string fileSource = $"{Dirs.AutoVersionCdnFolder}\\{asset.HashedFileName}";
                        if (File.Exists(fileSource))
                        {
                            string fileTarget = $"{Dirs.AutoVersionCdnFolder}\\{asset.StartsWith}.{asset.Extension}";
                            File.Copy(fileSource, fileTarget, true);
                            File.Delete(fileSource);
                        }
                        else
                        {
                            ctx.ErrorMsg = "Not Found";
                            throw new FileNotFoundException($"Failed to locate Client Asset {asset.StartsWith}.{asset.Extension}.");
                        }
                    });
                });
            });
        }

        /// <summary>
        /// Packages the updated files for the AutoVersion into an .sppkg file (ZIP format).
        /// </summary>
        private void PackageAutoVersion()
        {
            string sourceDir = Dirs.BuildExtractFolder;
            string targetPath = $"{Dirs.AutoVersionStableEnvFolder}\\{Dirs.WebPartPackageReleaseFileName}";

            Context.WrapAction("Packaging Auto Version Files...", (ctx) =>
            {
                if (File.Exists(targetPath))
                {
                    Context.WrapAction("Deleting Existing AutoVersion Package", (ctx) =>
                    {
                        ctx.SuccessColor = ConsoleColor.Yellow;
                        File.Delete(targetPath);
                    });
                }
                System.IO.Compression.ZipFile.CreateFromDirectory(sourceDir, targetPath, System.IO.Compression.CompressionLevel.Optimal, false);
            });
        }


        #endregion
    }
}
