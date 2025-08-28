using System.Threading.Tasks;
using System.Windows.Controls;

namespace AccountManager.Core.Interfaces
{
    /// <summary>
    /// Interface for managing application dialogs
    /// </summary>
    public interface IDialogManager
    {
        Task<bool?> ShowDialogAsync(UserControl dialog);
        void CloseDialog();
        string ShowSelectFileDialog(string title, string filter, string defaultExt = null);
        string ShowSaveFileDialog(string title, string filter, string defaultFileName = null, string defaultExt = null);
    }
}