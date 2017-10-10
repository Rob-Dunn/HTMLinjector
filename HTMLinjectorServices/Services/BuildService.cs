using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HTMLinjectorServices.Models;

namespace HTMLinjectorServices
{
    internal class BuildService
    {
        private IBuildHandler BuildHandler { get; set; }
        private ITagServices TagServices { get; set; }
        private ITemplateServices TemplateServices { get; set; }
        private IFolder SourceFolder { get; set; }
        private IFolder OutputFolder { get; set; }
        private Dictionary<string, Template> Templates { get; set; } = new Dictionary<string, Template>();

        public BuildService(IBuildHandler buildHandler, ITagServices tagServices, ITemplateServices templateServices)
        {
            this.BuildHandler = buildHandler;
            this.TagServices = tagServices;
            this.TemplateServices = templateServices;
        }

        /// <summary>
        /// Initiates a build
        /// </summary>
        /// <returns>The build.</returns>
        /// <param name="sourceFolder">A folder containing the source files to be processed</param>
        /// <param name="outputFolder">The folder in which the processed files should be placed</param>
        public async Task DoBuild(IFolder sourceFolder, IFolder outputFolder)
        {
            try
            {
                this.SourceFolder = sourceFolder;
                this.OutputFolder = outputFolder;

                if (!await sourceFolder.Exists(sourceFolder.Path))
                {
                    this.BuildHandler.SignalBuildError("Source folder doesn't seem to exist!");
                }

                if (outputFolder != null && await outputFolder.Exists(this.OutputFolder.Path))
                {
                    // Remove the existing output folder
                    await this.OutputFolder.Delete();
                }

                // Load templates from the source folder
                await this.TemplateServices.LoadTemplatesRecursive(this.SourceFolder, this.Templates);

                // Process HTML files in the source folder and write them out to the output folder
                await this.DoBuildRecursive(this.SourceFolder, this.OutputFolder);
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (BuildException buildException)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                this.BuildHandler.SignalBuildEvent("Processing failed :(");
            }
            catch(Exception exception)
            {
                this.BuildHandler.SignalBuildException(exception);
            }
        }

        private async Task DoBuildRecursive(IFolder currentSourceFolder, IFolder currentOutputFolder)
        {
            // Process files in the child folders
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
                    await this.DoBuildRecursive(childSourceFolder, childOutputFolder);
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
                        // Load html file and do injection as required
                        await this.ProcessHtmlFile(file, currentOutputFolder);
                    }
                    else if (!file.Name.EndsWith(".template", StringComparison.OrdinalIgnoreCase))
                    {
                        // Copy file to the output folder
                        await currentOutputFolder.CopyFileHere(file);
                    }
                }
            }
        }

        private async Task ProcessHtmlFile(IFile sourceFile, IFolder outputFolder)
        {
            this.BuildHandler.SignalBuildEvent("Processing HTML file " + sourceFile.Path);

            string rawHtml = await sourceFile.ReadAllTextAsync();

            // Inject the templates
            Dictionary<string, string> values = new Dictionary<string, string>();
            string outputHtml = null;
            this.TemplateServices.InjectTemplateRecursive(rawHtml, this.Templates, out outputHtml, ref values);

            // Inject template values
            outputHtml = this.TemplateServices.InjectTemplateValues(outputHtml, values);

            // Write out the processed html to the output folder
            await outputFolder.CreateFileAsync(sourceFile.Name, outputHtml);
        }
    }
}
