using System.Collections.Generic;
using System.Threading.Tasks;

namespace HTMLinjectorServices
{
    public interface IFolder
    {
        /// <summary>
        /// Gets the folder name not including path
        /// </summary>
        /// <value>The folder name</value>
        string Name { get; }

        /// <summary>
        /// Gets the folder name including path
        /// </summary>
        /// <value>The folder path</value>
        string Path { get; }

        Task<List<IFolder>> GetChildFoldersAsync();
        Task<IFolder> GetChildFolderAsync(string name);
        Task<List<IFile>> GetFilesAsync();
        Task<IFolder> CreateChildFolder(string name);
        Task Delete();
        Task<IFile> CreateFileAsync(string name, string contents);
        Task CopyFileHere(IFile file);
        Task<bool> Exists(string path);
    }
}