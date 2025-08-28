using System.Threading.Tasks;

namespace AccountManager.Core.Interfaces
{
    /// <summary>
    /// Interface for serialization strategies (JSON, XML, etc.)
    /// </summary>
    public interface ISerializer
    {
        Task<T> DeserializeAsync<T>(string filePath) where T : class, new();
        Task<bool> SerializeAsync<T>(T data, string filePath) where T : class;
        T Deserialize<T>(string content) where T : class, new();
        string Serialize<T>(T data) where T : class;
    }
}