using System.Windows;

namespace GTA_SA_PathsRedactor.Services.Interfaces
{
    public interface INonDialogWindowHelper
    {
        void Show<TWindow>() where TWindow : Window, new();
    }
}
