using System;
using System.Reflection;
using System.Threading.Tasks;
using HTMLinjectorServices;

namespace HTMLinjectorCL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("\r\nHTMLinjector v" + Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion + "\r\n");

            if (args == null || args.Length != 2)
            {
                Console.WriteLine("Expected 2 arguments, source folder and output folder");
                return;
            }

            string sourceFolderPath = args[0];
            string outputFolderPath = args[1];

            if (String.IsNullOrEmpty(sourceFolderPath)) 
            {
                Console.WriteLine("Missing source folder\r\n");
                return;
            }

            if (String.IsNullOrEmpty(outputFolderPath))
            {
                Console.WriteLine("Missing output folder\r\n");
                return;
            }

            Console.WriteLine("Source folder: " + sourceFolderPath);
            Console.WriteLine("Output folder: " + outputFolderPath + "\r\n");

            CLFolder sourceFolder = new CLFolder(sourceFolderPath);
            CLFolder outputFolder = new CLFolder(outputFolderPath);

            BuildHandler buildHandler = new BuildHandler();
            buildHandler.BuildEvent += (sender, e) => { Console.WriteLine(e.EventMessage); };

            BuildService buildService = new BuildService();

            buildService.DoBuild(sourceFolder, outputFolder, buildHandler).Wait();
        }
    }
}
