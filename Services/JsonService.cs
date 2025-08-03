using System;
using System.IO;
using System.Text.Json;
using AccountManager.Models;

namespace AccountManager.Services
{
    public class JsonService
    {
        private const string DataFileName = "accounts.json";

        public void SaveData(AccountData data)
        {
            try
            {
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(DataFileName, json);
                
                System.Diagnostics.Debug.WriteLine($"Data saved successfully to {DataFileName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving data: {ex.Message}");
                throw;
            }
        }

        public AccountData LoadData()
        {
            try
            {
                if (!File.Exists(DataFileName))
                {
                    System.Diagnostics.Debug.WriteLine($"Data file {DataFileName} not found, creating new data structure");
                    return new AccountData();
                }

                var json = File.ReadAllText(DataFileName);
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    System.Diagnostics.Debug.WriteLine("Data file is empty, creating new data structure");
                    return new AccountData();
                }

                var options = new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
                
                var data = JsonSerializer.Deserialize<AccountData>(json, options);
                
                System.Diagnostics.Debug.WriteLine($"Data loaded successfully from {DataFileName}");
                System.Diagnostics.Debug.WriteLine($"Loaded {data?.Groups?.Count ?? 0} groups");
                
                return data ?? new AccountData();
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON parsing error: {ex.Message}");
                throw new InvalidDataException($"Invalid JSON data in {DataFileName}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
                throw;
            }
        }
    }
}