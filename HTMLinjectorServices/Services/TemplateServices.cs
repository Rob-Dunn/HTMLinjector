using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HTMLinjectorServices.Models;

namespace HTMLinjectorServices
{
    internal class TemplateServices : ITemplateServices
    {
        private IBuildHandler BuildHandler { get; set; }
        private ITagServices TagServices { get; set; }

        public TemplateServices(IBuildHandler buildHandler, ITagServices tagServices)
        {
            this.BuildHandler = buildHandler;
            this.TagServices = tagServices;
        }

        /// <summary>
        /// Loads template files from the specified folder and child folders
        /// </summary>
        /// <returns>An awaitable Task</returns>
        /// <param name="currentFolder">The folder to load templates from</param>
        /// <param name="templates">A list of templates to be populated</param>
        public async Task LoadTemplatesRecursive(IFolder currentFolder, Dictionary<string, Template> templates)
        {
            // Recursively load template files from child folders
            List<IFolder> folders = await currentFolder.GetChildFoldersAsync();
            if (folders != null)
            {
                foreach (IFolder childFolder in folders)
                {
                    await this.LoadTemplatesRecursive(childFolder, templates);
                }
            }

            // Load templates from the specified folder
            List<IFile> files = await currentFolder.GetFilesAsync();
            if (files != null)
            {
                foreach (IFile file in files)
                {
                    if (file.Name.EndsWith(".template", StringComparison.OrdinalIgnoreCase))
                    {
                        // Load html file
                        Template template = await this.LoadTemplate(file);
                        templates.Add(template.Id, template);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the specified template file
        /// </summary>
        /// <returns>The template.</returns>
        /// <param name="file">File.</param>
        private async Task<Template> LoadTemplate(IFile file)
        {
            this.BuildHandler.SignalBuildEvent("Loading template " + file.Path);

            string text = await file.ReadAllTextAsync();

            // Look for and extract the template tag from the file contents
            List<Tag> tags = this.TagServices.ExtractTags(text, "TEMPLATE", true);
            if (tags == null || tags.Count == 0) this.BuildHandler.SignalBuildError("Missing TEMPLATE tag");

            Template template = null;

            foreach (Tag tag in tags)
            {
                // Get the Id from the tag properties
                string id = null;
                if (!tag.Properties.TryGetValue("id", out id)) this.BuildHandler.SignalBuildError("Missing id property for template tag");

                template = new Template()
                {
                    Id = id,
                    Text = tag.Contents
                };
            }

            return template;
        }

        /// <summary>
        /// Searches HTML for injection points and injects template HTML, recursively
        /// </summary>
        /// <param name="rawHtml">The HTML to inject templates in to</param>
        /// <param name="outputHtml">The processed HTML, including injected templates</param>
        /// <param name="values">A list of injection values that can be injected into the templates</param>
        public void InjectTemplateRecursive(string rawHtml, Dictionary<string, Template> templates, out string outputHtml, ref Dictionary<string, string> values)
        {
            outputHtml = null;

            // Find all the places in the html that we need to inject into
            List<Tag> tags = this.TagServices.ExtractTags(rawHtml, "INJECT_TEMPLATE", false);

            if (tags == null || tags.Count == 0)
            {
                // No injecting needed
                outputHtml = rawHtml;
                return;
            }

            int endOfLastTag = 0;
            StringBuilder outputHtmlBuilder = new StringBuilder();

            foreach (Tag tag in tags)
            {
                // Get values to inject into the HTML after the template HTML has been injected
                foreach (KeyValuePair<string, string> keyValuePair in tag.Properties)
                {
                    if (keyValuePair.Key != "templateId")
                    {
                        string dummy = null;
                        if (values.TryGetValue(keyValuePair.Key, out dummy))
                        {
                            this.BuildHandler.SignalBuildError("Duplicate template value " + keyValuePair.Key);
                        }
                        values.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }

                // Write out everything before the tag
                int length = tag.StartTagPosition.StartPosition - endOfLastTag;
                outputHtmlBuilder.Append(rawHtml.Substring(endOfLastTag, length));

                // Get the template id from the tag properties
                string templateId = null;
                if (!tag.Properties.TryGetValue("templateId", out templateId))
                {
                    this.BuildHandler.SignalBuildError("Missing templateId property for inject_template tag");
                }

                // Find the template that needs to be injected
                Template template = null;
                if (!templates.TryGetValue(templateId, out template))
                {
                    this.BuildHandler.SignalBuildError("Unknown template id " + templateId);
                }

                // Inject the template into the html
                outputHtmlBuilder.Append(template.Text);

                // Skip past the tag
                endOfLastTag = tag.HasContent ? tag.EndTagPosition.EndPosition : tag.StartTagPosition.EndPosition;
            }

            // Output the remainder of the html
            outputHtmlBuilder.Append(rawHtml.Substring(endOfLastTag));

            // Attempt to inject any more templates - this supports templates within templates...
            this.InjectTemplateRecursive(outputHtmlBuilder.ToString(), templates, out outputHtml, ref values);
        }

        /// <summary>
        /// Injects values into HTML that has had templates injected into it
        /// </summary>
        /// <returns>The result of the value injection</returns>
        /// <param name="html">The HTML containing injected templates</param>
        /// <param name="values">A list of values to inject into the templates</param>
        public string InjectTemplateValues(string html, Dictionary<string, string> values)
        {
            string outputHtml = this.InjectTagValues(html, values);

            outputHtml = this.InjectCurlyValues(outputHtml, values);

            // Remove unused injection points
            this.RemoveUnusedCurlyValuesRecursive(0, ref outputHtml);

            return outputHtml;
        }

        /// <summary>
        /// Injects comment based tag values
        /// </summary>
        /// <returns>HTML after injecting values</returns>
        /// <param name="html">Html.</param>
        /// <param name="values">Values.</param>
        private string InjectTagValues(string html, Dictionary<string, string> values)
        {
            // Find all the places in the html that we need to inject values into
            List<Tag> tags = this.TagServices.ExtractTags(html, "INJECT_TEMPLATE_VALUE", false);

            if (tags == null || tags.Count == 0)
            {
                // No injecting needed
                return html;
            }

            int endOfLastTag = 0;
            StringBuilder outputHtmlBuilder = new StringBuilder();

            foreach (Tag tag in tags)
            {
                // Write out everything before the tag
                int length = tag.StartTagPosition.StartPosition - endOfLastTag;
                outputHtmlBuilder.Append(html.Substring(endOfLastTag, length));

                // Get the value id from the tag properties
                string valueId = null;
                if (!tag.Properties.TryGetValue("valueId", out valueId))
                {
                    this.BuildHandler.SignalBuildError("Missing valueId property for INJECT_TEMPLATE_VALUE tag");
                }

                // Find the value that needs to be injected
                string value = null;
                if (values.TryGetValue(valueId, out value))
                {
                    outputHtmlBuilder.Append(value);
                }

                // Skip past the tag
                endOfLastTag = tag.HasContent ? tag.EndTagPosition.EndPosition : tag.StartTagPosition.EndPosition;
            }

            // Output the remainder of the html
            outputHtmlBuilder.Append(html.Substring(endOfLastTag));

            return outputHtmlBuilder.ToString();
        }

        /// <summary>
        /// Injects curly brace based values
        /// </summary>
        /// <returns>HTML after injecting values</returns>
        /// <param name="html">Html.</param>
        /// <param name="values">Values.</param>
        private string InjectCurlyValues(string html, Dictionary<string, string> values)
        {
            // Inject values                   
            foreach (KeyValuePair<string, string> property in values)
            {
                html = html.Replace("{{{" + property.Key + "}}}", property.Value);
            }

            return html;
        }

        /// <summary>
        /// Removes the unused curly values recursively
        /// </summary>
        /// <param name="startIndex">Position in the HTML to start looking for the next injection point</param>
        /// <param name="outputHtml">HTML after injecting values</param>
        private void RemoveUnusedCurlyValuesRecursive(int startIndex, ref string outputHtml)
        {
            // Find the next open tag
            int openTagIndex = outputHtml.IndexOf("{{{", startIndex, StringComparison.Ordinal);
            if (openTagIndex > -1)
            {
                // Find the next close tag
                int closeTagIndex = outputHtml.IndexOf("}}}", openTagIndex, StringComparison.Ordinal);
                if (closeTagIndex > -1)
                {
                    // Remove everything inbetween the start and end tags
                    string newOutputHtml = outputHtml.Substring(0, openTagIndex);
                    newOutputHtml += outputHtml.Substring(closeTagIndex + 3);

                    outputHtml = newOutputHtml;

                    // See if there are any more injection tags to be removed
                    this.RemoveUnusedCurlyValuesRecursive(openTagIndex, ref outputHtml);
                }
            }
        }
    }
}
