using Masticore.CodeGen.Enums;
using Masticore.CodeGen.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Masticore.CodeGen.Processors
{
    public class ControllerProcessor
    {
        #region Fields

        /// <summary>
        /// Stores a list of the HTTP attributes the controller processor "looks" for when 
        /// determining whether a method is an HTTP method.
        /// </summary>
        private static readonly List<Type> _httpAttributes = new List<Type>() {
            typeof(HttpGetAttribute),
            typeof(HttpDeleteAttribute),
            typeof(HttpPostAttribute),
            typeof(HttpPutAttribute),
            typeof(HttpPatchAttribute)
        };

        private readonly CodeGenContext Context;
        private readonly XmlDocHelper _xmlDocHelper;
        private readonly IList<Type> _typesProcessed = new List<Type>();

        private string _currentController;

        #endregion

        #region Constructors

        public ControllerProcessor(CodeGenContext settings, XmlDocHelper xmlDocHelper)
        {
            Context = settings;
            _xmlDocHelper = xmlDocHelper;
            string controllerDir = Path.Combine(Context.Settings.OutputRootDir, Context.Settings.ControllerOutputDir);
            if (!Directory.Exists(controllerDir))
            {
                Directory.CreateDirectory(controllerDir);
            }
        }

        #endregion

        public void ProcessType(Type controllerType)
        {
            _typesProcessed.Add(controllerType);
            _currentController = controllerType.Name;

            string serviceName = controllerType.Name.Replace("Controller", string.Empty);
            string fileName = Path.Combine(Context.Settings.OutputRootDir, Context.Settings.ControllerOutputDir, $"{serviceName.ToLower()}.service.ts");
            StringBuilder content = new StringBuilder();
            IEnumerable<HttpMethodInfo> httpMethods = GetHttpMethods(controllerType);
            RouteAttribute routeAttribute = controllerType.GetCustomAttribute<RouteAttribute>();
            string route = routeAttribute?.Template ?? "[controller]";
            Dictionary<string, Type> imports = new Dictionary<string, Type>();
            List<string> importLines = new List<string>();
            route = route.Replace("[controller]", serviceName);

            bool isDeprecated = controllerType.GetCustomAttribute<ObsoleteAttribute>() != null;

            int importsInsertionIndex = AppendStaticImports(content);
            content.AppendLine();
            content.AppendLine($"/**");
            content.AppendLine($" * Automatically generated endpoint API for the {controllerType.ModelNameWithNamespace()}");
            if (isDeprecated)
            {
                content.AppendLine($" * DO NOT USE; please examine the documentation to find its replacement");
                content.AppendLine($" * @deprecated");
            }
            content.AppendLine($" **/");
            content.AppendLine($"class {serviceName}ServiceClass {{");
            content.AppendLine();
            httpMethods.ForEach((httpMethod, index) =>
            {
                if (index > 0)
                {
                    content.AppendLine();
                }

                ProcessHttpMethod(content, controllerType, httpMethod, imports, importLines);
            });
            content.AppendLine($"}}");
            content.AppendLine();
            content.AppendLine($"export const {serviceName}Service = new {serviceName}ServiceClass();");

            // Import enumerations (if any)
            IEnumerable<string> enums = imports.Where(i => i.Value.IsEnum).OrderBy(i => i.Value.Name).Select(i => i.Value.Name);
            if (enums.Any())
            {
                content.Insert(importsInsertionIndex, $"import {{ {string.Join(", ", enums)} }} from '../enums/index';\r\n");
            }

            // Import models (if any)
            IEnumerable<string> models = imports
                .Where(i => !i.Value.IsEnum)
                .Select(i => Utils.ModelName(i.Value))
                .OrderBy(n => n);

            if (models.Any())
            {
                content.Insert(importsInsertionIndex, $"import {{ {string.Join(", ", models)} }} from '../models/index';\r\n");
            }

            // Import enumerations (if any)
            importLines.OrderBy(i => i).ForEach(importLine =>
            {
                content.Insert(importsInsertionIndex, $"{importLine}\r\n");
            });

            Utils.AppendAutoGenWarning(content);
            File.WriteAllText(fileName, content.ToString());
        }

        private static int AppendStaticImports(StringBuilder content)
        {
            content.AppendLine("import { HttpService } from '../../code/HttpService';");
            content.AppendLine("import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';");
            content.AppendLine("import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';");
            int importsInsertionIndex = content.Length;
            return importsInsertionIndex;
        }

        private HttpMethodType GetHttpMethodType(Type attribute)
        {
            if (typeof(HttpDeleteAttribute).IsAssignableFrom(attribute)) { return HttpMethodType.Delete; }
            if (typeof(HttpGetAttribute).IsAssignableFrom(attribute)) { return HttpMethodType.Get; }
            if (typeof(HttpPostAttribute).IsAssignableFrom(attribute)) { return HttpMethodType.Post; }
            if (typeof(HttpPutAttribute).IsAssignableFrom(attribute)) { return HttpMethodType.Put; }
            if (typeof(HttpPatchAttribute).IsAssignableFrom(attribute)) { return HttpMethodType.Patch; }
            throw new Exception("Cannot map attribute to HttpMethodType");
        }

        private IEnumerable<MethodParamInfo> GetParamInfo(MethodInfo method, string routeTemplate)
        {
            // {andhere(?::.*?)?}
            List<MethodParamInfo> results = new List<MethodParamInfo>();
            method.GetParameters().ForEach(param =>
            {
                if (param.IsIgnored())
                {
                    return;
                }

                MethodParamInfo item = new MethodParamInfo(param);
                Regex regex = new Regex($"{{{param.Name}(?::.*?)?}}");
                Match match = regex.Match(routeTemplate);
                if (match.Success)
                {
                    item.RouteText = match.Value;
                    item.Location = ParamLocation.Route;
                }
                else
                {
                    FromBodyAttribute fromBodyAttribute = param.GetCustomAttribute<FromBodyAttribute>();
                    if (fromBodyAttribute != null)
                    {
                        item.Location = ParamLocation.Body;
                    }
                    else
                    {
                        item.Location = ParamLocation.QueryString;
                    }

                    item.AllowUndefined = param.IsNullable();
                }
                results.Add(item);
            });
            return results;
        }

        private IEnumerable<HttpMethodInfo> GetHttpMethods(Type controllerType)
        {
            List<HttpMethodInfo> httpMethods = new List<HttpMethodInfo>();
            controllerType.GetMethods().ForEach(method =>
            {
                bool isAdded = false;
                IEnumerable<Attribute> methodAttributes = method.GetCustomAttributes();
                methodAttributes.ForEach(attribute =>
                {
                    Type attributeType = attribute.GetType();
                    if (!isAdded && _httpAttributes.Any(i => i.IsAssignableFrom(attributeType)))
                    {
                        string routeTemplate = "/";
                        Attribute routeAttribute = methodAttributes.FirstOrDefault(i => i.GetType().IsAssignableFrom(typeof(RouteAttribute)));
                        if (routeAttribute != null)
                        {
                            routeTemplate = (routeAttribute as RouteAttribute)?.Template;
                        }
                        httpMethods.Add(new HttpMethodInfo(method, GetHttpMethodType(attributeType), routeTemplate));
                        isAdded = true;
                    }
                });
            });
            return httpMethods;
        }

        private string LowerCaseFirstLetter(string s)
        {
            return s[0].ToString().ToLower() + s.Substring(1);
        }

        private string BuildEndPointUrlString(string routeTemplate, IEnumerable<MethodParamInfo> methodParams)
        {
            string result = $"`{routeTemplate}";
            bool hasQueryStringMarker = false;
            methodParams.ForEach(methodParam =>
            {
                methodParam.AppendUrlArguments(ref result, ref hasQueryStringMarker);
            });
            result += "`";
            return result;
        }

        private void ProcessHttpMethod(StringBuilder content, Type controllerType, HttpMethodInfo httpMethod, IDictionary<string, Type> imports, IList<string> importLines)
        {
            CodeGenMethodAttribute codeGenAttr = httpMethod.MethodInfo.GetCustomAttribute<CodeGenMethodAttribute>();
            string controllerName = controllerType.Name;
            string methodName = LowerCaseFirstLetter(httpMethod.MethodInfo.Name);
            string returnType = codeGenAttr?.ClientScriptReturnType ?? ConvertToScriptType(httpMethod.MethodInfo.ReturnType, imports);
            string apiCallReturnType = returnType;
            bool isClientOpWrapper = returnType?.StartsWith("ClientOpWrapper<") == true;
            returnType = isClientOpWrapper ? returnType.Substring(16, returnType.Length - 17) : returnType;
            IEnumerable<MethodParamInfo> methodParams = GetParamInfo(httpMethod.MethodInfo, httpMethod.RouteTemplate);
            MethodParamInfo bodyParam = methodParams.FirstOrDefault(p => p.Location == ParamLocation.Body);
            bool isDeprecated = httpMethod.MethodInfo.GetCustomAttribute<ObsoleteAttribute>() != null;
            Utils.WriteSummary(content, _xmlDocHelper.GetSummary(controllerType, httpMethod.MethodInfo, "Automatically generated API call"), 2, isDeprecated);
            content.Append($"  async {methodName}(");

            methodParams.ForEach((methodParam, index) =>
            {
                if (index > 0) { content.Append(", "); }
                string optional = methodParam.AllowUndefined ? "?" : "";

                string tsType = ConvertToScriptType(Context.GetTransformedType(methodParam.ParamType), imports);
                string defaultValue = DefaultValueToTs(methodParam);
                if (string.IsNullOrEmpty(defaultValue))
                {
                    content.Append($"{methodParam.Name}{optional}: {tsType}");
                }
                else
                {
                    content.Append($"{methodParam.Name}{optional}: {tsType} = {defaultValue}");
                }
            });

            bool hasParameters = methodParams?.Any() == true;

            bool isBlobUpload = codeGenAttr?.Scenario == CodeGenScenarioType.FileUploadSingleFromBlob;
            if (isBlobUpload)
            {
                if (httpMethod.MethodType != HttpMethodType.Post)
                {
                    throw new Exception("FileUploadSingleFromBlob scenario is only valid on a POST method.");
                }

                if (hasParameters)
                {
                    content.Append(", ");
                }

                content.Append($"blob: Blob");
                bodyParam = new MethodParamInfo() { Name = "blob" };
            }

            bool isFormData = codeGenAttr?.Scenario == CodeGenScenarioType.FileUploadSingleFromFormData;
            if (isFormData)
            {
                if (httpMethod.MethodType != HttpMethodType.Post)
                {
                    throw new Exception("FileUploadSingleFromFormData scenario is only valid on a POST method.");
                }
                if (methodParams?.Any() == true)
                {
                    content.Append(", ");
                }

                content.Append($"formData: FormData");
                bodyParam = new MethodParamInfo() { Name = "formData" };
            }

            // Every method should also take an options object
            if (hasParameters || isFormData || isBlobUpload)
            {
                content.Append(", ");
            }
            content.AppendLine("options?: IHttpRequestOptions");

            if (returnType != null)
            {
                string retUndef = codeGenAttr?.CanReturnNull == true ? " | undefined" : "";
                content.AppendLine($"): Promise<{returnType}{retUndef}> {{");
            }
            else
            {
                content.AppendLine($"): Promise<void> {{");
            }

            AppendMethodBody(content, httpMethod, returnType, apiCallReturnType, isClientOpWrapper, methodParams, bodyParam);

            content.AppendLine($"  }}");
        }

        /// <summary>
        /// Reads the default value of the given parameter in a format Typescript might understand as a variable (EG, quoted for strings; not quoted for non-strings; null for no default)
        /// </summary>
        /// <param name="methodParam"></param>
        /// <returns></returns>
        private static string DefaultValueToTs(MethodParamInfo methodParam)
        {
            object defaultValue = methodParam.ParameterInfo.DefaultValue;
            if (defaultValue == null)
            {
                return null;
            }

            if (methodParam.ParameterInfo.ParameterType.IsEnum)
            {
                return $"{methodParam.ParameterInfo.ParameterType.Name}.{defaultValue}";
            }

            // Check if it's a 
            if (defaultValue is string)
            {
                string ret = $"\"{defaultValue}\"";
                return ret;
            }

            if (defaultValue is bool)
            {
                return ((bool)defaultValue) ? "true" : "false";
            }

            string strValue = defaultValue.ToString();
            if (string.IsNullOrEmpty(strValue))
            {
                return null;
            }

            if (strValue.Equals("False") || strValue.Equals("True"))
            {
                strValue = strValue.ToLower();
            }

            return strValue;
        }

        private void AppendMethodBody(StringBuilder content, HttpMethodInfo httpMethod, string returnType, string apiCallReturnType, bool isClientOpWrapper, IEnumerable<MethodParamInfo> methodParams, MethodParamInfo bodyParam)
        {
            content.AppendLine($"    const url = SoundbiteApiConfig.ApiPrefixUrl + {BuildEndPointUrlString(httpMethod.RouteTemplate, methodParams)};");

            switch (httpMethod.MethodType)
            {
                case HttpMethodType.Get:
                    content.AppendLine($"    {(apiCallReturnType != null ? "const response = " : null)}await HttpService.get{(apiCallReturnType != null ? $"<{apiCallReturnType}>" : "")}(url, options);");
                    break;
                case HttpMethodType.Delete:
                    content.AppendLine($"    {(apiCallReturnType != null ? "const response = " : null)}await HttpService.delete{(apiCallReturnType != null ? $"<{apiCallReturnType}>" : "")}(url, options);");
                    break;
                case HttpMethodType.Post:
                    content.AppendLine($"    {(apiCallReturnType != null ? "const response = " : null)}await HttpService.post{(apiCallReturnType != null ? $"<{apiCallReturnType}>" : "")}(url, {NullableBodyParamValue(bodyParam)}, options);");

                    break;
                case HttpMethodType.Put:
                    content.AppendLine($"    {(apiCallReturnType != null ? "const response = " : null)}await HttpService.put{(apiCallReturnType != null ? $"<{apiCallReturnType}>" : "")}(url, {NullableBodyParamValue(bodyParam)}, options);");
                    break;
                case HttpMethodType.Patch:
                    content.AppendLine($"    {(apiCallReturnType != null ? "const response = " : null)}await HttpService.patch{(apiCallReturnType != null ? $"<{apiCallReturnType}>" : "")}(url, {NullableBodyParamValue(bodyParam)}, options);");
                    break;
                default:
                    content.AppendLine($"    [UNKNOWN HTTP ACTION - {httpMethod.MethodType}];");
                    break;
            }

            if (returnType != null)
            {
                if (isClientOpWrapper)
                {
                    // Make sure the client op service is being imported
                    content.AppendLine($"    await SoundbiteApiConfig.clientOpsHandler(response?.clientOps);");
                    content.AppendLine($"    return response.result;");
                }
                else
                {
                    content.AppendLine($"    return response;");
                }
            }
        }

        private static string NullableBodyParamValue(MethodParamInfo bodyParam)
        {
            if (bodyParam?.Name == null)
            {
                return "null";
            }

            if (bodyParam.AllowUndefined)
            {
                return $"{bodyParam.Name} ?? null";
            }
            else
            {
                return bodyParam.Name;
            }
        }

        private string ResolveTypeName(Type dotNetType, IDictionary<string, Type> imports)
        {
            if (dotNetType.IsGenericType)
            {
                Type[] genericArgs = dotNetType.GetGenericArguments();
                string name = dotNetType.Name.Split('`')[0] + "<" + string.Join(", ", genericArgs.Select(i => ResolveTypeName(i, imports)).ToArray()) + ">";
                genericArgs.Where(i => !i.IsGenericType).ForEach(i =>
                {
                    ConvertToScriptType(i, imports);
                });
                return name;
            }
            else
            {
                return dotNetType.Name;
            }
        }

        private void AddImportLine(IList<string> importLines, string importLine)
        {
            if (!importLines.Contains(importLine))
            {
                importLines.Add(importLine);
            }
        }

        private void AddToImports(Type dotNetType, IDictionary<string, Type> imports)
        {
            dotNetType = Context.GetTransformedType(dotNetType);
            if (!imports.ContainsKey(Utils.ModelNameWithNamespace(dotNetType)))
            {
                Context.RegisterType(dotNetType);
                imports.Add(Utils.ModelNameWithNamespace(dotNetType), dotNetType);
            }
        }

        private string ConvertToScriptType(Type t, IDictionary<string, Type> imports)
        {
            t = Context.GetTransformedType(t);
            return Utils.ConvertToJsType(Context, t, (dotNetType) =>
            {
                if (dotNetType.IsGenericType)
                {
                    AddToImports(dotNetType, imports);
                    return ResolveTypeName(dotNetType, imports);
                }
                else
                {
                    AddToImports(dotNetType, imports);
                    return dotNetType.Name;
                }
            });
        }

        public void UpdateIndex()
        {
            string outputFilePath = Path.Combine(Context.Settings.OutputRootDir, Context.Settings.ControllerOutputDir, "index.ts");
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
                _typesProcessed.OrderByDescending(i => i.Name).ForEach((controllerType, index) =>
                {
                    content.Insert(insertionPoint, $"export * from './{controllerType.Name.Replace("Controller", string.Empty).ToLower()}.service';\r\n");
                });
            }, noteText);

            File.WriteAllText(outputFilePath, content.ToString());
        }
    }
}