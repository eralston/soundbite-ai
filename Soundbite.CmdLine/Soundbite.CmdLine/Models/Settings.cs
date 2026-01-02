namespace Soundbite.CmdLine
{
    public static class Settings
    {
        public static string SolutionRootDir { get; set; } = ".\\";
        public static string ClientsDir => SolutionRootDir + "Soundbite.Client\\packages\\";
        public static string PowerShellExe => "c:\\windows\\system32\\WindowsPowerShell\\v1.0\\powershell.exe";

        public static class PackageDirectories
        {
            public static string Api => ClientsDir + "api\\";
            public static string Api_Axios => ClientsDir + "api.axios\\";
            public static string Widgets_Api => ClientsDir + "widgets.api\\";
            public static string Widgets_React => ClientsDir + "widgets.react\\";

            public static string[] AllPackageDirectories => new string[] {
                Api, Api_Axios, Widgets_Api, Widgets_React
            };
        }


    }
}
