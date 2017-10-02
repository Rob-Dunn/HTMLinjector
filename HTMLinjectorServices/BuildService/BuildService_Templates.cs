using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HTMLinjectorServices.Models;

namespace HTMLinjectorServices
{
    public partial class BuildService
    {
        /// <summary>
        /// Loads template files from the specified folder and child folders
        /// </summary>
        /// <returns>An awaitable Task</returns>
        /// <param name="currentFolder">The folder to load templates from</param>
        private async Task LoadTemplatesRecursive(IFolder currentFolder)
        {
            // Recursively load template files from child folders
            List<IFolder> folders = await currentFolder.GetChildFoldersAsync();
            if (folders != null)
            {
                foreach (IFolder childFolder in folders)
                {
                    await this.LoadTemplatesRecursive(childFolder);
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
                        await this.LoadTemplate(file);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the specified template file
        /// </summary>
        /// <returns>The template.</returns>
        /// <param name="file">File.</param>
        private async Task LoadTemplate(IFile file)
        {
            this.BuildHandler.SignalBuildEvent("Loading template " + file.Path);

            string text = await file.ReadAllTextAsync();

            // Look for and extract the template tag from the file contents
            List<Tag> tags = this.ExtractTags(text, "TEMPLATE", true);
            if (tags == null || tags.Count == 0) this.BuildHandler.SignalBuildError("Missing TEMPLATE tag");

            foreach (Tag tag in tags)
            {
                // Get the Id from the tag properties
                string id = null;
                if (!tag.Properties.TryGetValue("id", out id)) this.BuildHandler.SignalBuildError("Missing id property for template tag");

                Template template = new Template()
                {
                    Id = id,
                    Text = tag.Contents
                };

                // Add the template
                this.Templates.Add(id, template);
            }
        }

        /// <summary>
        /// Searches HTML for injection points and injects template HTML, recursively
        /// </summary>
        /// <param name="rawHtml">The HTML to inject templates in to</param>
        /// <param name="outputHtml">The processed HTML, including injected templates</param>
        /// <param name="values">A list of injection values that can be injected into the templates</param>
        private void InjectTemplateRecursive(string rawHtml, out string outputHtml, ref Dictionary<string, string> values)
        {
            outputHtml = null;

            // Find all the places in the html that we need to inject into
            List<Tag> tags = this.ExtractTags(rawHtml, "INJECT_TEMPLATE", false);

            if (tags == null || tags.Count == 0)
            {
                // No injecting needed
                outputHtml = rawHtml;
                return;
            }
            else
            {
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
                    if (!Templates.TryGetValue(templateId, out template))
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
                this.InjectTemplateRecursive(outputHtmlBuilder.ToString(), out outputHtml, ref values);
            }
        }

        /// <summary>
        /// Injects values into HTML that has had templates injected into it
        /// </summary>
        /// <param name="html">The HTML containing injected templates</param>
        /// <param name="values">A list of values to inject into the templates</param>
        /// <param name="outputHtml">The result of the value injection</param>
        private void InjectTemplateValues(string html, Dictionary<string, string> values, out string outputHtml)
        {
            outputHtml = null;

            // Find all the places in the html that we need to inject values into
            List<Tag> tags = this.ExtractTags(html, "INJECT_TEMPLATE_VALUE", false);

            if (tags == null || tags.Count == 0)
            {
                // No injecting needed
                outputHtml = html;
                return;
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

            outputHtml = outputHtmlBuilder.ToString();

            // Inject values                   
            foreach (KeyValuePair<string, string> property in values)
            {
                outputHtml = outputHtml.Replace("{{{" + property.Key + "}}}", property.Value, StringComparison.OrdinalIgnoreCase);
            }

            // Remove unused injection points
            this.RemoveUnusedInjectionPointsRecursive(0, ref outputHtml);
        }

        private void RemoveUnusedInjectionPointsRecursive(int startIndex, ref string outputHtml)
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
                    RemoveUnusedInjectionPointsRecursive(openTagIndex, ref outputHtml);
                }
            }
        }
    }
}
