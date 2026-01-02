using Masticore.CodeGen.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Masticore.CodeGen.Processors
{
    public class EnumProcessor
    {
        #region Fields

        private readonly CodeGenContext _context;
        private readonly XmlDocHelper _xmlDocHelper;
        private readonly IList<Type> _typesProcessed = new List<Type>();

        #endregion

        public EnumProcessor(CodeGenContext context, XmlDocHelper xmlDocHelper)
        {
            _context = context;
            _xmlDocHelper = xmlDocHelper;

            string enumDirectory = Path.Combine(_context.Settings.OutputRootDir, _context.Settings.EnumOutputDir);
            if (!Directory.Exists(enumDirectory))
            {
                Directory.CreateDirectory(enumDirectory);
            }
        }

        public void ProcessType(Type enumTypeName)
        {
            _typesProcessed.Add(enumTypeName);

            string fileName = Path.Combine(_context.Settings.OutputRootDir, _context.Settings.EnumOutputDir, $"{enumTypeName.Name}.enum.ts");
            StringBuilder content = new StringBuilder();
            Dictionary<string, Type> imports = new Dictionary<string, Type>();

            Utils.WriteSummary(content, _xmlDocHelper.GetSummary(enumTypeName, $"Automatically generated enum for {enumTypeName.ModelNameWithNamespace()}"), 0);
            content.AppendLine($"export enum {enumTypeName.Name} {{");
            content.AppendLine();

            Dictionary<string, int> enumItems = new Dictionary<string, int>();
            foreach (string name in Enum.GetNames(enumTypeName))
            {
                enumItems.Add(name, Convert.ToInt32(Enum.Parse(enumTypeName, name)));
            }

            bool wroteSummary = false;

            int index = 0;
            enumItems.ToList().ForEach((enumItem) =>
            {
                if (wroteSummary)
                {
                    content.AppendLine();
                    wroteSummary = false;
                }

                string summary = _xmlDocHelper.GetEnumSummary(enumTypeName, enumItem.Key, null);
                if (!string.IsNullOrEmpty(summary))
                {
                    Utils.WriteSummary(content, summary, 2);
                    wroteSummary = true;
                }

                NumberValueForEnum(enumItem, content, enumItems, index);

                index++;
            });

            content.AppendLine();
            content.AppendLine($"}}");

            Utils.AppendAutoGenWarning(content);
            File.WriteAllText(fileName, content.ToString());
        }

        protected static void NumberValueForEnum(KeyValuePair<string, int> enumItem, StringBuilder content, Dictionary<string, int> enumItems, int index)
        {
            content.AppendLine($"  {enumItem.Key} = {enumItem.Value}{(index == enumItems.Count - 1 ? "" : ",")}");
        }

        protected static void StringValueForEnum(KeyValuePair<string, int> enumItem, StringBuilder content, Dictionary<string, int> enumItems, int index)
        {
            content.AppendLine($"  {enumItem.Key} = '{enumItem.Key}'{(index == enumItems.Count - 1 ? "" : ",")}");
        }

        public void UpdateIndex()
        {
            string outputFilePath = Path.Combine(_context.Settings.OutputRootDir, _context.Settings.EnumOutputDir, "index.ts");
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
                _typesProcessed.OrderByDescending(t => t.Name).ToList().ForEach((modelType) =>
            {
                content.Insert(insertionPoint, $"export * from './{modelType.Name}.enum';\r\n");
            });
            }, noteText);

            File.WriteAllText(outputFilePath, content.ToString());
        }
    }
}