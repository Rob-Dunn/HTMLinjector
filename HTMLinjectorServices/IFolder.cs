using System.Collections.Generic;
using System.Threading.Tasks;

namespace HTMLinjectorServices
{
    /// <summary>
    /// Platform independent representation of a Folder
    /// </summary>
    public interface IFolder
    {
        /// <summary>
        /// Gets the folder name excluding the path
        /// </summary>
        /// <remarks>If the folder path was "c:/folder/subfolder" the Name would be "subfolder"</remarks>
        /// <value>The folder name</value>
        string Name { get; }

        /// <summary>
        /// The full folder name including path
        /// </summary>
        /// <remarks>If the folder path was "c:/folder/subfolder" the Path would be "c:/folder/subfolder"</remarks>
        /// <value>The folder path</value>
        string Path { get; }

        /// <summary>
        /// Returns a list of child folders
        /// </summary>
        /// <returns>A list of child folders</returns>
        Task<List<IFolder>> GetChildFoldersAsync();

        /// <summary>
        /// Gets the named child folder
        /// </summary>
        /// <returns>The child folder</returns>
        /// <param name="name">The name of the child folder to find</param>
        Task<IFolder> GetChildFolderAsync(string name);

        /// <summary>
        /// Returns a list of files in the folder
        /// </summary>
        /// <returns>The files in the folder</returns>
        Task<List<IFile>> GetFilesAsync();

        /// <summary>
        /// Create a new child folder
        /// </summary>
        /// <returns>The child folder to create</returns>
        /// <param name="name">The name for the folder to create</param>
        Task<IFolder> CreateChildFolder(string name);

        /// <summary>
        /// Delete this folder
        /// </summary>
        /// <returns>An awaitable Task</returns>
        Task Delete();

        /// <summary>
        /// Creates a text file in the folder with the provided name and text contents
        /// </summary>
        /// <returns>An awaitable Task</returns>
        /// <param name="name">The name of the file to create</param>
        /// <param name="contents">The text contents of the file to be created</param>
        Task<IFile> CreateFileAsync(string name, string contents);

        /// <summary>
        /// Copies the specified file to this folder
        /// </summary>
        /// <returns>An awaitable Task</returns>
        /// <param name="file">The file to copy to this folder</param>
        Task CopyFileHere(IFile file);

        /// <summary>
        /// Checks to see if the folder exists
        /// </summary>
        /// <returns>Whether or not the folder exists</returns>
        /// <param name="path">The path of the folder to check</param>
        Task<bool> Exists(string path);
    }
}