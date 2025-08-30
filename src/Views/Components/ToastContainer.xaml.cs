using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AccountManager.Views.Components
{
    public partial class ToastContainer : UserControl
    {
        public ObservableCollection<ToastNotification> Toasts { get; }

        public ToastContainer()
        {
            InitializeComponent();
            Toasts = new ObservableCollection<ToastNotification>();
            ToastsContainer.ItemsSource = Toasts;
        }

        public void ShowToast(string title, string message, ToastNotification.ToastType type, bool autoClose = true)
        {
            var toast = new ToastNotification();
            toast.SetContent(title, message, type, autoClose);
            
            toast.Closed += (s, e) =>
            {
                Toasts.Remove(toast);
            };
            
            // Add to the top of the list
            Toasts.Insert(0, toast);
            
            // Limit the number of visible toasts
            while (Toasts.Count > 5)
            {
                Toasts.RemoveAt(Toasts.Count - 1);
            }
        }
    }
}