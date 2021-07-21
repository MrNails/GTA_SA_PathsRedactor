using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using GTA_SA_PathsRedactor.Services;

namespace GTA_SA_PathsRedactor.View
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        private static AboutWindow? s_existWindow;

        public AboutWindow()
        {
            InitializeComponent();

            if (s_existWindow == null)
                s_existWindow = this;

            LoadAssemblyInfo();
        }

        private void LoadAssemblyInfo()
        {
            var assemblyInfo = GetType().Assembly.GetAssemblyInfo();
            var gitHubInfoIndex = assemblyInfo.Description.LastIndexOf("GitHub");

            VersionTextBlock.Text = assemblyInfo.Version;

            if (gitHubInfoIndex != -1)
            {
                var description = assemblyInfo.Description.Remove(gitHubInfoIndex);
                var gitHub = assemblyInfo.Description.Remove(0, gitHubInfoIndex - 1);

                DescriptionTextBlock.Inlines.Add(description);

                var gitHubRun = new Run("GitHub");
                gitHubRun.Style = (Style)Resources["UnderlineText"];
                gitHubRun.MouseUp += (s, arg) =>
                {
                    var tempLink = gitHub.Remove(0, 6).Trim().Remove(0, 3);
                    tempLink = tempLink.Remove(tempLink.LastIndexOf(')'));

                    OpenLinkInBrowser(tempLink);
                };

                DescriptionTextBlock.Inlines.Add(gitHubRun);
            }
            else
                DescriptionTextBlock.Text = assemblyInfo.Description;
        }

        public static AboutWindow? ExistWindow => s_existWindow;

        private void OpenLinkInBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}") { CreateNoWindow = true });
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Process.Start("xdg-open", url);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Process.Start("open", url);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (s_existWindow == this)
                s_existWindow = null;
        }

        private void GitHubLink_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenLinkInBrowser(GitHubLink.Text);
        }
    }
}
