using System;
using System.Threading;
using System.Windows;
using Serilog;

namespace GTA_SA_PathsRedactor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex mutex;

        public App()
        {
            bool createdNew;

            mutex = new Mutex(true, "Path editor", out createdNew);

            if (!createdNew)
            {
                this.Shutdown();
            }

            ConfigureLogger();

            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void ConfigureLogger()
        {
            var logConfiguration = new LoggerConfiguration();

            Log.Logger = logConfiguration.WriteTo.File(System.IO.Path.Combine(Environment.CurrentDirectory, "log.txt"),
                                                       outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}",
                                                       restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                                         .CreateLogger();

            this.Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            var gSettings = GlobalSettings.GetInstance();

            gSettings.CurrentLoader.Dispose();
            gSettings.CurrentSaver.Dispose();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogErrorInfoAndShowMessageBox("Occured unexpected error.", e.Exception);

            e.Handled = true;
        }

        public static void LogErrorInfoAndShowMessageBox(string userMessage, string logMessage, Exception? ex = null)
        {
            MessageBox.Show(userMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            if (ex == null)
                Log.Error(logMessage);
            else
                Log.Error(ex, logMessage);
        }
        public static void LogErrorInfoAndShowMessageBox(string userMessage, Exception? ex = null)
        {
            MessageBox.Show(userMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            if (ex == null)
                Log.Error(userMessage);
            else
                Log.Error(ex, userMessage);
        }
        public static void LogErrorInfoAndShowMessageBox(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Log.Error(ex, "{Message}");
        }
    }
}
