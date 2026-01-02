using Masticore.CodeGen.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Masticore.CodeGen.Processors
{
    /// <summary>
    /// Converts .Net classes and interfaces into Typescript interface declarations.
    /// The behavior of this process for a given model can be customized using the <see cref="CodeGenModelAttribute"/> and <see cref="CodeGenFieldAttribute"/>
    /// </summary>
    public class ModelProcessor
    {
        #region Fields

        private readonly CodeGenContext _context;
        private readonly XmlDocHelper _xmlDocHelper;
        private readonly IList<Type> _typesProcessed = new List<Type>();
        protected readonly HashSet<string> _modelNames = new HashSet<string>();

        #endregion

        public ModelProcessor(CodeGenContext context, XmlDocHelper xmlDocHelper)
        {
            _context = context;
            _xmlDocHelper = xmlDocHelper;
            string outputDir = Path.Combine(_context.Settings.OutputRootDir, _context.Settings.ModelOutputDir);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
        }

        private IEnumerable<Type> ProcessBaseTypeAndInterfaces(Type modelType)
        {
            List<Type> result = modelType.GetInterfaces()
                .Where(type => _context.AssembliesToScan.Contains(type.Assembly) && !type.IsIgnored())
                .OrderBy(i => i.Name)
                .ToList();
            Type baseType = modelType.BaseType;
            if (baseType != null && _context.AssembliesToScan.Contains(baseType.Assembly))
            {
                result.Insert(0, baseType);
            }

            result.ForEach(i =>
            {
                _context.RegisterType(i);
            });

            return result;
        }

        public void ProcessType(Type modelType)
        {
            string modelName = modelType.ModelName();

            if (_modelNames.Contains(modelName))
            {
                return;
            }

            _modelNames.Add(modelName);

            _typesProcessed.Add(modelType);

            if (modelType.IsGenericType)
            {
                modelType = modelType.GetGenericTypeDefinition();
            }

            IEnumerable<Type> associatedTypes = ProcessBaseTypeAndInterfaces(modelType);
            string fileName = Path.Combine(_context.Settings.OutputRootDir, _context.Settings.ModelOutputDir, $"{modelName}.model.ts");
            StringBuilder content = new StringBuilder();
            Dictionary<string, Type> imports = associatedTypes.ToDictionary(i => i.ModelName(), i => i);

            bool isDeprecated = modelType.GetCustomAttribute<ObsoleteAttribute>() != null;

            Utils.WriteSummary(content, _xmlDocHelper.GetSummary(modelType, $"Automatically generated model for {modelType.TypeNameWithNamespace()}"), 0, isDeprecated);
            BuildInterfaceDeclaration(content, modelType, modelName, associatedTypes);
            BuildInterfaceBody(modelType, content, imports);
            BuildImports(content, imports);
            File.WriteAllText(fileName, content.ToString());
        }

        private static void BuildImports(StringBuilder content, Dictionary<string, Type> imports)
        {
            // Add imports (if necessary)
            if (imports.Any())
            {
                content.Insert(0, "\r\n");

                // Import models (if any)
                imports
                    .Where(i => !i.Value.IsEnum)
                    .Select(i => i.Value.ModelName())
                    .OrderByDescending(n => n)
                    .ForEach(modelName =>
                    {
                        content.Insert(0, $"import {{ {modelName} }} from './{modelName}.model';\r\n");
                    });

                // Import enumerations (if any)
                IEnumerable<string> enums = imports
                                                .Where(i => i.Value.IsEnum)
                                                .Select(i => i.Value.ModelName())
                                                .OrderBy(n => n);

                if (enums.Any())
                {
                    content.Insert(0, $"import {{ {string.Join(", ", enums)} }} from '../enums';\r\n");
                }
            }

            Utils.AppendAutoGenWarning(content);
        }

        private static void BuildInterfaceDeclaration(StringBuilder content, Type modelType, string modelName, IEnumerable<Type> associatedTypes)
        {
            content.Append($"export interface ");

            content.Append(modelType.ModelNameWithOptionalGeneric());

            if (associatedTypes.Any())
            {
                content.Append(" extends ");
                content.Append(string.Join(", ", associatedTypes.Select(t => t.ModelNameWithOptionalGeneric())));
            }
        }

        private void BuildInterfaceBody(Type modelType, StringBuilder content, Dictionary<string, Type> imports)
        {
            content.AppendLine($" {{");

            bool wroteSummary = false;
            modelType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(p => p.CodeGenName())
                .ForEach((property, index) =>
                {
                    if (property.IsIgnored())
                    {
                        return;
                    }

                    if (wroteSummary)
                    {
                        content.AppendLine();
                        wroteSummary = false;
                    }
                    bool isDeprecated = property.GetCustomAttribute<ObsoleteAttribute>() != null;

                    string summary = _xmlDocHelper.GetSummary(modelType, property, null);
                    if (!string.IsNullOrEmpty(summary) || isDeprecated)
                    {
                        content.AppendLine($"  /***");
                        if (!string.IsNullOrEmpty(summary))
                        {
                            summary.Split("\r\n").ForEach(line =>
                            {
                                content.AppendLine($"   * {line}");
                            });
                        }

                        if (isDeprecated)
                        {
                            content.AppendLine($"   * DO NOT USE; Please check the documentation for its replaced");
                            content.AppendLine($"   * @deprecated");
                        }

                        content.AppendLine($"   */");
                        wroteSummary = true;
                    }

                    string propertyType = property.OverrideTypeName();
                    if (string.IsNullOrEmpty(propertyType))
                    {
                        propertyType = Utils.ConvertToJsType(_context, property.PropertyType, (t) =>
                        {
                            string name = t.ModelName();
                            // Do not process generic type parameters
                            if (!t.IsGenericParameter)
                            {
                                // Determine whether the type referenced in this model as already been imported
                                if (!imports.ContainsKey(name))
                                {
                                    _context.RegisterType(t);
                                    imports.Add(name, t);
                                }
                            }
                            return name;
                        });
                    }

                    string propName = property.CodeGenName();
                    content.AppendLine($"  {propName.LowerFirstChar()}{Nullable(property)}: {propertyType};");
                });
            content.AppendLine($"}}");
        }

        private static string Nullable(PropertyInfo property)
        {
            bool isNullable = property.IsNullable();
            if (isNullable)
            {
                return "?";
            }

            return "";
        }

        public void UpdateIndex()
        {
            string outputFilePath = Path.Combine(_context.Settings.OutputRootDir, _context.Settings.ModelOutputDir, "index.ts");
            string tagStartText = "/***** [GENERATED FILES::START] *******************************************************************/";
            string tagEndText = "/***** [GENERATED FILES::END] *********************************************************************/";
            string noteText = "\r\n//NOTE: content in the following section of code is automatically generated.  Do not remove comments \r\n//      defining the start and end of the section.  Do not add any custom code inside the section of\r\n//      code because it will be removed during the code generation process.\r\n\r\n";
            StringBuilder content = new StringBuilder();

            if (File.Exists(outputFilePath))
            {
                content.Append(File.ReadAllText(outputFilePath));
            }

            Utils.UpdateTaggedSectionInText(content, tagStartText, tagEndText, (insertionPoint) =>
            {
                // Add back all the auto generated import statements.  We do this backwards to avoid having to keep
                // track of the insertion point as text is written into the string builder.
                _typesProcessed
                    .Select(t => Utils.ModelName(t))
                    .OrderByDescending(n => n)
                    .ForEach((modelName, index) =>
                    {
                        content.Insert(insertionPoint, $"export * from './{modelName}.model';\r\n");
                    });
            }, noteText);

            File.WriteAllText(outputFilePath, content.ToString());
        }
    }
}
