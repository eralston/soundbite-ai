using Masticore.CodeGen.Enums;
using System.Reflection;

namespace Masticore.CodeGen.Models
{
    public class HttpMethodInfo
    {
        public HttpMethodInfo(MethodInfo methodInfo, HttpMethodType methodType, string routeTemplate)
        {
            MethodType = methodType;
            MethodInfo = methodInfo;
            RouteTemplate = routeTemplate;
        }

        public HttpMethodType MethodType { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public string RouteTemplate { get; set; }
    }
}