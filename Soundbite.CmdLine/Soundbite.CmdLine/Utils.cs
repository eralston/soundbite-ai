using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;

namespace Soundbite.CmdLine
{
    public static class Utils
    {
        public static void ReplaceSectionInFile(string filePath, string startKey, string endKey, string replacement, bool crlfBefore = true, bool crlfAfter = false)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    string content = File.ReadAllText(filePath);
                    int start = content.IndexOf(startKey);
                    int end = content.IndexOf(endKey);

                    // Only process if both keys were found
                    if (start >= 0 && end >= start)
                    {
                        start = start + startKey.Length;
                        content = content.Substring(0, start) + (crlfBefore ? "\r\n" : "") + replacement + (crlfAfter ? "\r\n" : "") + content.Substring(end, content.Length - end);
                        content = content.Trim();
                        File.WriteAllText(filePath, content);
                    }
                    else
                    {
                        throw new Exception($"Failed to replace section of text in '{filePath}' because start and end keys were not found.");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to replace section of text in '{filePath}'", ex);
                }
            }
            else
            {
                throw new FileNotFoundException($"Could not locate file '{filePath}' for content replacement.");
            }
        }

        public static void SaveJsonFile(dynamic objectToPersist, string filePath)
        {
            try
            {
                dynamic json = Newtonsoft.Json.JsonConvert.SerializeObject(objectToPersist, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save JSON to file '{filePath}'", ex);
            }
        }

        /// <summary>
        /// Creates a dynamic object from a JSON string.
        /// </summary>
        /// <param name="json">JSON string.</param>
        /// <returns>a dynamic object populated with data from the JSON string.</returns>
        public static dynamic CreateDynamicFromJson(string json)
        {
            try
            {
                dynamic result = JObject.Parse(json);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Parsing JSON into a dynamic object failed.", ex);
            }
        }

        /// <summary>
        /// Creates a dynamic object from JSON contained in file.
        /// </summary>
        /// <param name="file">Path to the file containing the JSON.</param>
        /// <returns>a dynamic object populated with data from the JSON file.</returns>
        public static dynamic CreateDynamicFromJsonFile(string file)
        {
            FileInfo jsonFile = new FileInfo(file);
            if (jsonFile.Exists)
            {
                string json = File.ReadAllText(jsonFile.FullName);
                return CreateDynamicFromJson(json);
            }
            else
            {
                throw new FileNotFoundException("JSON file '{file}' does not exist.");
            }
        }

        [DebuggerStepThrough]
        public static void Try(Action action, Func<Exception, Exception> exFunc)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception innerEx)
            {
                throw exFunc(innerEx);
            }
        }

        [DebuggerStepThrough]
        public static T Try<T>(Func<T> func, Func<Exception, Exception> exFunc)
        {
            try
            {
                return func();
            }
            catch (Exception innerEx)
            {
                throw exFunc(innerEx);
            }
        }

        [DebuggerStepThrough]
        /// <summary>
        /// Runs code and disregards any errors that may occur.
        /// </summary>
        /// <param name="action">Code to run with wanton disregard for errors.</param>
        /// <param name="onError"></param>
        public static void IgnoreError(Action action, Action<Exception> onError = null)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                // Do not throw an exception
                onError?.Invoke(ex);
            }
        }

        /// <summary>
        /// Runs a PowerShell script with parameters and prints the resulting pipeline objects to the console output. 
        /// </summary>
        /// <param name="scriptContents">The script file contents.</param>
        /// <param name="scriptParameters">A dictionary of parameter names and parameter values.</param>
        public static bool RunScript(string scriptContents)
        {
            VerifyPowershellExeExists();
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo(Settings.PowerShellExe, scriptContents)
            };
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        /// <summary>
        /// Verifies that the PowerShell setting is valid.
        /// </summary>
        private static void VerifyPowershellExeExists()
        {
            if (!File.Exists(Settings.PowerShellExe))
            {
                throw new FileNotFoundException("PowerShellExe setting does not reference a PowerShell executable.", Settings.PowerShellExe);
            }
        }

        [DebuggerStepThrough]
        public static void RunInRootDirectory(Action codeToRun)
        {
            RunInDirectory(Settings.SolutionRootDir, codeToRun);
        }

        [DebuggerStepThrough]
        public static void RunInDirectory(string directoryPath, Action codeToRun)
        {
            // Validate Directory Path
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Cannot change directory to \"{directoryPath}\"");
            }

            string originalDirectory = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(directoryPath);
                codeToRun?.Invoke();
            }
            finally
            {
                // Always revert to the original directory
                Directory.SetCurrentDirectory(originalDirectory);
            }
        }

    }
}
