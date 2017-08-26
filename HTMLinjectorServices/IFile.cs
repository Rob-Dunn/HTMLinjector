using System.Threading.Tasks;

namespace HTMLinjectorServices
{
    public interface IFile
    {
        /// <summary>
        /// Gets the file name not including path
        /// </summary>
        /// <value>The file name</value>
        string Name { get; }

        /// <summary>
        /// Gets the file name including path
        /// </summary>
        /// <value>The file path</value>
        string Path { get; }

        Task<string> ReadAllTextAsync();
    }
}