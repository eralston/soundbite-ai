using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Masticore.CodeGen
{
    public class XmlDocHelper
    {
        #region Fields

        private readonly XDocument _xmlDoc;

        #endregion

        /// <summary>
        /// Initializes a new <see cref="XmlDocHelper"/> instances.
        /// </summary>
        /// <param name="xmlDocumentationFilePath">File path to the XML documentation file to read.</param>
        public XmlDocHelper(string xmlDocumentationFilePath)
        {
            _xmlDoc = XDocument.Load(xmlDocumentationFilePath);
        }

        public string GetSummary(Type classType, string defaultSummary = null)
        {
            if (classType.FullName == null)
            {
                return "";
            }

            string key = $"T:{classType.FullName.Replace("+", ".")}";
            XElement members = _xmlDoc.Root.Element("members");
            XElement memberNode = _xmlDoc.Root?.Element("members")?.Elements().FirstOrDefault(i => i.Attribute("name")?.Value == key);
            string summary = (memberNode == null ? defaultSummary : memberNode.Element("summary")?.Value)?.Trim().Replace("\r\n", "\n").Replace("\n", "\r\n");
            return summary;
        }

        public string GetSummary(Type classType, PropertyInfo propertyInfo, string defaultSummary = null)
        {
            string key = $"P:{classType.FullName.Replace("+", ".")}.{propertyInfo.Name}";
            XElement members = _xmlDoc.Root.Element("members");
            XElement memberNode = _xmlDoc.Root?.Element("members")?.Elements().FirstOrDefault(i => i.Attribute("name")?.Value == key);
            string summary = (memberNode == null ? defaultSummary : memberNode.Element("summary")?.Value)?.Trim().Replace("\r\n", "\n").Replace("\n", "\r\n");
            return summary;
        }

        public string GetSummary(Type classType, MethodInfo methodInfo, string defaultSummary = null)
        {
            string key = $"M:{classType.FullName.Replace("+", ".")}.{methodInfo.Name}";
            XElement members = _xmlDoc.Root.Element("members");
            XElement memberNode = _xmlDoc.Root?.Element("members")?.Elements().FirstOrDefault(i => i.Attribute("name")?.Value == key);
            string summary = (memberNode == null ? defaultSummary : memberNode.Element("summary")?.Value)?.Trim().Replace("\r\n", "\n").Replace("\n", "\r\n");
            return summary;
        }

        public string GetEnumSummary(Type enumType, string enumItemName, string defaultSummary = null)
        {
            string key = $"F:{enumType.FullName.Replace("+", ".")}.{enumItemName}";
            XElement members = _xmlDoc.Root.Element("members");
            XElement memberNode = _xmlDoc.Root?.Element("members")?.Elements().FirstOrDefault(i => i.Attribute("name")?.Value == key);
            string summary = (memberNode == null ? defaultSummary : memberNode.Element("summary")?.Value)?.Trim().Replace("\r\n", "\n").Replace("\n", "\r\n");
            return summary;
        }
    }
}
