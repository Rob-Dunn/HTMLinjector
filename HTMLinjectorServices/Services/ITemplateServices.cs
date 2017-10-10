using System.Collections.Generic;
using System.Threading.Tasks;
using HTMLinjectorServices.Models;

namespace HTMLinjectorServices
{
    public interface ITemplateServices
    {
        /// <summary>
        /// Loads template files from the specified folder and child folders
        /// </summary>
        /// <returns>An awaitable Task</returns>
        /// <param name="currentFolder">The folder to load templates from</param>
        /// <param name="templates">A list of templates to be populated</param>
        Task LoadTemplatesRecursive(IFolder currentFolder, Dictionary<string, Template> templates);

        /// <summary>
        /// Searches HTML for injection points and injects template HTML, recursively
        /// </summary>
        /// <param name="rawHtml">The HTML to inject templates in to</param>
        /// <param name="outputHtml">The processed HTML, including injected templates</param>
        /// <param name="values">A list of injection values that can be injected into the templates</param>
        void InjectTemplateRecursive(string rawHtml, Dictionary<string, Template> templates, out string outputHtml, ref Dictionary<string, string> values);

        /// <summary>
        /// Injects values into HTML that has had templates injected into it
        /// </summary>
        /// <param name="html">The HTML containing injected templates</param>
        /// <param name="values">A list of values to inject into the templates</param>
        /// <param name="outputHtml">The result of the value injection</param>
        string InjectTemplateValues(string html, Dictionary<string, string> values);
    }
}
