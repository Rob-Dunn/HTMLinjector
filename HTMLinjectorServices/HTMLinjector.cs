using System.Threading.Tasks;

namespace HTMLinjectorServices
{
    public class HTMLinjector
    {
        public async Task ProcessFiles(IFolder sourceFolder, IFolder outputFolder, IBuildHandler buildHandler)
        {
            ITagServices tagServices = new TagServices(buildHandler);
            ITemplateServices templateService = new TemplateServices(buildHandler, tagServices);

            BuildService buildService = new BuildService(buildHandler, tagServices, templateService);
            await buildService.DoBuild(sourceFolder, outputFolder);
        }
    }
}
