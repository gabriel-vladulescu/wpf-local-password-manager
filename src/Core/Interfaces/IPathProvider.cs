using System.Threading.Tasks;

namespace AccountManager.Core.Interfaces
{
    /// <summary>
    /// Interface for providing file paths with different strategies
    /// </summary>
    public interface IPathProvider
    {
        string GetDefaultDataPath();
        string GetCurrentDataPath();
        string GetCustomDataPath();
        Task<bool> SetCustomDataPathAsync(string path);
        Task ResetToDefaultPathAsync();
        Task LoadCustomDataPathAsync();
        bool IsUsingDefaultPath();
        string GetDisplayPath();
        string GetDataDirectory();
    }
}