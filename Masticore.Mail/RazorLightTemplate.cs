using RazorLight;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Masticore.Mail
{
    // Adapted from: https://github.com/aspnet/Entropy/blob/dev/samples/Mvc.RenderViewToString/RazorViewToStringRenderer.cs

    public class NoEngineException : Exception { public NoEngineException(string msg) : base(msg) { } }

    /// <summary>
    /// RazorLight template for projects using embedded CSHTML templates
    /// </summary>
    public class RazorLightTextTemplate : ITextTemplate
    {
        /// <summary>
        /// Gets or sets the root type the engine uses to build types
        /// </summary>
        public static void Init(Assembly entryAssembly, Assembly viewAssembly, string rootNamespace)
        {
            if (Engine != null)
            {
                throw new Exception("RazorLightTemplate Already Initialized");
            }

            Engine = new RazorLightEngineBuilder()
                                .SetOperatingAssembly(entryAssembly)
                                .UseEmbeddedResourcesProject(viewAssembly, rootNamespace)
                                .Build();
        }

        /// <summary>
        /// Gets the engine instance, lazy-loading it based on RootType
        /// </summary>
        public static RazorLightEngine Engine { get; set; }

        /// <summary>
        /// Renders a given view
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> RenderAsync<TModel>(string viewName, TModel model)
        {
            if (Engine == null)
            {
                throw new NoEngineException("Attempting to RenderAsync without a RazorLightEngine engine initialized in RazorLightTemplate");
            }

            string result = await Engine.CompileRenderAsync(viewName, model);
            return result;
        }
    }
}
