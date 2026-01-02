using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Soundbite.CmdLine
{
    /// <summary>
    /// Command line action that updates the version number in all Soundbite NPM packages.
    /// </summary>
    [CmdLineActionInfo(Name, Description)]
    public class SetVersionOnPackages : ICmdLineAction
    {
        #region Constants

        private const string Name = "Set Versions On All Packages";
        private const string Description = "Sets the same version on all Soundbite NPM packages.";

        #endregion

        #region Parameters

        /// <summary>
        /// Stores the version number to apply to the package files.
        /// </summary>
        private readonly Parameter<Version> Version = new Parameter<Version>("Version", "Version to set on all packages");

        #endregion

        #region ICmdLineAction Implementation

        public Parameter[] Parameters => new Parameter[] {
            Version
        };


        public void Run()
        {
            ProcessPackageJson(Settings.PackageDirectories.Api + "package.json");
            ProcessPackageJson(Settings.PackageDirectories.Api_Axios + "package.json");
            ProcessPackageJson(Settings.PackageDirectories.Widgets_Api + "package.json");
            ProcessPackageJson(Settings.PackageDirectories.Widgets_React + "package.json");
        }

        public void ProcessPackageJson(string packageDotJsonPath)
        {
            // Validate parameters
            if (!File.Exists(packageDotJsonPath))
            {
                throw new Exception($"Could not locate the file {packageDotJsonPath}");
            }

            // Read the file content
            string content = Utils.Try(
                () => File.ReadAllText(packageDotJsonPath),
                (ex) => new Exception($"Failed to text content from the file \"{packageDotJsonPath}\"", ex)
            );

            // Parse the JSON
            JObject packageJson = Utils.Try(
                () => JObject.Parse(content),
                (ex) => new Exception($"Failed to parse JSON from the file \"{packageDotJsonPath}\"", ex)
            );

            // Update the value
            packageJson["version"] = Version.Value.ToString();

            // Save the file
            Utils.Try(
                () => { File.WriteAllText(packageDotJsonPath, packageJson.ToString()); },
                (ex) => new Exception($"Failed to save updated JSON back to the file \"{packageDotJsonPath}\"", ex)
            );
        }



        #endregion
    }
}
