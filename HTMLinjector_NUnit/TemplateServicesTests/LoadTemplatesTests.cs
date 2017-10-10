using NUnit.Framework;
using HTMLinjectorServices;
using System.Collections.Generic;
using HTMLinjectorServices.Models;
using System.Threading.Tasks;

namespace HTMLinjector_NUnit
{
    [TestFixture()]
    public class LoadTemplatesTests
    {
        [Test()]
        public async Task SingleFolderSingleTemplate()
        {
            TemplateServices templateServices = TemplateUtilities.CreateTestTemplateServices();
            Dictionary<string, Template> templates = new Dictionary<string, Template>();

            await templateServices.LoadTemplatesRecursive(new FolderMock() { Name="Folder0", Path="Folder0" }, templates);

            // Check result
            Assert.AreEqual(1, templates.Count);
            Template template = templates["template1"];
        }
    }
}
