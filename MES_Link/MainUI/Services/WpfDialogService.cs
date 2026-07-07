using MES_Link.Interfaces;

namespace MES_Link.MainUI.Services
{
    public class WpfDialogService : IDialogService
    {
        public void ShowError(string message, string title)
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            });
        }

        public bool Confirm(string message, string title)
        {
            var result = System.Windows.MessageBox.Show(
                message, title, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

            return result == System.Windows.MessageBoxResult.Yes;
        }
    }
}