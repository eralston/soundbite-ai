using Masticore.CodeGen.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Masticore.CodeGen
{
    /// <summary>
    /// Utility methods supporting, especially those interacting with categorizing fields and types
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Gets the name of the model for the given type, falling back through each option
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string ModelName(this Type type)
        {
            CodeGenModelAttribute attr = type.CodeGenModel();
            if (attr != null && !string.IsNullOrEmpty(attr.Name))
            {
                return attr.Name;
            }

            return TypeName(type);
        }

        /// <summary>
        /// Gets the original classname for this type (no check for <see cref="CodeGenModel(Type)"/> attribute)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static string TypeName(Type type)
        {
            if (type.Name != null)
            {
                string className = type.Name.Split('`')[0];
                return className;
            }

            throw new Exception("Could not find class name for type");
        }

        /// <summary>
        /// <see cref="ModelName(Type)"/>, but with any potentially type argument added on
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public static string ModelNameWithOptionalGeneric(this Type modelType)
        {
            StringBuilder builder = new StringBuilder($"{modelType.ModelName()}");

            if (modelType.IsGenericType)
            {
                builder.Append("<");
                Type[] genericArgs = modelType.GetGenericArguments();
                builder.Append(string.Join(", ", genericArgs.Select(i => i.ModelName())));
                builder.Append(">");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Returns the full name of the model, including namespace
        /// </summary>
        /// <param name="dotNetType"></param>
        /// <returns></returns>
        public static string ModelNameWithNamespace(this Type dotNetType)
        {
            return dotNetType.Namespace + "." + ModelName(dotNetType);
        }

        /// <summary>
        /// Gets the original classname for this type (no check for <see cref="CodeGenModel(Type)"/> attribute) plus namespace
        /// </summary>
        /// <param name="dotNetType"></param>
        /// <returns></returns>
        public static string TypeNameWithNamespace(this Type dotNetType)
        {
            return dotNetType.Namespace + "." + TypeName(dotNetType);
        }

        /// <summary>
        /// Returns true if the given <see cref="PropertyInfo"/> can carry undefined in Typescript
        /// If the underlying type <see cref="Nullable"/> then return true.
        /// If the property has a <see cref="CodeGenFieldAttribute.IsNullable"/> set to true, then return true.
        /// Otherwise, return false - it is NOT nullable in that case.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static bool IsNullable(this PropertyInfo propertyInfo)
        {
            bool isNullable = Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null;
            if (isNullable)
            {
                return true;
            }

            CodeGenFieldAttribute attr = propertyInfo.CodeGenField();
            if (attr != null && attr.IsNullable)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns true if the given <see cref="ParameterInfo"/> can carry undefined in Typescript
        /// If the underlying type <see cref="Nullable"/> then return true.
        /// If the property has a <see cref="CodeGenFieldAttribute.IsNullable"/> set to true, then return true.
        /// Otherwise, return false - it is NOT nullable in that case.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static bool IsNullable(this ParameterInfo paramInfo)
        {
            bool isNullable = Nullable.GetUnderlyingType(paramInfo.ParameterType) != null;
            if (isNullable)
            {
                return true;
            }

            CodeGenFieldAttribute attr = paramInfo.CodeGenField();
            if (attr != null && attr.IsNullable)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the given parameter should be ignored
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <returns></returns>
        public static bool IsIgnored(this ParameterInfo paramInfo)
        {
            if (paramInfo == null)
            {
                return true;
            }

            CodeGenFieldAttribute attr = paramInfo.CodeGenField();
            if (attr != null && attr.Ignore)
            {
                return true;
            }

            FromServicesAttribute fromServices = paramInfo.GetCustomAttribute<FromServicesAttribute>();
            if (fromServices != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the optional overriding type name for the given <see cref="ParameterInfo"/>
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <returns></returns>
        public static string OverrideTypeName(this ParameterInfo paramInfo)
        {
            // If CodeGenField
            CodeGenFieldAttribute codeGen = paramInfo.CodeGenField();
            if (codeGen != null && !string.IsNullOrEmpty(codeGen.TypeName))
            {
                return codeGen.TypeName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns true if the given method can return a null.
        /// If the method has a <see cref="CodeGenMethodAttribute.CanReturnNull"/> set to true on it, then true.
        /// Otherwise, return false.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static bool CanReturnNull(this MethodInfo methodInfo)
        {
            CodeGenMethodAttribute attr = methodInfo.CodeGenMethod();
            if (attr != null && attr.CanReturnNull)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reads the name of the given property.
        /// If the propery has a <see cref="CodeGenFieldAttribute.Name"/>, it will use that.
        /// If not present, then it will look for <see cref="JsonPropertyAttribute.PropertyName"/> and use its value.
        /// Finally, it will fallback to the <see cref="PropertyInfo"/>.Name field field, which is the actual name in the C# code for the property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string CodeGenName(this PropertyInfo property)
        {
            // Default is the property name; third priority
            string name = property.Name;

            // JsonProperty name is second priority
            Newtonsoft.Json.JsonPropertyAttribute newtonsoftJsonProp = property.GetCustomAttribute<Newtonsoft.Json.JsonPropertyAttribute>();
            if (newtonsoftJsonProp != null && newtonsoftJsonProp.PropertyName != null)
            {
                name = newtonsoftJsonProp.PropertyName;
            }

            System.Text.Json.Serialization.JsonPropertyNameAttribute jsonProp = property.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>();
            if (jsonProp != null && jsonProp.Name != null)
            {
                name = jsonProp.Name;
            }

            // Also check CodeGen; number one priority
            CodeGenFieldAttribute codeGenProp = property.GetCustomAttribute<CodeGenFieldAttribute>();
            if (codeGenProp != null && codeGenProp.Name != null)
            {
                name = codeGenProp.Name;
            }

            return name;
        }

        /// <summary>
        /// Returns true if the given <see cref="PropertyInfo"/> is ignored based on the following criteria:
        /// Annotated with <see cref="CodeGenFieldAttribute.Ignore"/> set to true.
        /// Annoated with <see cref="JsonIgnoreAttribute"/> present.
        /// Otherwise, return false
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static bool IsIgnored(this PropertyInfo prop)
        {
            // If CodeGenField
            CodeGenFieldAttribute codeGen = prop.CodeGenField();
            if (codeGen != null)
            {
                // When a property has a CodeGenField attribute respect the Ignore property.
                // Note: this allows us to generate one-direction properties allowing sensitive data
                // (e.g. passwords/keys/etc) to be sent from the client to the server but not from
                // the server back to the client
                return codeGen.Ignore;
            }

            // Models internal a project should probably use Newtonsoft singe it's faster
            bool isNewtonsoftJsonIgnore = Attribute.IsDefined(prop, typeof(Newtonsoft.Json.JsonIgnoreAttribute));
            if (isNewtonsoftJsonIgnore)
            {
                return true;
            }

            // Models passed back out of the ASP.Net Core API should use the built-in since Newtonsoft seems fallible
            bool isJsonIgnore = Attribute.IsDefined(prop, typeof(System.Text.Json.Serialization.JsonIgnoreAttribute));
            if (isJsonIgnore)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// If the property explicitly defines the name of its own type, then return that value; otherwise, return null
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static string OverrideTypeName(this PropertyInfo prop)
        {
            // If CodeGenField
            CodeGenFieldAttribute codeGen = prop.CodeGenField();
            if (codeGen != null && !string.IsNullOrEmpty(codeGen.TypeName))
            {
                return codeGen.TypeName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns true if the given <see cref="Type"/> is ignored based on the following criteria:
        /// Annotated with <see cref="CodeGenModelAttribute.Ignore"/> set to true.
        /// Otherwise, return false
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsIgnored(this Type type)
        {
            // If CodeGenField
            CodeGenModelAttribute codeGen = type.CodeGenModel();
            if (codeGen != null && codeGen.Ignore)
            {
                return true;
            }

            return false;
        }

        public static CodeGenMethodAttribute CodeGenMethod(this MethodInfo method)
        {
            return method.GetCustomAttribute<CodeGenMethodAttribute>();
        }

        public static CodeGenFieldAttribute CodeGenField(this ParameterInfo param)
        {
            return param.GetCustomAttribute<CodeGenFieldAttribute>();
        }

        public static CodeGenFieldAttribute CodeGenField(this PropertyInfo prop)
        {
            return prop.GetCustomAttribute<CodeGenFieldAttribute>();
        }

        public static CodeGenModelAttribute CodeGenModel(this Type type)
        {
            return type.GetCustomAttribute<CodeGenModelAttribute>();
        }

        public static void AppendAutoGenWarning(StringBuilder content)
        {
            content.Insert(0, "/***************************************************************************************************\r\n" +
                " * Auto-Generated File - do not modify because all changes will be lost\r\n" +
                " **************************************************************************************************/\r\n\r\n");
        }

        public static void WriteSummary(StringBuilder content, string summary, int indentCount, bool isDeprecated = false)
        {
            string indent = new string(' ', indentCount);
            content.AppendLine($"{indent}/** ");
            string[] summaryLines = summary.Split("\r\n");
            summaryLines.ToList().ForEach(summaryLine => content.AppendLine($"{indent}* {summaryLine.Trim()}"));
            if (isDeprecated)
            {
                content.AppendLine($"{indent}* DO NOT USE; please check the documentation for its replacement");
                content.AppendLine($"{indent}* @deprecated");
            }
            content.AppendLine($"{indent}*/");
        }

        public static string ConvertToJsType(CodeGenContext context, Type dotNetType, Func<Type, string> onNonPrimativeType)
        {
            Type transformedType = context.GetTransformedType(dotNetType);

            if (transformedType != dotNetType)
            {
                return ConvertToJsType(context, transformedType, onNonPrimativeType);
            }
            else if (dotNetType.IsArray)
            {
                return ConvertToJsType(context, dotNetType.GetElementType(), onNonPrimativeType) + "[]";
            }
            else
            {
                switch (dotNetType?.Name)
                {
                    case "Task":
                        // Task is normally encoutned on an async void method so there is no return type
                        return null;
                    case "Task`1":
                        return ConvertToJsType(context, dotNetType.GenericTypeArguments[0], onNonPrimativeType);
                    case "IEnumerable`1":
                    case "iList`1":
                    case "IList`1":
                    case "ReadOnlyCollection`1":
                        return ConvertToJsType(context, dotNetType.GenericTypeArguments[0], onNonPrimativeType) + "[]";
                    case "Nullable`1":
                        return ConvertToJsType(context, dotNetType.GenericTypeArguments[0], onNonPrimativeType);
                    case "Boolean": return "boolean";
                    case "String": return "string";
                    case "DateTime": return "string";   // Newtonsoft renders dates as ISO string
                    case "Int16":
                    case "Int32":
                    case "Int64":
                    case "Decimal":
                    case "Double":
                    case "Single":
                        return "number";
                    case "JsonPatchDocument":
                    case "Stream":
                    case "Object":
                        return "any";
                    default:
                        return onNonPrimativeType(dotNetType);
                }
            }
        }

        public static void UpdateTaggedSectionInText(StringBuilder content, string tagStartText, string tagEndText, string sectionText, string noteText = null, Func<int> getDefaultInsertIndex = null)
        {
            UpdateTaggedSectionInText(content, tagStartText, tagEndText, sectionText, null, noteText, getDefaultInsertIndex);
        }

        public static void UpdateTaggedSectionInText(StringBuilder content, string tagStartText, string tagEndText, Action<int> updateSectionText, string noteText = null, Func<int> getDefaultInsertIndex = null)
        {
            UpdateTaggedSectionInText(content, tagStartText, tagEndText, null, updateSectionText, noteText, getDefaultInsertIndex);
        }

        private static void UpdateTaggedSectionInText(StringBuilder content, string tagStartText, string tagEndText, string sectionText, Action<int> updateSectionText, string noteText = null, Func<int> getDefaultInsertIndex = null)
        {
            // When no insertion point function is provided just assume it goes at the end of the file
            getDefaultInsertIndex ??= new Func<int>(() => { return content.Length; });
            bool addNote = false;

            // Look for data insertion point inside of the index file:
            int tagStart = content.ToString().IndexOf(tagStartText);
            int tagEnd = content.ToString().IndexOf(tagEndText);

            // Determine whether data insertion point needs to be added to the file
            if (tagStart == -1)
            {
                if (tagEnd == -1)
                {
                    tagStart = content.ToString().Length;
                    content.Insert(tagStart, tagStartText);
                }
                else
                {
                    tagStart = tagEnd;
                    content.Insert(tagStart, tagStartText);
                    tagEnd += tagStartText.Length;
                }
                addNote = true;
            }

            // Make sure the tag ending is NOT before
            while (tagEnd != -1 && tagEnd < tagStart)
            {
                content.Remove(tagEnd, tagEndText.Length);
                tagEnd = content.ToString().IndexOf(tagEndText);
            }

            // Make sure the tag ending exists
            if (tagEnd == -1)
            {
                // If it does not then put it RIGHT after the starting tag
                tagEnd = tagStart + tagStartText.Length;
                content.Insert(tagEnd, tagEndText + "\r\n");
            }

            // Remove everything between the tags
            content.Remove(tagStart + tagStartText.Length, tagEnd - (tagStart + tagStartText.Length));

            // Add updated content
            if (updateSectionText != null)
            {
                updateSectionText(tagStart + tagStartText.Length);
            }
            else
            {
                content.Insert(tagStart + tagStartText.Length, sectionText);
            }

            // Ensure the ending tag ends up on a separate line
            content.Insert(tagStart + tagStartText.Length, "\r\n");

            if (addNote)
            {
                content.Insert(tagStart, noteText);
            }
        }

    }
}