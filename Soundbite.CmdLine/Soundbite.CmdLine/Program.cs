using System;
using System.IO;
using System.Linq;

namespace Soundbite.CmdLine
{
    internal class Program
    {
        #region Fields

        private static string _startingDir;
        private static readonly ActionService _actionService = new ActionService();

        #endregion

        #region Methods

        private static void Main(string[] args)
        {
            // Acquire the starting directory so we can return there later
            _startingDir = Directory.GetCurrentDirectory();

            // Make sure to load up all of the actions
            _actionService.ScanAssembliesForActions();

            try
            {
                if (args.Length == 0)
                {
                    GotoSolutionRoot();
                    SelectActionToRun();
                }
                else
                {
                    // Determine which command is being called
                    switch (args[0].ToLower())
                    {
                        case "/batchfile":
                            RunBatchFile(args[1]);
                            break;
                        case "/action":
                            RunActionFromCmdLine(args);
                            break;
                    }


                    Console.WriteLine("Unexpected number of arguments.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Unexpected Error Occured");
                Console.WriteLine(new string('-', 50));
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Type:    {ex.GetType().FullName}");
                Console.WriteLine(new string('-', 50));

                throw; //TODO output the error more diligently
            }
            finally
            {
                // Make sure to return to the directory from which the console app was executed
                Directory.SetCurrentDirectory(_startingDir);
            }
        }

        private static void SelectActionToRun()
        {
            Console.Clear();
            for (int i = 0; i < _actionService.AllActions.Count; i++)
            {
                Console.WriteLine($"{(i + 1)}.) {_actionService.AllActions[i].Name}");
            }

            Console.WriteLine();
            Console.Write("Choose an Action: ");
            string selection = Console.ReadLine();

            if (selection.ToLower() == "x")
            {
                Console.WriteLine();
                Console.WriteLine("Exiting");
            }
            else if (int.TryParse(selection, out int selectionIndex))
            {
                ActionInfo action = _actionService.AllActions[selectionIndex - 1];
                RunAction(action.TypeRef);
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Selection not found.");
                Console.WriteLine();
            }
        }

        private static void RunAction(Type t)
        {
            ICmdLineAction actionToRun = (ICmdLineAction)Activator.CreateInstance(t);
            PopulateParameters(actionToRun);
            actionToRun.Run();
        }

        private static void PopulateParameters(ICmdLineAction actionToRun)
        {
            if (actionToRun == null)
            {
                throw new ArgumentNullException(nameof(actionToRun));
            }
            foreach (Parameter parameter in actionToRun.Parameters)
            {
                Console.Write($"{parameter.Name}: ");
                string value = Console.ReadLine();
                _actionService.SetValue(parameter, value);
            }
        }

        private static void GotoSolutionRoot()
        {
            bool rootFound = false;
            DirectoryInfo currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (!rootFound && currentDir != null)
            {
                if (currentDir.GetFiles("Soundbite.sln").Length == 1)
                {
                    rootFound = true;

                }
                else
                {
                    currentDir = currentDir.Parent;
                }
            }

            if (currentDir == null)
            {
                throw new Exception("Failed to locate solution root directory. This application expects to run within the directory structure containing the solution.");
            }

            Directory.SetCurrentDirectory(currentDir.FullName);
            Settings.SolutionRootDir = currentDir.FullName + "\\";
        }

        private static void RunBatchFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    string fileContent = File.ReadAllText(fileName);
                    BatchFile file = Newtonsoft.Json.JsonConvert.DeserializeObject<BatchFile>(fileContent);
                    foreach (BatchAction batchAction in file.Actions)
                    {
                        ICmdLineAction action = _actionService.GetActionByName(batchAction.Name);
                        foreach (BatchParameter pDef in batchAction.Params)
                        {
                            Parameter p = action.Parameters.FirstOrDefault(i =>
                                string.Equals(pDef.Name, i.Name, StringComparison.InvariantCultureIgnoreCase));

                            if (p != null)
                            {
                                _actionService.SetValue(p, pDef.Value);
                            }
                        }
                        action.Run();
                    }
                }
                catch (Exception ex)
                {
                    throw new FormatException($"Failed to parse JSON file {fileName}", ex);
                }
            }
            else
            {
                throw new FileNotFoundException($"Batch file '{fileName}' not found.");
            }
        }

        private static void RunActionFromCmdLine(string[] args)
        {
            if (args.Length > 1)
            {
                string actionName = args[1];
                ICmdLineAction cmd = _actionService.GetActionByName(actionName);
                if (cmd != null)
                {
                    for (int i = 2; i < args.Length; i++)
                    {
                        PopulateParameterInAction(cmd, args[i]);
                    }
                    cmd.Run();
                }
                else
                {
                    throw new Exception($"Action named '{actionName}' not found.");
                }
            }
            else
            {
                throw new Exception("Action name not specified.");
            }
        }

        private static void PopulateParameterInAction(ICmdLineAction action, string arg)
        {
            try
            {
                string[] parts = arg.Split("=");
                if (parts.Length >= 2)
                {
                    string paramName = parts[0];
                    string paramValue = parts.Length == 2 ? parts[1] : string.Join(null, parts.Skip(2).Take(parts.Length - 2));
                    Parameter param = action.Parameters.FirstOrDefault(i => string.Equals(i.Name, paramName, StringComparison.InvariantCultureIgnoreCase));
                    if (param != null)
                    {
                        _actionService.SetValue(param, paramValue);
                    }
                }
                else
                {
                    throw new Exception("Length is invalid.");
                }
            }
            catch (Exception)
            {
                throw new Exception("Failed to parse parameter");
            }
        }

        #endregion

    }
}
