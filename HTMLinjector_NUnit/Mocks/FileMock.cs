using System;
using System.Threading.Tasks;
using HTMLinjectorServices;

namespace HTMLinjector_NUnit
{
    public class FileMock : IFile
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public async Task<string> ReadAllTextAsync()
        {
            string text = "";

            switch (this.Name)
            {
                case "Folder0File0.notatemplate":
                    text = "this is not a template file";
                    break;

                case "Folder0File1.template":
                    text = "<!-- TEMPLATE_START id='template1' -->template1 contents<!-- TEMPLATE_END -->";
                    break;
            }

            return text;
        }
    }
}
