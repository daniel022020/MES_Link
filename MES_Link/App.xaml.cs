using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MES_Link.Interfaces.MES_Link.Interfaces;
using MES_Link.MesSimulator;
using MES_Link.MainUI.ViewModels;
using MES_Link.MainUI.Views;
using MES_Link.Log;
using MES_Link.Interfaces;
using MES_Link.MainUI.Services;

namespace MES_Link
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // NLog初始化
            LogService.GetInstance().CreateLogConfig();

            var services = new ServiceCollection();

            services.AddSingleton<ILoggerService, NLogLoggerService>();
            services.AddSingleton<IDialogService, WpfDialogService>();
            services.AddSingleton<IMesSimulatorService, MesSimulatorService>();
            services.AddSingleton<MES_LinkViewModel>();
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
    }
}