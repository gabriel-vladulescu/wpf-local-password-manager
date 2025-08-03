using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AccountManager.Models;
using AccountManager.Services.Interfaces;
using AccountManager.Utilities.Constants;
using AccountManager.Utilities.Helpers;

namespace AccountManager.Services
{
    public class JsonService : IJsonService
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public JsonService()
        {
            _serializerOptions = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public void SaveData(AccountData data)
        {
            try
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data));

                data.UpdateLastModified();
                var json = JsonSerializer.Serialize(data, _serializerOptions);
                
                // Create backup before saving
                if (File.Exists(AppConstants.DataFileName))
                {
                    FileHelper.CreateBackup(AppConstants.DataFileName);
                }
                
                File.WriteAllText(AppConstants.DataFileName, json);
                
                System.Diagnostics.Debug.WriteLine($"Data saved successfully to {AppConstants.DataFileName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving data: {ex.Message}");
                throw;
            }
        }

        public async void SaveDataAsync(AccountData data)
        {
            await Task.Run(() => SaveData(data));
        }

        public AccountData LoadData()
        {
            try
            {
                if (!File.Exists(AppConstants.DataFileName))
                {
                    System.Diagnostics.Debug.WriteLine($"Data file {AppConstants.DataFileName} not found, creating new data structure");
                    return new AccountData();
                }

                var json = File.ReadAllText(AppConstants.DataFileName);
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    System.Diagnostics.Debug.WriteLine("Data file is empty, creating new data structure");
                    return new AccountData();
                }

                var data = JsonSerializer.Deserialize<AccountData>(json, _serializerOptions);
                
                System.Diagnostics.Debug.WriteLine($"Data loaded successfully from {AppConstants.DataFileName}");
                System.Diagnostics.Debug.WriteLine($"Loaded {data?.Groups?.Count ?? 0} groups");
                
                return data ?? new AccountData();
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON parsing error: {ex.Message}");
                throw new InvalidDataException($"Invalid JSON data in {AppConstants.DataFileName}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
                throw;
            }
        }

        public async Task<AccountData> LoadDataAsync()
        {
            return await Task.Run(() => LoadData());
        }

        public bool BackupData(AccountData data, string backupPath = null)
        {
            try
            {
                if (data == null)
                    return false;

                backupPath ??= $"{AppConstants.DataFileName}.{DateTime.Now:yyyyMMdd_HHmmss}.backup";
                
                var json = JsonSerializer.Serialize(data, _serializerOptions);
                File.WriteAllText(backupPath, json);
                
                System.Diagnostics.Debug.WriteLine($"Data backed up to {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error backing up data: {ex.Message}");
                return false;
            }
        }

        public AccountData RestoreData(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException($"Backup file not found: {backupPath}");
                }

                var json = File.ReadAllText(backupPath);
                var data = JsonSerializer.Deserialize<AccountData>(json, _serializerOptions);
                
                System.Diagnostics.Debug.WriteLine($"Data restored from {backupPath}");
                return data ?? new AccountData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring data: {ex.Message}");
                throw;
            }
        }
    }
}