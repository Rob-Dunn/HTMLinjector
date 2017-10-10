using NUnit.Framework;
using HTMLinjectorServices;
using System.Collections.Generic;
using HTMLinjectorServices.Models;

namespace HTMLinjector_NUnit
{
    [TestFixture()]
    public class InjectTemplateTests
    {
        [Test()]
        public void EmptyHtml()
        {
            TemplateServices templateServices = TemplateUtilities.CreateTestTemplateServices();

            string rawHtml = "";
            Dictionary<string, Template> templates = new Dictionary<string, Template>();
            string actualHtml = null;
            Dictionary<string, string> actualValues = new Dictionary<string, string>();

            templateServices.InjectTemplateRecursive(rawHtml, templates, out actualHtml, ref actualValues);

            // Check result
            Assert.AreEqual(rawHtml, actualHtml);
            Assert.AreEqual(0, actualValues.Count);
        }

        [Test()]
        public void NoTemplates()
        {
            TemplateServices templateServices = TemplateUtilities.CreateTestTemplateServices();

            string rawHtml = "some content";
            Dictionary<string, Template> templates = new Dictionary<string, Template>();
            string actualHtml = null;
            Dictionary<string, string> actualValues = new Dictionary<string, string>();

            templateServices.InjectTemplateRecursive(rawHtml, templates, out actualHtml, ref actualValues);

            // Check result
            Assert.AreEqual(rawHtml, actualHtml);
            Assert.AreEqual(0, actualValues.Count);
        }

        [Test()]
        public void SingleTemplate()
        {
            TemplateServices templateServices = TemplateUtilities.CreateTestTemplateServices();

            string rawHtml = "some <!-- INJECT_TEMPLATE templateId='template0' --> content";
            Dictionary<string, Template> templates = TemplateUtilities.CreateTestTemplates();
            string actualHtml = null;
            Dictionary<string, string> actualValues = new Dictionary<string, string>();

            templateServices.InjectTemplateRecursive(rawHtml, templates, out actualHtml, ref actualValues);

            // Check result
            string expectedHTML = "some template0 content text content";

            Assert.AreEqual(expectedHTML, actualHtml);
            Assert.AreEqual(0, actualValues.Count);
        }

        [Test()]
        public void SingleTemplateSingleValue()
        {
            TemplateServices templateServices = TemplateUtilities.CreateTestTemplateServices();

            string rawHtml = "some <!-- INJECT_TEMPLATE templateId='template1' template1_value0='injected text' --> content";
            Dictionary<string, Template> templates = TemplateUtilities.CreateTestTemplates();
            string actualHtml = null;
            Dictionary<string, string> actualValues = new Dictionary<string, string>();

            templateServices.InjectTemplateRecursive(rawHtml, templates, out actualHtml, ref actualValues);

            // Check result
            string expectedHTML = "some template1 content text content";

            Assert.AreEqual(expectedHTML, actualHtml);
            Assert.AreEqual(1, actualValues.Count);

            Assert.AreEqual("injected text", actualValues["template1_value0"]);
        }

        // TODO: Templates within templates
    }
}
