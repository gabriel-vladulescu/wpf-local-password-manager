namespace AccountManager.Core.Interfaces
{
    /// <summary>
    /// Interface for user notifications and messages
    /// </summary>
    public interface INotificationService
    {
        void ShowInfo(string message, string title = "Information");
        void ShowError(string message, string title = "Error");
        void ShowWarning(string message, string title = "Warning");
        void ShowSuccess(string message, string title = "Success");
        bool ShowConfirmation(string message, string title = "Confirm");
    }
}