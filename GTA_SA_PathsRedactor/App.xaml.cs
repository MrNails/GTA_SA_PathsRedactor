using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        private bool m_createdNew;

        private object m_lock = new object();

        public App()
        {
            m_mutex = new Mutex(true, "Path editor", out m_createdNew);

            if (!m_createdNew)
            {
                BringExistAppWindowToFront();
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

        private void BringExistAppWindowToFront()
        {
            var currentProc = System.Diagnostics.Process.GetCurrentProcess();
            var foundProc = System.Diagnostics.Process.GetProcessesByName(currentProc.ProcessName).Where(existProc => currentProc.Id != existProc.Id)
                                                                                                  .FirstOrDefault();

            if (foundProc != null)
            {
                ShowWindow(foundProc.MainWindowHandle, 9);
                SetWindowPos(foundProc.MainWindowHandle, new IntPtr(-1), 
                             -1, -1, -1, -1,
                             (int)(SWPFlags.NoSize | SWPFlags.NoMove));
            }
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            if (m_createdNew)
            {
                lock (m_lock)
                {
                    m_mutex.ReleaseMutex();
                }
            }

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

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        [Flags]
        private enum SWPFlags
        {
            /// <summary>
            /// Retains the current size (ignores the cx and cy parameters).
            /// </summary>
            NoSize = 0x0001,

            /// <summary>
            /// Retains the current position (ignores X and Y parameters).
            /// </summary>
            NoMove = 0x0002,

            /// <summary>
            /// Retains the current Z order (ignores the hWndInsertAfter parameter).
            /// </summary>
            NoZOrder = 0x0004,

            /// <summary>
            /// Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
            /// </summary>
            NoRedraw = 0x0008,

            /// <summary>
            /// Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
            /// </summary>
            NoActive = 0x0010,

            /// <summary>
            /// Draws a frame (defined in the window's class description) around the window.
            /// </summary>
            DrawFrame = 0x0020,

            /// <summary>
            /// Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
            /// </summary>
            FrameChanged = 0x0020,

            /// <summary>
            /// Displays the window.
            /// </summary>
            ShowWindow = 0x0040,

            /// <summary>
            /// Hides the window.
            /// </summary>
            HideWindow = 0x0080,

            /// <summary>
            /// Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
            /// </summary>
            NoCopyBits = 0x0100,

            /// <summary>
            /// Does not change the owner window's position in the Z order.
            /// </summary>
            NoOwnerZOrder = 0x0200,

            /// <summary>
            /// Same as the <see cref="SWPFlags.NoOwnerZOrder"/> flag.
            /// </summary>
            NoReposition = 0x0200,

            /// <summary>
            /// Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
            /// </summary>
            NoSendChanging = 0x0400,

            /// <summary>
            /// Prevents generation of the WM_SYNCPAINT message.
            /// </summary>
            DeferErase = 0x2000,

            /// <summary>
            /// If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request..
            /// </summary>
            AsyncWindowPos = 0x4000,
        }
    }
}
