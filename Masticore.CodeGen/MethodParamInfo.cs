using Masticore.CodeGen.Processors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Masticore.CodeGen
{
    public class MethodParamInfo
    {
        public MethodParamInfo() { }

        public MethodParamInfo(ParameterInfo paramInfo)
        {
            ParameterInfo = paramInfo;
            Name = paramInfo.Name;
            ParamType = paramInfo.ParameterType;
            IsComplexQueryStringObject = paramInfo.GetCustomAttribute<FromQueryAttribute>() != null && !IsSimpleType(ParamType);
        }

        /// <summary>
        /// Determines whether the type is a "simple" type.
        /// </summary>
        /// <param name="t">.NET type whose simplicity is being determined.</param>
        /// <returns><c>true</c> if the type is simple otherwise <c>false</c>.</returns>
        private bool IsSimpleType(Type t)
        {
            //NOTE: this is NOT extensive
            return t.IsPrimitive || t == typeof(string);
        }

        public bool IsComplexQueryStringObject { get; protected set; }
        public ParameterInfo ParameterInfo { get; protected set; }
        public string Name { get; set; }
        public Type ParamType { get; set; }
        public ParamLocation Location { get; set; }
        public string RouteText { get; set; }
        public bool AllowUndefined { get; set; }

        public void AppendUrlArguments(ref string result, ref bool hasQueryStringMarker)
        {
            if (IsComplexQueryStringObject)
            {
                AppendComplexArguments(ref result, ref hasQueryStringMarker);
            }
            else
            {
                AppendSimpleArgument(ref result, ref hasQueryStringMarker);
            }
        }

        private void AppendComplexArguments(ref string result, ref bool hasQueryStringMarker)
        {
            string optionalPrefix = AllowUndefined ? "?" : "";
            string optionalSuffix = AllowUndefined ? " ?? \"\"" : "";
            Dictionary<string, string> args = new Dictionary<string, string>();
            Type paramType = ParameterInfo.ParameterType;
            bool hasMarker = hasQueryStringMarker;
            StringBuilder str = new StringBuilder();
            str.Append(hasQueryStringMarker ? "&" : "?");
            paramType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(p => p.CodeGenName())
                .ForEach((property, index) =>
                {
                    string propertyName = Utils.CodeGenName(property).LowerFirstChar();

                    str.Append($"&{propertyName}=${{encodeURIComponent({Name}{optionalPrefix}.{propertyName}{optionalSuffix})}}");
                });

            result += str.ToString();
        }

        private void AppendSimpleArgument(ref string result, ref bool hasQueryStringMarker)
        {
            // If it is optional, then allow fallback to empty string
            string optional = AllowUndefined ? " ?? \"\"" : "";
            switch (Location)
            {
                case ParamLocation.QueryString:
                    result += hasQueryStringMarker ? "&" : "?";
                    hasQueryStringMarker = true;
                    result += $"{Name}=${{encodeURIComponent({Name}{optional})}}";
                    break;
                case ParamLocation.Route:
                    result = result.Replace(RouteText, $"${{encodeURIComponent({Name}{optional})}}");
                    break;
            }
        }
    }
}