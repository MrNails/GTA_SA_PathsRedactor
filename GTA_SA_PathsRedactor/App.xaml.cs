using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace GTA_SA_PathsRedactor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex m_mutex;

        public App()
        {
            bool createdNew;

            m_mutex = new Mutex(true, "Path editor", out createdNew);

            if (!createdNew)
            {
                this.Shutdown();
            }

            ConfigureLogger();

            DispatcherUnhandledException += App_DispatcherUnhandledException;

            this.Startup += App_Startup;
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

        private void LoadSettings()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "PointManipulationsDLLs.json");
            string text = string.Empty;

            try
            {
                if (!File.Exists(path))
                    return;
                else
                    text = File.ReadAllText(path);

                if (text == string.Empty)
                    return;

                var jObj = JObject.Parse(text);
                var gSettingJson = jObj["GlobalSettings"];
                var assembliesLocs = jObj["AssembliesLocs"].ToObject<List<string>>();
                var gSettings = GlobalSettings.GetInstance();

                gSettings.PTD = gSettingJson["PTD"].ToObject<Services.PointTransformationData>();
                gSettings.Resolution = gSettingJson["Resolution"].ToObject<Resolution>();

                foreach (var location in assembliesLocs)
                {
                    if (File.Exists(location))
                        Services.ProxyController.AddAssembly(location);
                }

                var saverAssembly = gSettingJson["CurrentSaver"]["AssemblyInfo"]?.ToObject<string>();
                var saverType = gSettingJson["CurrentSaver"]["TypeInfo"]?.ToObject<string>();
                var saver = gSettingJson["CurrentSaver"]["Saver"];
                var loaderAssembly = gSettingJson["CurrentLoader"]["AssemblyInfo"]?.ToObject<string>();
                var loaderType = gSettingJson["CurrentLoader"]["TypeInfo"]?.ToObject<string>();
                var loader = gSettingJson["CurrentLoader"]["Loader"];

                if (saverAssembly != null && saverType != null && saver != null &&
                    Services.ProxyController.ContainsAssembly(saverAssembly))
                    gSettings.CurrentSaver = (Core.PointSaver)saver.ToObject(Services.ProxyController.GetTypeByName(saverAssembly, saverType));
                else if (saverType == typeof(Services.DefaultPointSaver).FullName)
                    gSettings.CurrentSaver = saver.ToObject<Services.DefaultPointSaver>();

                if (loaderAssembly != null && loaderType != null && loader != null &&
                    Services.ProxyController.ContainsAssembly(loaderAssembly))
                    gSettings.CurrentLoader = (Core.PointLoader)loader.ToObject(Services.ProxyController.GetTypeByName(loaderAssembly, loaderType));
                else if (loaderType == typeof(Services.DefaultPointLoader).FullName)
                    gSettings.CurrentLoader = loader.ToObject<Services.DefaultPointLoader>();

            }
            catch (Exception ex)
            {
                LogErrorInfoAndShowMessageBox(ex);
            }
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            var globalSettings = GlobalSettings.GetInstance();

            var path = Path.Combine(Environment.CurrentDirectory, "PointManipulationsDLLs.json");
            var jsonSerializer = new JsonSerializer();

            FileStream fStream = null;

            try
            {
                if (!File.Exists(path))
                    fStream = File.Create(path);
                else
                    fStream = new FileStream(path, FileMode.Truncate, FileAccess.Write);

                using (var streamWriter = new StreamWriter(fStream))
                using (var jWriter = new JsonTextWriter(streamWriter))
                {
                    var gSettingsProp = new JProperty("GlobalSettings", JObject.FromObject(globalSettings));
                    var assembliesProp = new JProperty("AssembliesLocs", JArray.FromObject(Services.ProxyController.Assemblies.Select(assembly => assembly.Location)));

                    var jObj = new JObject();
                    jObj.Add(gSettingsProp);
                    jObj.Add(assembliesProp);

                    jObj.WriteTo(jWriter);

                    jWriter.Flush();
                    streamWriter.Flush();
                }
            }
            finally
            {
                fStream?.Close();
            }
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            LoadSettings();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG
            LogErrorInfoAndShowMessageBox(e.Exception.Message);
#else
            LogErrorInfoAndShowMessageBox("Occured unexpected error.", e.Exception);
#endif


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
