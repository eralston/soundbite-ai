using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Masticore.CodeGen.Models
{
    public class CodeGenSettings
    {
        public string[] AssemblyPaths = new string[] { };
        public string OutputRootDir = "c:\\Code\\Soundbite\\Soundbite\\Soundbite.App\\ClientApp\\src\\";
        public string ControllerOutputDir = "services";
        public string ModelOutputDir = "models";
        public string EnumOutputDir = "enums";
        public string XmlDocFile = "C:\\Code\\Soundbite\\Soundbite\\Soundbite.Common\\Soundbite.Common.xml";
        public string ControllerBaseClass = "Service";
    }

    /// <summary>
    /// Carries the execution context for the CodeGen process
    /// </summary>
    public class CodeGenContext
    {
        private readonly IDictionary<string, Type> _registeredTypes = new Dictionary<string, Type>();
        private readonly IList<Type> _nextBatch = new List<Type>();

        public IList<Assembly> AssembliesToScan = new List<Assembly>();
        public IList<Type> ModelsToIgnore = new List<Type>();
        public IDictionary<Type, CodeGenTransform> TypesToTransform = new Dictionary<Type, CodeGenTransform>();

        public CodeGenSettings Settings { get; set; }

        public Type GetTransformedType(Type t)
        {
            Type[] genericTypes = new Type[] {
                typeof(Task<>),
                typeof(List<>),
                typeof(IList<>),
                typeof(IEnumerable<>)
            };

            if (t.IsGenericType)
            {
                Type genericDef = t.GetGenericTypeDefinition();
                if (genericTypes.Contains(genericDef))
                {
                    Type itemType = GetTransformedType(t.GetGenericArguments().First());
                    return genericDef.MakeGenericType(itemType);
                }
            }

            return GetTransformedTypeSimple(t);
        }

        private Type GetTransformedTypeSimple(Type t)
        {
            TypesToTransform.TryGetValue(t, out CodeGenTransform transformInfo);
            return transformInfo == null ? t : transformInfo.TargetType;
        }

        public void RegisterType(Type t)
        {
            // If it's completely ignored, then don't let it in
            if (t.IsIgnored())
            {
                return;
            }

            if (!_registeredTypes.ContainsKey(t.ModelName()))
            {
                // Register the type
                _registeredTypes.Add(t.ModelName(), t);

                // Determine if the type will be switched out for an interface
                CodeGenTransform transformInfo = t.GetCustomAttribute<CodeGenTransform>(false);
                if (transformInfo != null)
                {
                    TypesToTransform.Add(t, transformInfo);
                    RegisterType(transformInfo.TargetType);
                }
                else
                {

                    _nextBatch.Add(t);
                }
            }
        }

        public IEnumerable<Type> GetNextBatch()
        {
            List<Type> result = _nextBatch.ToList();
            _nextBatch.Clear();
            return result;
        }
    }
}