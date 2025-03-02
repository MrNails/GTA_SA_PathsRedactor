using System;
using System.IO;
using GTA_SA_PathsRedactor.Services;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GTA_SA_PathsRedactor.Services.Wrappers;
using System.Threading.Tasks;

namespace GTA_SA_PathsRedactor.ViewModel
{
    public sealed partial class PointTransformViewModel : ObservableObject
    {
        private readonly INotificationService _notificationService;
        private readonly ISettingsService _settingsService;
        private readonly IFileManipulationService _fileManipulationService;

        private ICommand? _saveSetting;
        private ICommand? _loadSetting;

        [ObservableProperty]
        private PointTransformationData _pointTransformationData;

        public PointTransformViewModel(INotificationService notificationService, ISettingsService settingsService, IFileManipulationService fileManipulationService)
        {
            _notificationService = notificationService;
            _settingsService = settingsService;
            _fileManipulationService = fileManipulationService;
            _pointTransformationData = new PointTransformationData();
        }

        public ICommand SaveSettingsCommand => _saveSetting ??= new AsyncRelayCommand(SaveSettingCommandHandler);
        public ICommand LoadSettingsCommand => _loadSetting ??= new AsyncRelayCommand(LoadSettingCommandHandler);

        private async Task SaveSettingCommandHandler()
        {
            try
            {
                _fileManipulationService.Filter = "JSON files (*.json) |*.json";
                var fileName = _fileManipulationService.SaveFile();

                if (fileName == string.Empty)
                {
                    return;
                }

                await _settingsService.SaveSettings(PointTransformationData, fileName);

                _notificationService.NotifyInformation("Settings saved succesfully.");
            }
            catch (Exception ex)
            {
#if DEBUG
                _notificationService.NotifyError($"{ex.Message}\n{ex.StackTrace}");
#else
                _notificationService.NotifyError("An error ocured while settings is saving.");
#endif
            }
        }
        private async Task LoadSettingCommandHandler()
        {
            try
            {
                _fileManipulationService.Filter = "JSON files (*.json) |*.json";
                var fileName = _fileManipulationService.OpenFile();

                if (fileName == string.Empty)
                {
                    return;
                }

                var settings = await _settingsService.LoadSettings<PointTransformationData>(fileName);

                if (settings is null)
                {
                    settings = new PointTransformationData();
                    _notificationService.NotifyError($"Unable to load settings from file {fileName}.");
                }
                else
                {
                    _notificationService.NotifyInformation("Settings saved succesfully.");
                }

                PointTransformationData = settings;
            }
            catch (FileNotFoundException)
            {
                _notificationService.NotifyError("Specified file not found.");
            }
            catch (Exception ex)
            {
#if DEBUG
                _notificationService.NotifyError($"{ex.Message}\n{ex.StackTrace}");
#else
                _notificationService.NotifyError("An error ocured while settings is loading.");
#endif

            }
        }
    }
}
