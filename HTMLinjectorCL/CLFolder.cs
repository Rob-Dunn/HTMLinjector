using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using HTMLinjectorServices;

namespace HTMLinjectorCL
{
    /// <summary>
    /// Represents a folder in Command Line environment
    /// </summary>
    public class CLFolder : IFolder
    {
        /// <summary>
        /// Gets the folder name excluding the path
        /// </summary>
        /// <remarks>If the folder path was "c:/folder/subfolder" the Name would be "subfolder"</remarks>
        /// <value>The folder name</value>
        public string Name 
        { 
            get
            {
                int lastSeperator = this.Path.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
                if (lastSeperator > -1)
                {
                    return this.Path.Substring(lastSeperator + 1);
                }
                else
                {
                    return this.Path;
                }
            }
        }

        /// <summary>
        /// The full folder name including path
        /// </summary>
        /// <remarks>If the folder path was "c:/folder/subfolder" the Path would be "c:/folder/subfolder"</remarks>
        public string Path { get; internal set; }

        public CLFolder(string path)
        {
            this.Path = path;
        }

        /// <summary>
        /// Returns a list of child folders
        /// </summary>
        /// <returns>A list of child folders</returns>
        public async Task<List<IFolder>> GetChildFoldersAsync()
        {
            List<IFolder> childCLFolders = new List<IFolder>();

            string[] childFolderPaths = await Task.FromResult<string[]>( Directory.GetDirectories(this.Path) );
            
            foreach(string childFolderPath in childFolderPaths)
            {
                childCLFolders.Add(new CLFolder(childFolderPath));
            }

            return childCLFolders;
        }

        /// <summary>
        /// Gets the named child folder
        /// </summary>
        /// <returns>The child folder</returns>
        /// <param name="name">The name of the child folder to find</param>
        public async Task<IFolder> GetChildFolderAsync(string name)
        {
            IFolder childFolder = null;

            string childFolderPath = System.IO.Path.Combine(this.Path, name);

            bool folderExists = await this.Exists(childFolderPath);

            if (folderExists)
            {
                childFolder = new CLFolder(childFolderPath);
            }

            return childFolder;
        }

        /// <summary>
        /// Returns a list of files in the folder
        /// </summary>
        /// <returns>The files in the folder</returns>
        public async Task<List<IFile>> GetFilesAsync()
        {
            List<IFile> childFiles = new List<IFile>();

            string[] childCLFiles = await Task.FromResult<string[]>( Directory.GetFiles(this.Path) );

            foreach (string childCLFile in childCLFiles)
            {
                childFiles.Add(new CLFile(childCLFile));
            }

            return childFiles;
        }

        /// <summary>
        /// Create a new child folder
        /// </summary>
        /// <returns>The child folder to create</returns>
        /// <param name="name">The name for the folder to create</param>
        public async Task<IFolder> CreateChildFolder(string name)
        {
            string folderPath = System.IO.Path.Combine(this.Path, name);

            await Task.Run( () => Directory.CreateDirectory(folderPath) );

            return new CLFolder(folderPath);
        }

        /// <summary>
        /// Delete this folder
        /// </summary>
        /// <returns>An awaitable Task</returns>
        public async Task Delete()
        {
            await Task.Run(() => Directory.Delete(this.Path, true) );
        }

        /// <summary>
        /// Creates a text file in the folder with the provided name and text contents
        /// </summary>
        /// <returns>An awaitable Task</returns>
        /// <param name="name">The name of the file to create</param>
        /// <param name="contents">The text contents of the file to be created</param>
        public async Task<IFile> CreateFileAsync(string name, string contents)
        {
            string filePath = System.IO.Path.Combine(this.Path, name);
            
            await File.WriteAllTextAsync(filePath, contents);

            return new CLFile(filePath);
        }

        /// <summary>
        /// Copies the specified file to this folder
        /// </summary>
        /// <returns>An awaitable Task</returns>
        /// <param name="file">The file to copy to this folder</param>
        public async Task CopyFileHere(IFile file)
        {
            await Task.Run(() => File.Copy(file.Path, System.IO.Path.Combine(this.Path, file.Name)));
        }

        /// <summary>
        /// Checks to see if the folder exists
        /// </summary>
        /// <returns>Whether or not the folder exists</returns>
        /// <param name="path">The path of the folder to check</param>
        public async Task<bool> Exists(string path)
        {
            return await Task.FromResult<bool>(Directory.Exists(path));
        }
    }
}
