using System.Threading.Tasks;

namespace HTMLinjectorServices
{
    /// <summary>
    /// Platform independent representation of a File
    /// </summary>
    public interface IFile
    {
        /// <summary>
        /// Gets the file name not including path
        /// </summary>
        /// <value>The file name</value>
        string Name { get; }

        /// <summary>
        /// The full file name including path
        /// </summary>
        /// <value>The file path</value>
        string Path { get; }

        /// <summary>
        /// Returns the contents of the file
        /// </summary>
        /// <returns>The text contained in the file</returns>
        Task<string> ReadAllTextAsync();
    }
}