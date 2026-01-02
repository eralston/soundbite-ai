using CommandLine;
using Masticore.CodeGen.Models;
using Masticore.CodeGen.Processors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace Masticore.CodeGen
{
    /// <summary>
    /// Entry point for running the CodeGen process
    /// </summary>
    public static class Runner
    {
        /// <summary>
        /// Converts command-lines arguments into a CodeGenSettings object then runs Process on it
        /// </summary>
        /// <param name="args"></param>
        public static void ParseAndProcess(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(opt =>
            {
                Console.WriteLine("Starting...");
                CodeGenSettings settings = ReadSettings(opt);
                Process(settings);
                Console.WriteLine("Done");
            });
        }

        /// <summary>
        /// Loads a CodeGenSettings based on the given options
        /// </summary>
        /// <param name="opt"></param>
        /// <returns></returns>
        public static CodeGenSettings ReadSettings(Options opt)
        {
            Console.WriteLine();
            Console.WriteLine("Reading Settings...");

            string path = opt.SettingsPath;
            if (string.IsNullOrEmpty(path))
            {
                path = "CodeGen.json";
            }

            string json = File.ReadAllText(path);
            CodeGenSettings settings = JsonConvert.DeserializeObject<CodeGenSettings>(json);

            if (settings.AssemblyPaths.Length == 0)
            {
                throw new Exception("Assembly list is empty! Must have assemblies to search.");
            }

            if (string.IsNullOrEmpty(settings.ControllerBaseClass))
            {
                settings.ControllerBaseClass = "Service";
            }

            settings.OutputRootDir = Path.Combine(Directory.GetCurrentDirectory(), settings.OutputRootDir);
            Console.WriteLine("Output: {0}", settings.OutputRootDir);

            settings.XmlDocFile = Path.Combine(Directory.GetCurrentDirectory(), settings.XmlDocFile);
            Console.WriteLine("Xml Output: {0}", settings.XmlDocFile);

            for (int i = 0; i < settings.AssemblyPaths.Length; ++i)
            {
                settings.AssemblyPaths[i] = Path.Combine(Directory.GetCurrentDirectory(), settings.AssemblyPaths[i]);
                Console.WriteLine("Assembly sweep: {0}", settings.AssemblyPaths[i]);
            }

            return settings;
        }

        /// <summary>
        /// Runs the process based on the given settings
        /// </summary>
        /// <param name="settings"></param>
        public static void Process(CodeGenSettings settings)
        {
            Console.WriteLine();
            Console.WriteLine("Starting process...");

            CodeGenContext context = new CodeGenContext
            {
                Settings = settings
            };

            FindAssemblies(context);

            XmlDocHelper xmlDocHelper = new XmlDocHelper(context.Settings.XmlDocFile);
            TypeProcessor processor = new TypeProcessor();
            processor.Process(context, xmlDocHelper);
        }

        private static void FindAssemblies(CodeGenContext context)
        {
            foreach (string path in context.Settings.AssemblyPaths)
            {
                Assembly assembly = Assembly.LoadFrom(path);
                context.AssembliesToScan.Add(assembly);
            }

            context.AssembliesToScan.ForEach(assembly =>
            {
                try
                {
                    Type[] allTypes = assembly.GetTypes();
                    allTypes.ForEach(t =>
                    {
                        CodeGenModelAttribute codeGenModelAttribute = t.GetCustomAttribute<CodeGenModelAttribute>();
                        CodeGenTransform codeGenTransformAttribute = t.GetCustomAttribute<CodeGenTransform>();

                        if (codeGenTransformAttribute != null)
                        {
                            context.TypesToTransform.Add(t, codeGenTransformAttribute);
                            context.RegisterType(codeGenTransformAttribute.TargetType);
                        }
                        else if (codeGenModelAttribute != null)
                        {
                            context.RegisterType(t);
                        }
                        else if (typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
                        {
                            Console.WriteLine("Controller found: {0}", t.Name);
                            context.RegisterType(t);
                        }
                        else if (t.IsEnum)
                        {
                            Console.WriteLine("Enum found: {0}", t.Name);
                            context.RegisterType(t);
                        }
                    });
                }
                catch (Exception e)
                {
                    // Pikachu!
                    Console.WriteLine("Exception scanning assemblies for types: {0}", e.Message);
                }
            });
        }
    }
}
