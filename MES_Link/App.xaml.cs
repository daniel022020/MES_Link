using System;
using System.Windows;
using MES_Link.Log;

namespace MES_Link
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LogService.GetInstance().CreateLogConfig();
        }
    }
}