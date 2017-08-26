using System;
using System.Threading.Tasks;
using System.IO;
using HTMLinjectorServices;

namespace HTMLinjectorCL
{
    public class CLFile : IFile
    {
        /// <summary>
        /// Gets the file name not including path
        /// </summary>
        /// <value>The name.</value>
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
        public string Path { get; internal set; }

        public CLFile(string path)
        {
            this.Path = path;
        }

        public async Task<string> ReadAllTextAsync()
        {
            return await File.ReadAllTextAsync(this.Path);
        }
    }
}
