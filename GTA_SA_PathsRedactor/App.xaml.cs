using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using GTA_SA_PathsRedactor.IoC;
using GTA_SA_PathsRedactor.Models;
using GTA_SA_PathsRedactor.Services;
using GTA_SA_PathsRedactor.Services.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GTA_SA_PathsRedactor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Mutex _mutex;
        private readonly bool _createdNew;

        public App()
        {
            _mutex = new Mutex(true, "GTA SA Path editor", out _createdNew);

            if (!_createdNew)
            {
                User32Wrapper.BringExistAppWindowToFront();
                Shutdown();
            }

            ServiceProvider = IoCConfig.ConfigureIoC();

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            Startup += App_Startup;
            Exit += App_Exit;
        }

        public IServiceProvider ServiceProvider { get; }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            if (_createdNew)
            {
                _mutex.ReleaseMutex();
            }
            
            var path = Path.Combine(Environment.CurrentDirectory, "PointManipulationsDLLs.json");
            var projectData = new ProjectDataService(ServiceProvider);
            
            File.WriteAllText(path, projectData.ExtractInfo());
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            
            var path = Path.Combine(Environment.CurrentDirectory, "PointManipulationsDLLs.json");
            var projectData = new ProjectDataService(ServiceProvider);
            var json = string.Empty;
            
            if (File.Exists(path))
                json = File.ReadAllText(path);
            
            projectData.RestoreInfo(json);
        }

        private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Occured unexpected error.");

            e.Handled = true;
        }
    }
}