using System;
using System.IO;
using System.Threading.Tasks;

namespace AccountManager.Utilities.Helpers
{
    public static class FileHelper
    {
        /// <summary>
        /// Safely reads all text from a file
        /// </summary>
        public static async Task<string> SafeReadAllTextAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                return await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading file {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Safely writes text to a file
        /// </summary>
        public static async Task<bool> SafeWriteAllTextAsync(string filePath, string content)
        {
            try
            {
                // Create directory if it doesn't exist
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

        /// <summary>
        /// Creates a backup of a file
        /// </summary>
        public static bool CreateBackup(string filePath, string backupSuffix = ".backup")
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                var backupPath = filePath + backupSuffix;
                File.Copy(filePath, backupPath, true);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating backup for {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the size of a file in bytes
        /// </summary>
        public static long GetFileSize(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return 0;

                var fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Checks if a file is locked by another process
        /// </summary>
        public static bool IsFileLocked(string filePath)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}