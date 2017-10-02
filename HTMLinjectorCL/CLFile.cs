using System.Threading.Tasks;
using System.IO;
using HTMLinjectorServices;

namespace HTMLinjectorCL
{
    /// <summary>
    /// Represents a file in Command Line environment
    /// </summary>
    public class CLFile : IFile
    {
        /// <summary>
        /// Gets the file name not including path
        /// </summary>
        /// <value>The file name</value>
        public string Name 
        { 
            get
            {
                return System.IO.Path.GetFileName(this.Path);
            }
        }

        /// <summary>
        /// The full file name including path
        /// </summary>
        /// <value>The file path</value>
        public string Path { get; internal set; }

        public CLFile(string path)
        {
            this.Path = path;
        }

        /// <summary>
        /// Returns the contents of the file
        /// </summary>
        /// <returns>The text contained in the file</returns>
        public async Task<string> ReadAllTextAsync()
        {
            return await File.ReadAllTextAsync(this.Path);
        }
    }
}
