using System.IO;
using System.Threading.Tasks;

namespace AccountManager.Core.Interfaces
{
    /// <summary>
    /// Interface for file system operations
    /// </summary>
    public interface IFileStorage
    {
        Task<string> ReadTextAsync(string filePath);
        Task<bool> WriteTextAsync(string filePath, string content);
        bool Exists(string filePath);
        FileInfo GetFileInfo(string filePath);
        bool CreateDirectory(string directoryPath);
        bool IsWritable(string directoryPath);
        bool ValidatePath(string filePath);
    }
}