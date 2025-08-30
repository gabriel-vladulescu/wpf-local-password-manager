using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Exceptions;

namespace AccountManager.Infrastructure.Serialization
{
    /// <summary>
    /// JSON serialization implementation
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        private readonly IFileStorage _fileStorage;
        private readonly JsonSerializerOptions _readOptions;
        private readonly JsonSerializerOptions _writeOptions;

        public JsonSerializer(IFileStorage fileStorage)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            
            _readOptions = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            
            _writeOptions = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<T> DeserializeAsync<T>(string filePath) where T : class, new()
        {
            try
            {
                if (!_fileStorage.Exists(filePath))
                {
                    return new T();
                }

                var json = await _fileStorage.ReadTextAsync(filePath);
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new T();
                }

                var data = System.Text.Json.JsonSerializer.Deserialize<T>(json, _readOptions);
                return data ?? new T();
            }
            catch (JsonException ex)
            {
                throw new DataException($"Invalid JSON data in {filePath}: {ex.Message}", ex);
            }
            catch (Exception ex) when (!(ex is DataException))
            {
                throw new DataException($"Error loading data from {filePath}: {ex.Message}", ex);
            }
        }

        public async Task<bool> SerializeAsync<T>(T data, string filePath) where T : class
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(data, _writeOptions);
                return await _fileStorage.WriteTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public T Deserialize<T>(string content) where T : class, new()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                    return new T();

                return System.Text.Json.JsonSerializer.Deserialize<T>(content, _readOptions) ?? new T();
            }
            catch (JsonException ex)
            {
                throw new DataException($"Invalid JSON content: {ex.Message}", ex);
            }
        }

        public string Serialize<T>(T data) where T : class
        {
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(data, _writeOptions);
            }
            catch (Exception ex)
            {
                throw new DataException($"Error serializing data: {ex.Message}", ex);
            }
        }
    }
}