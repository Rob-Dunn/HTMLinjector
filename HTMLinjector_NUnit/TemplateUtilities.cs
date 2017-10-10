using System;
using System.Collections.Generic;
using HTMLinjectorServices;
using HTMLinjectorServices.Models;
using NUnit.Framework;

namespace HTMLinjector_NUnit
{
    internal class TemplateUtilities
    {
        internal static TemplateServices CreateTestTemplateServices()
        {
            BuildHandler buildHandler = new BuildHandler();
            TagServices tagServices = new TagServices(buildHandler);

            return new TemplateServices(buildHandler, tagServices);
        }

        internal static Dictionary<string, Template> CreateTestTemplates()
        {
            Dictionary<string, Template> templates = new Dictionary<string, Template>();

            templates.Add("template0", new Template()
            {
                Filename = "somefilename0",
                Id = "template0",
                Text = "template0 content text"
            });

            templates.Add("template1", new Template()
            {
                Filename = "somefilename1",
                Id = "template1",
                Text = "template1 content text"
            });

            return templates;
        }
    }
}
