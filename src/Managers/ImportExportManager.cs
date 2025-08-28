using System;
using System.Threading.Tasks;
using AccountManager.Core.Interfaces;
using AccountManager.Models;
using AccountManager.Repositories;

namespace AccountManager.Managers
{
    /// <summary>
    /// Manages data import and export operations
    /// </summary>
    public class ImportExportManager
    {
        private readonly AppDataRepository _dataRepository;
        private readonly IDialogManager _dialogManager;
        private readonly INotificationService _notificationService;

        public ImportExportManager(
            AppDataRepository dataRepository, 
            IDialogManager dialogManager,
            INotificationService notificationService)
        {
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _dialogManager = dialogManager ?? throw new ArgumentNullException(nameof(dialogManager));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        /// <summary>
        /// Shows import dialog and imports data if successful
        /// </summary>
        public async Task ImportDataAsync()
        {
            try
            {
                var importPath = _dialogManager.ShowSelectFileDialog(
                    "Import Account Data",
                    "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    "json");

                if (!string.IsNullOrEmpty(importPath))
                {
                    var importedData = await _dataRepository.ImportAsync(importPath);
                    if (importedData != null)
                    {
                        // Replace current data with imported data
                        await _dataRepository.SaveAsync(importedData);
                        _notificationService.ShowInfo("Account data has been imported successfully.", "Import Successful");
                        return;
                    }
                }
                
                _notificationService.ShowError("Failed to import data. The file may be corrupted or in an invalid format.", "Import Failed");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"An error occurred while importing data: {ex.Message}", "Import Error");
            }
        }

        /// <summary>
        /// Shows export dialog and exports current data if successful
        /// </summary>
        public async Task ExportDataAsync()
        {
            try
            {
                var exportPath = _dialogManager.ShowSaveFileDialog(
                    "Export Account Data",
                    "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    $"accounts_export_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                    "json");

                if (!string.IsNullOrEmpty(exportPath))
                {
                    var success = await _dataRepository.ExportAsync(exportPath);
                    if (success)
                    {
                        _notificationService.ShowInfo($"Account data has been exported to:\n{exportPath}", "Export Successful");
                        return;
                    }
                }
                
                _notificationService.ShowError("Failed to export data. Check if the destination is writable.", "Export Failed");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"An error occurred while exporting data: {ex.Message}", "Export Error");
            }
        }

        /// <summary>
        /// Imports data from a specific file path
        /// </summary>
        public async Task<bool> ImportFromPathAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;

                var importedData = await _dataRepository.ImportAsync(filePath);
                if (importedData != null)
                {
                    await _dataRepository.SaveAsync(importedData);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing from {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Exports current data to a specific file path
        /// </summary>
        public async Task<bool> ExportToPathAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;

                return await _dataRepository.ExportAsync(filePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting to {filePath}: {ex.Message}");
                return false;
            }
        }
    }
}