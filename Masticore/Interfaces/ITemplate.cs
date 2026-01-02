using System.Threading.Tasks;

namespace Masticore
{
    /// <summary>
    /// Object that implements rendering templates to strings EG, CSHTML Razor templates
    /// </summary>
    public interface ITextTemplate
    {
        /// <summary>
        /// Renders a given view using the given model returning a string (could be HTML or text based on the nature of the view)
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<string> RenderAsync<TModel>(string viewName, TModel model);
    }
}