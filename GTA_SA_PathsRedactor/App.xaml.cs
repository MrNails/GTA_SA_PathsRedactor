using System;
using System.Threading;
using System.Windows;

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

            mutex = new Mutex(true, "Path redactor", out createdNew);

            if (!createdNew)
            {
                this.Shutdown();
            }
        }
    }
}
