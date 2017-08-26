using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HTMLinjectorServices
{
    public class BuildEventArgs : EventArgs
    {
        public string EventMessage { get; set; } 
    }

    public class Template
    {
        public string Filename { get; set; }
        public string Id { get; set; }
        public string Text { get; set; }
    }

    public class Tag
    {
        public string Name { get; set; }
        public string Contents { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        public string SearchText { get; set; }
        public bool HasContent { get; set; }

        public TagPosition StartTagPosition { get; set; }
        public TagPosition EndTagPosition { get; set; }
    }

    public class TagPosition
    {
        public int StartPosition { get; set; } = 0;
        public int EndPosition { get; set; } = 0;
        public int StartEndPosition { get; set; } = 0;
    }

    public class BuildException: Exception
    {}

    public class BuildHandler
    {
        public event EventHandler<BuildEventArgs> BuildEvent;

        public bool HasError { get; internal set; }

        public void SignalBuildEvent(string eventMessage)
        {
            if (BuildEvent != null)
            {
                BuildEvent(null, new BuildEventArgs() { EventMessage = eventMessage });
            }
        }

        public void SignalBuildError(string eventMessage)
        {
            HasError = true;

            SignalBuildEvent(eventMessage);

            throw new BuildException();
        }

        public void SignalBuildException(Exception exception)
        {
            HasError = true;

            string eventMessage = exception.Message;

            SignalBuildEvent(eventMessage);

            throw new BuildException();
        }
    }

    public class BuildService
    {
        private BuildHandler BuildHandler { get; set; }
        private IFolder SourceFolder { get; set; }
        private IFolder OutputFolder { get; set; }
        private Dictionary<string, Template> Templates { get; set; } = new Dictionary<string, Template>();

        public async Task DoBuild(IFolder sourceFolder, IFolder outputFolder, BuildHandler buildHandler)
        {
            try
            {
                BuildHandler = buildHandler;
                SourceFolder = sourceFolder;
                OutputFolder = outputFolder;

                if (!await sourceFolder.Exists(sourceFolder.Path))
                {
                    BuildHandler.SignalBuildError("Source folder doesn't seem to exist!");
                }

                if (outputFolder != null && await outputFolder.Exists(outputFolder.Path))
                {
                    // Remove the existing output folder
                    await outputFolder.Delete();
                }

                // Load templates
                await LoadTemplatesRecursive(sourceFolder);

                // Process HTML files
                await DoBuildRecursive(sourceFolder, outputFolder);
            }
            catch (BuildException buildException)
            {
                BuildHandler.SignalBuildEvent("Processing failed :(");
            }
            catch(Exception exception)
            {
                BuildHandler.SignalBuildException(exception);
            }
        }

        private async Task<IFolder> FindChildFolder(string name, IFolder parentFolder)
        {
            IFolder foundChildFolder = null;

            List<IFolder> folders = await parentFolder.GetChildFoldersAsync();
            if (folders != null)
            {
                foreach (IFolder childFolder in folders)
                {
                    if (childFolder.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        foundChildFolder = childFolder;
                        break;
                    }
                }
            }

            return foundChildFolder;
        }

        private async Task LoadTemplatesRecursive(IFolder currentFolder)
        {
            List<IFolder> folders = await currentFolder.GetChildFoldersAsync();
            if (folders != null)
            {
                foreach (IFolder childFolder in folders)
                {
                    await LoadTemplatesRecursive(childFolder);
                }
            }

            List<IFile> files = await currentFolder.GetFilesAsync();
            if (files != null)
            {
                foreach (IFile file in files)
                {
                    if (file.Name.EndsWith(".template", StringComparison.OrdinalIgnoreCase))
                    {
                        // Load html file
                        await LoadTemplate(file);
                    }
                }
            }
        }

        private async Task DoBuildRecursive(IFolder currentSourceFolder, IFolder currentOutputFolder)
        {
            // Process the child folders
            List<IFolder> childSourceFolders = await currentSourceFolder.GetChildFoldersAsync();
            if (childSourceFolders != null)
            {
                foreach (IFolder childSourceFolder in childSourceFolders)
                {
                    // Does the corresponding child output folder exist?
                    IFolder childOutputFolder = await currentOutputFolder.GetChildFolderAsync(childSourceFolder.Name);
                    if (childOutputFolder == null)
                    {
                        // No corresponding child folder in the output folder so create one 
                        childOutputFolder = await currentOutputFolder.CreateChildFolder(childSourceFolder.Name);
                    }

                    // Build the child folder
                    await DoBuildRecursive(childSourceFolder, childOutputFolder);
                }
            }

            // Process the files in this folder
            List<IFile> files = await currentSourceFolder.GetFilesAsync();
            if (files != null)
            {
                foreach (IFile file in files)
                {
                    if (file.Name.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                    {
                        // Load html file
                        await ProcessHtmlFile(file, currentOutputFolder);
                    }
                    else if (!file.Name.EndsWith(".template", StringComparison.OrdinalIgnoreCase))
                    {
                        // Copy file to the output folder
                        await CopyFileToOutput(file, currentOutputFolder);
                    }
                }
            }
        }

        private async Task LoadTemplate(IFile file)
        {
            BuildHandler.SignalBuildEvent("Loading template " + file.Path);

            string text = await file.ReadAllTextAsync();

            // Look for and extract the template tag
            List<Tag> tags = null;
            if (!ExtractTags(text, "TEMPLATE", true, out tags)) BuildHandler.SignalBuildError("Missing TEMPLATE tag");

            foreach (Tag tag in tags)
            {
                // Get the Id from the tag properties
                string id = null;
                if (!tag.Properties.TryGetValue("id", out id)) BuildHandler.SignalBuildError("Missing id property for template tag");

                Template template = new Template()
                {
                    Id = id,
                    Text = tag.Contents
                };

                // Add the template
                Templates.Add(id, template);
            }
        }

        private bool ExtractTags(string searchText, string tagName, bool hasContent, out List<Tag> tags)
        {
            tags = new List<Tag>();

            int startPosition = 0;

            while (true)
            {
                Tag tag = new Tag
                {
                    Name = tagName,
                    SearchText = searchText,
                    HasContent = hasContent
                };

                // See if we can find the tag
                if (!ExtractTag(tag, startPosition)) return true;

                if (tag.StartTagPosition == null) break; // Tag not found // TODO: Find a better way to determine if a tag was found

                // Extract content
                if (tag.HasContent)
                {
                    if (!ExtractTagContent(tag)) return false;
                }

                // Successful
                tags.Add(tag);

                // Start looking AFTER the last tag we found
                startPosition = tag.HasContent ? tag.EndTagPosition.EndPosition : tag.StartTagPosition.EndPosition;
            }

            return true;
        }

        private bool ExtractTag(Tag tag, int currentPosition)
        {
            // Find the tag
            TagPosition startTagPosition = null;
            if (!FindTagPosition(tag.SearchText, currentPosition, tag.Name + (tag.HasContent ? "_START" : ""), out startTagPosition)) return false;
            if (startTagPosition == null) return true; // Tag not found

            tag.StartTagPosition = startTagPosition;

            // Extract the values
            if (!GetTagValues(tag)) return false;

            return true;
        }

        private bool FindTagPosition(string searchText, int startPosition, string tagName, out TagPosition tagPosition)
        {
            tagPosition = null;

            string startTagText = "<!-- " + tagName + " ";
            int tagStartStartIndex = searchText.IndexOf(startTagText, startPosition, StringComparison.OrdinalIgnoreCase);
            if (tagStartStartIndex == -1) return true; // tag not found

            string startTagEndText = " -->";
            int tagStartEndIndex = searchText.IndexOf(startTagEndText, tagStartStartIndex, StringComparison.OrdinalIgnoreCase);
            if (tagStartEndIndex == -1) BuildHandler.SignalBuildError(tagName + " tag not closed");

            tagPosition = new TagPosition();
            tagPosition.StartPosition = tagStartStartIndex;
            tagPosition.StartEndPosition = tagStartStartIndex + startTagText.Length;
            tagPosition.EndPosition = tagStartEndIndex + startTagEndText.Length;

            return true;
        }

        private bool ExtractTagContent(Tag tag)
        {
            // Find the end tag
            string endTagName = tag.Name + "_END";

            TagPosition endTagPosition = null;
            if (!FindTagPosition(tag.SearchText, tag.StartTagPosition.EndPosition, endTagName, out endTagPosition)) return false;

            if (endTagPosition == null) BuildHandler.SignalBuildError("Missing end tag");

            tag.EndTagPosition = endTagPosition;

            // Get all the text inbetween the start and end tags - that's the content
            int contentLength = tag.EndTagPosition.StartPosition - tag.StartTagPosition.EndPosition;
            tag.Contents = tag.SearchText.Substring(tag.StartTagPosition.EndPosition, contentLength);

            return true;
        }

        private bool GetTagValues(Tag tag)
        {
            // Get tag key value pairs
            int position = tag.StartTagPosition.StartEndPosition;
            return GetNextKeyValuePair(tag, ref position);
        }

        private bool GetNextKeyValuePair(Tag tag, ref int position)
        {
           while (true)
            {
                // We're potentially at the start of the next value or key/value pair
                // We need to figure out if we've got to the end of the tag or if there's a value or a key/value pair
                int nextEqualsIndex = tag.SearchText.IndexOf("=", position, StringComparison.OrdinalIgnoreCase);

                // Any more values or value key pairs?
                if (nextEqualsIndex == -1 || nextEqualsIndex > tag.StartTagPosition.EndPosition)
                {
                    // Finished processing values
                    break;
                }

                if (nextEqualsIndex != -1)
                {
                    // Next character is an equals.  It has to be a key value pair
                    if (!GetTagKeyValuePair(tag, nextEqualsIndex, ref position)) return false;
                }
            }

            return true;
        }

        private bool GetTagKeyValuePair(Tag tag, int nextEqualsIndex, ref int position)
        {
            // Get the key
            int keyLength = nextEqualsIndex - position;
            string key = tag.SearchText.Substring(position, keyLength).Trim();

            // Get the value
            int firstSingleQuoteIndex = tag.SearchText.IndexOf("'", nextEqualsIndex, StringComparison.OrdinalIgnoreCase);
            if (firstSingleQuoteIndex == -1) BuildHandler.SignalBuildError("Missing first single quote");

            int secondSingleQuoteIndex = tag.SearchText.IndexOf("'", firstSingleQuoteIndex + 1, StringComparison.OrdinalIgnoreCase);
            if (secondSingleQuoteIndex == -1) BuildHandler.SignalBuildError("Missing second single quote");

            int valueLength = secondSingleQuoteIndex - firstSingleQuoteIndex - 1;
            string value = tag.SearchText.Substring(firstSingleQuoteIndex + 1, valueLength);

            // Add it to the tags values
            tag.Properties.Add(key, value);

            // Move the position along
            position = secondSingleQuoteIndex + 1;

            return true;
        }

        private async Task CopyFileToOutput(IFile file, IFolder outputFolder)
        {
            await outputFolder.CopyFileHere(file);
        }

        private async Task ProcessHtmlFile(IFile sourceFile, IFolder outputFolder)
        {
            BuildHandler.SignalBuildEvent("Processing HTML file " + sourceFile.Path);

            string rawHtml = await sourceFile.ReadAllTextAsync();

            // Inject the templates
            Dictionary<string, string> values = new Dictionary<string, string>();
            string outputHtml = null;
            InjectTemplateRecursive(rawHtml, out outputHtml, ref values);

            // Inject template values
            InjectTemplateValues(outputHtml, values, out outputHtml);

            // Write out the html file
            await outputFolder.CreateFileAsync(sourceFile.Name, outputHtml);
        }

        private void InjectTemplateRecursive(string rawHtml, out string outputHtml, ref Dictionary<string, string> values)
        {
            outputHtml = null;

            // Find all the places in the html that we need to inject into
            List<Tag> tags = null;
            ExtractTags(rawHtml, "INJECT_TEMPLATE", false, out tags);
            
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
                                BuildHandler.SignalBuildError("Duplicate template value " + keyValuePair.Key);
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
                        BuildHandler.SignalBuildError("Missing templateId property for inject_template tag");
                    }

                    // Find the template that needs to be injected
                    Template template = null;
                    if (!Templates.TryGetValue(templateId, out template))
                    {
                        BuildHandler.SignalBuildError("Unknown template id " + templateId);
                    }

                    // Inject the template into the html
                    outputHtmlBuilder.Append(template.Text);

                    // Skip past the tag
                    endOfLastTag = tag.HasContent ? tag.EndTagPosition.EndPosition : tag.StartTagPosition.EndPosition;
                }

                // Output the remainder of the html
                outputHtmlBuilder.Append(rawHtml.Substring(endOfLastTag));

                InjectTemplateRecursive(outputHtmlBuilder.ToString(), out outputHtml, ref values);
            }
        }

        private void InjectTemplateValues(string html, Dictionary<string, string> values, out string outputHtml)
        {
            outputHtml = null;

            // Find all the places in the html that we need to inject values into
            List<Tag> tags = null;
            if (!ExtractTags(html, "INJECT_TEMPLATE_VALUE", false, out tags)) return;

            if (tags.Count == 0)
            {
                // No injecting needed
                outputHtml = html;
            }
            else
            {
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
                        BuildHandler.SignalBuildError("Missing valueId property for INJECT_TEMPLATE_VALUE tag");
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

                // Replace html attributes                   
                foreach (KeyValuePair<string, string> property in values)
                {
                    outputHtml = outputHtml.Replace("injectTemplateValue=\"" + property.Key + "\"", property.Value); //, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
    }
}
