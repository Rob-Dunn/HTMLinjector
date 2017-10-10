using NUnit.Framework;
using HTMLinjectorServices;
using System.Collections.Generic;

namespace HTMLinjector_NUnit
{
    [TestFixture()]
    public class InjectTemplateValuesTests
    {
        [Test()]
        public void EmptyHtml()
        {
            TemplateServices templateServices = TemplateUtilities.CreateTestTemplateServices();

            string html = "";
            Dictionary<string, string> values = new Dictionary<string, string>();
            string actualHtml = null;

            actualHtml = templateServices.InjectTemplateValues(html, values);

            // Check result
            Assert.AreEqual(html, actualHtml);
        }

        [Test()]
        public void SingleInjectionPointCurly()
        {
            TemplateServices templateServices = TemplateUtilities.CreateTestTemplateServices();

            string html = "some {{{injectionPoint0}}} content";
            Dictionary<string, string> values = new Dictionary<string, string>() { {"injectionPoint0", "injected text"} };
            string actualHtml = null;

            actualHtml = templateServices.InjectTemplateValues(html, values);

            // Check result
            Assert.AreEqual("some injected text content", actualHtml);
        }

        [Test()]
        public void TwoInjectionPointsCurly()
        {
            TemplateServices templateServices = TemplateUtilities.CreateTestTemplateServices();

            string html = "some {{{injectionPoint0}}} content {{{injectionPoint1}}} text";
            Dictionary<string, string> values = new Dictionary<string, string>() { { "injectionPoint0", "injected text" }, { "injectionPoint1", "injected text2" } };
            string actualHtml = null;

            actualHtml = templateServices.InjectTemplateValues(html, values);

            // Check result
            Assert.AreEqual("some injected text content injected text2 text", actualHtml);
        }

        [Test()]
        public void TwoInjectionPointsCurlyStartAndEnd()
        {
            TemplateServices templateServices = TemplateUtilities.CreateTestTemplateServices();

            string html = "{{{injectionPoint0}}} some content text {{{injectionPoint1}}}";
            Dictionary<string, string> values = new Dictionary<string, string>() { { "injectionPoint0", "injected text" }, { "injectionPoint1", "injected text2" } };
            string actualHtml = null;

            actualHtml = templateServices.InjectTemplateValues(html, values);

            // Check result
            Assert.AreEqual("injected text some content text injected text2", actualHtml);
        }

        [Test()]
        public void SingleInjectionPoint()
        {
            TemplateServices templateServices = TemplateUtilities.CreateTestTemplateServices();

            string html = "some <!-- INJECT_TEMPLATE_VALUE valueId='injectionPoint0' --> content";
            Dictionary<string, string> values = new Dictionary<string, string>() { { "injectionPoint0", "injected text" } };
            string actualHtml = null;

            actualHtml = templateServices.InjectTemplateValues(html, values);

            // Check result
            Assert.AreEqual("some injected text content", actualHtml);
        }

        [Test()]
        public void TwoInjectionPoints()
        {
            TemplateServices templateServices = TemplateUtilities.CreateTestTemplateServices();

            string html = "some <!-- INJECT_TEMPLATE_VALUE valueId='injectionPoint0' --> content <!-- INJECT_TEMPLATE_VALUE valueId='injectionPoint1' --> text";
            Dictionary<string, string> values = new Dictionary<string, string>() { { "injectionPoint0", "injected text" }, { "injectionPoint1", "injected text2" } };
            string actualHtml = null;

            actualHtml = templateServices.InjectTemplateValues(html, values);

            // Check result
            Assert.AreEqual("some injected text content injected text2 text", actualHtml);
        }

        [Test()]
        public void TwoInjectionPointsStartEnd()
        {
            TemplateServices templateServices = TemplateUtilities.CreateTestTemplateServices();

            string html = "<!-- INJECT_TEMPLATE_VALUE valueId='injectionPoint0' --> some content text <!-- INJECT_TEMPLATE_VALUE valueId='injectionPoint1' -->";
            Dictionary<string, string> values = new Dictionary<string, string>() { { "injectionPoint0", "injected text" }, { "injectionPoint1", "injected text2" } };
            string actualHtml = null;

            actualHtml = templateServices.InjectTemplateValues(html, values);

            // Check result
            Assert.AreEqual("injected text some content text injected text2", actualHtml);
        }
    }
}