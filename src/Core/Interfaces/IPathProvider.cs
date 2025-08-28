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
        bool SetCustomDataPath(string path);
        void ResetToDefaultPath();
        bool IsUsingDefaultPath();
        string GetDisplayPath();
        string GetDataDirectory();
    }
}