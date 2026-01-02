using System.IO;

namespace Soundbite.CmdLine
{
    /// <summary>
    /// Compiles and packages all of the Soundbite NPM packages.
    /// </summary>
    [CmdLineActionInfo(Name, Description)]
    public class CreateNpmPackages : ICmdLineAction
    {
        #region Constants

        private const string Name = "Package NPM Libraries";
        private const string Description = "Compiles and packages all of the NPM libraries for distribution.";

        #endregion

        #region ICmdLineAction Implementation

        public Parameter[] Parameters => new Parameter[] {

        };


        public void Run()
        {
            Utils.RunInDirectory(Settings.ClientsDir, () =>
            {
                if (Utils.RunScript("npm run build"))
                {
                    foreach (string packageDir in Settings.PackageDirectories.AllPackageDirectories)
                    {
                        Directory.SetCurrentDirectory(packageDir);
                        Utils.RunScript("npm pack;xcopy *.tgz '..\\..\\dist\\\' /Y;rm *.tgz");
                    }
                }
            });
        }



        #endregion
    }
}
