using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;

namespace GTA_SA_PathsRedactor.Services.Wrappers;

public interface IFileManipulationService
{
    string Filter { get; set; }

    /// <summary>
    /// Select existing file path for opening.
    /// </summary>
    /// <returns>File path if successfully opened; otherwise - <see cref="string.Empty"/></returns>
    string OpenFile();
    /// <summary>
    /// Select path of file for saving.
    /// </summary>
    /// <returns>File path if successfully opened; otherwise - <see cref="string.Empty"/></returns>
    string SaveFile();
}

public sealed partial class FileManipulationService : ObservableObject, IFileManipulationService
{
    [ObservableProperty]
    private string _filter = string.Empty;

    /// <inheritdoc />
    public string OpenFile()
    {
        var openDialog = new OpenFileDialog
        {
            Filter = Filter
        };

        return openDialog.ShowDialog() == true ? openDialog.FileName : string.Empty;
    }

    /// <inheritdoc />
    public string SaveFile()
    {
        var saveDialog = new SaveFileDialog()
        {
            Filter = Filter
        };

        return saveDialog.ShowDialog() == true ? saveDialog.FileName : string.Empty;
    }
}