using CommandLine;

namespace Masticore.CodeGen
{
    /// <summary>
    /// Options object that carries the path for the settings file
    /// </summary>
    public class Options
    {
        /// <summary>
        /// All paths in settings are relative to the executing directory
        /// </summary>
        [Option('s', "settings", Required = false, HelpText = "Sets the path to the settings JSON file")]
        public string SettingsPath { get; set; }
    }
}
