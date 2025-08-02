using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AccountManager.Models;

namespace AccountManager.Services
{
    public class JsonService
    {
        private const string FileName = "accounts.json";
        private readonly JsonSerializerOptions _options;

        public JsonService()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public AccountData LoadData()
        {
            try
            {
                if (!File.Exists(FileName))
                {
                    var defaultData = CreateDefaultData();
                    SaveData(defaultData);
                    return defaultData;
                }

                var json = File.ReadAllText(FileName);
                var data = JsonSerializer.Deserialize<AccountData>(json, _options);
                return data ?? CreateDefaultData();
            }
            catch (Exception ex)
            {
                // Log error or show message to user
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
                return CreateDefaultData();
            }
        }

        public void SaveData(AccountData data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _options);
                File.WriteAllText(FileName, json);
                System.Diagnostics.Debug.WriteLine($"Successfully saved data to {FileName}");
                System.Diagnostics.Debug.WriteLine($"Data: {json}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving data: {ex.Message}");
                throw new Exception($"Failed to save accounts: {ex.Message}", ex);
            }
        }

        private AccountData CreateDefaultData()
        {
            return new AccountData
            {
                Groups = new List<AccountGroup>() // Start with empty list - no default groups
            };
        }
    }
}