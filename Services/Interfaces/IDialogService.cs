using System.Threading.Tasks;
using System.Windows.Controls;

namespace AccountManager.Services.Interfaces
{
    public interface IDialogService
    {
        void Initialize(System.Windows.Window mainWindow);
        Task<bool?> ShowDialogAsync(UserControl dialog);
        Task<bool?> ShowDialogWithErrorHandlingAsync(UserControl dialog, string errorTitle = "Dialog Error", string errorMessage = null);
        void CloseDialog();
    }
}