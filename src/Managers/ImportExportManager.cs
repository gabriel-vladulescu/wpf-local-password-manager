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
                    await ImportDataWithLoadingAsync(importPath);
                }
                // If importPath is empty, user cancelled the dialog - don't show error
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"An error occurred while importing data: {ex.Message}", "Import Error");
            }
        }

        /// <summary>
        /// Import data with smart loading overlay based on file size
        /// </summary>
        public async Task ImportDataWithLoadingAsync(string filePath)
        {
            try
            {
                var fileInfo = new System.IO.FileInfo(filePath);
                var (loadingDuration, loadingMessage, isLargeFile) = CalculateLoadingParameters(fileInfo);
                
                if (isLargeFile)
                {
                    await ImportLargeFileAsync(filePath, loadingMessage);
                }
                else
                {
                    await ImportSmallFileAsync(filePath, loadingMessage, loadingDuration);
                }
            }
            catch (Exception ex)
            {
                _dialogManager.HideLoadingOverlay();
                _notificationService.ShowError($"An error occurred while importing data: {ex.Message}", "Import Error");
            }
        }

        /// <summary>
        /// Calculate loading parameters based on file size
        /// </summary>
        private (TimeSpan duration, string message, bool isLargeFile) CalculateLoadingParameters(System.IO.FileInfo fileInfo)
        {
            const double SmallFileSizeKB = 100.0;
            const double MediumFileSizeKB = 1024.0;
            const int SmallFileLoadingSeconds = 5;
            const int MediumFileLoadingSeconds = 10;
            
            var fileSizeKB = fileInfo.Length / 1024.0;
            
            if (fileSizeKB <= SmallFileSizeKB)
            {
                return (TimeSpan.FromSeconds(SmallFileLoadingSeconds), $"Importing data ({fileSizeKB:F1} KB)...", false);
            }
            else if (fileSizeKB <= MediumFileSizeKB)
            {
                return (TimeSpan.FromSeconds(MediumFileLoadingSeconds), $"Importing data ({fileSizeKB:F1} KB)...", false);
            }
            else
            {
                var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);
                return (TimeSpan.Zero, $"Importing large file ({fileSizeMB:F1} MB)...", true);
            }
        }

        /// <summary>
        /// Import large files with operation-based loading
        /// </summary>
        private async Task ImportLargeFileAsync(string filePath, string loadingMessage)
        {
            await _dialogManager.ShowLoadingOverlayAsync(loadingMessage, async () =>
            {
                await ExecuteImportAsync(filePath);
            });
            
            _notificationService.ShowSuccess("Large data file has been imported successfully.", "Import Successful");
        }

        /// <summary>
        /// Import small files with time-based loading
        /// </summary>
        private async Task ImportSmallFileAsync(string filePath, string loadingMessage, TimeSpan loadingDuration)
        {
            var loadingTask = _dialogManager.ShowLoadingOverlayAsync(loadingMessage, loadingDuration);
            var importTask = ExecuteImportAsync(filePath);
            
            await Task.WhenAll(loadingTask, importTask);
            
            _notificationService.ShowSuccess("Account data has been imported successfully.", "Import Successful");
        }

        /// <summary>
        /// Execute the actual import operation
        /// </summary>
        private async Task ExecuteImportAsync(string filePath)
        {
            var importedData = await _dataRepository.ImportAsync(filePath);
            if (importedData == null)
            {
                throw new InvalidOperationException("Failed to import data. The file may be corrupted or in an invalid format.");
            }
            
            await _dataRepository.SaveAsync(importedData);
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
                    else
                    {
                        _notificationService.ShowError("Failed to export data. Check if the destination is writable.", "Export Failed");
                    }
                }
                // If exportPath is empty, user cancelled the dialog - don't show error
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
                return false;
            }
        }
    }
}