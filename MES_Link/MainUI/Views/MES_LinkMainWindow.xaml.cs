using MES_Link.MainUI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MES_Link.MainUI.Views
{
    /// <summary>
    /// MES_LinkMainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MES_LinkViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void LogView_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 當文字改變時，自動滾動到最底（如 Tail 效果）
            if (sender is TextBox textBox)
            {
                textBox.ScrollToEnd();
            }
        }
    }
}
