using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using HTMLinjectorServices;

namespace HTMLinjectorCL
{
    public class CLFolder : IFolder
    {
        /// <summary>
        /// Gets the folder name excluding the path
        /// </summary>
        /// <value>The name.</value>
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
        public string Path { get; internal set; }

        public CLFolder(string path)
        {
            this.Path = path;
        }

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

        public async Task<IFolder> CreateChildFolder(string name)
        {
            string folderPath = System.IO.Path.Combine(this.Path, name);

            await Task.Run( () => Directory.CreateDirectory(folderPath) );

            return new CLFolder(folderPath);
        }

        public async Task Delete()
        {
            await Task.Run(() => Directory.Delete(this.Path, true) );
        }


        public async Task<IFile> CreateFileAsync(string name, string contents)
        {
            string filePath = System.IO.Path.Combine(this.Path, name);
            
            await File.WriteAllTextAsync(filePath, contents);

            return new CLFile(filePath);
        }

        public async Task CopyFileHere(IFile file)
        {
            await Task.Run(() => File.Copy(file.Path, System.IO.Path.Combine(this.Path, file.Name)));
        }

        public async Task<bool> Exists(string path)
        {
            return await Task.FromResult<bool>(Directory.Exists(path));
        }
    }
}
