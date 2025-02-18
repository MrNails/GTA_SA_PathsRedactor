using System;
using System.IO;
using GTA_SA_PathsRedactor.Services;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace GTA_SA_PathsRedactor.ViewModel
{
    public sealed partial class PointTransformViewModel : ObservableObject
    {
        private readonly NotificationService _notificationService;
        private readonly SettingsService _settingsService;
        
        private ICommand? _saveSetting;
        private ICommand? _loadSetting;
        
        [ObservableProperty]
        private PointTransformationData _pointTransformationData;

        public PointTransformViewModel(NotificationService notificationService, SettingsService settingsService
            )
        {
            _notificationService = notificationService;
            _settingsService = settingsService;
            _pointTransformationData = new PointTransformationData();
        }
      
        public ICommand SaveSettingsCommand => _saveSetting ??= new RelayCommand(SaveSettingCommandHandler);
        public ICommand LoadSettingsCommand => _loadSetting ??= new RelayCommand(LoadSettingCommandHandler);
        
        private void SaveSettingCommandHandler()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "JSON files (*.json) |*.json";

                // if (saveFileDialog.ShowDialog() == true)
                // {
                //     var transformSettingLoader = new TransformSettingSaverLoader(saveFileDialog.FileName, true);
                //
                //     transformSettingLoader.SaveSettings(CurrentPointTransformData);
                //
                //     MessageBox.Show("Settings saved succesfully", "Information",
                //                     MessageBoxButton.OK, MessageBoxImage.Asterisk);
                // }
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
        private void LoadSettingCommandHandler()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "JSON files (*.json) |*.json";

                // if (openFileDialog.ShowDialog() == true)
                // {
                //     var transformSettingLoader = new TransformSettingSaverLoader(openFileDialog.FileName);
                //
                //     var setting = transformSettingLoader.LoadSettings();
                //
                //     AddNewPointTransformationData(setting);
                //
                //     MessageBox.Show("Settings loaded succesfully", "Information",
                //                     MessageBoxButton.OK, MessageBoxImage.Asterisk);
                // }
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
