using GTA_SA_PathsRedactor.Services.Interfaces;
using System.Linq;
using System.Windows;

namespace GTA_SA_PathsRedactor.Services.Helpers
{
    public sealed class NonDialogWindowHelper : INonDialogWindowHelper
    {
        public void Show<TWindow>() where TWindow : Window, new()
        {
            var mainWindow = App.Current.MainWindow;
            var window = mainWindow.OwnedWindows.OfType<Window>()
                                                .FirstOrDefault(wnd => wnd is TWindow);

            if (window is not null)
            {
                window.Activate();
                return;
            }

            window = new TWindow();
            window.Owner = mainWindow;
            window.Show();
        }
    }
}
