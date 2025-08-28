using System;
using System.IO;
using System.Threading.Tasks;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Exceptions;

namespace AccountManager.Infrastructure.Storage
{
    /// <summary>
    /// File system storage implementation
    /// </summary>
    public class FileStorage : IFileStorage
    {
        public async Task<string> ReadTextAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new StorageException($"File not found: {filePath}");

                return await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex) when (!(ex is StorageException))
            {
                throw new StorageException($"Error reading file {filePath}: {ex.Message}", ex);
            }
        }

        public async Task<bool> WriteTextAsync(string filePath, string content)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(filePath, content);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error writing file {filePath}: {ex.Message}");
                return false;
            }
        }

        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

        public FileInfo GetFileInfo(string filePath)
        {
            return Exists(filePath) ? new FileInfo(filePath) : null;
        }

        public bool CreateDirectory(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating directory {directoryPath}: {ex.Message}");
                return false;
            }
        }

        public bool IsWritable(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                    return false;

                var tempFile = Path.Combine(directoryPath, $"temp_{Guid.NewGuid()}.tmp");
                File.WriteAllText(tempFile, "test");
                File.Delete(tempFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidatePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            try
            {
                Path.GetFullPath(filePath);
                var directory = Path.GetDirectoryName(filePath);
                
                if (string.IsNullOrEmpty(directory))
                    return false;

                if (Directory.Exists(directory))
                {
                    return IsWritable(directory);
                }

                try
                {
                    var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempDir);
                    Directory.Delete(tempDir);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}