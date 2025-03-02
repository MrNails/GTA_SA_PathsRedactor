using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GTA_SA_PathsRedactor.Models.EventArguments;
using GTA_SA_PathsRedactor.Services;
using GTA_SA_PathsRedactor.Services.Helpers;
using GTA_SA_PathsRedactor.Services.Interfaces;
using GTA_SA_PathsRedactor.Services.Wrappers;
using GTA_SA_PathsRedactor.View.Windows;
using Microsoft.Win32;
using Serilog;

namespace GTA_SA_PathsRedactor.ViewModel
{
    public sealed class MainViewModel : ObservableObject
    {
        /// <summary>
        /// Time in milliseconds
        /// </summary>
        private const int MaxWaitingTimeForSavingLoadingPaths_ = 30000;

        private static int _pathCounter;

        private readonly ILogger _logger;
        private readonly IDataToStorageService _dataToStorageService;
        private readonly INotificationService _notificationService;
        private readonly INonDialogWindowHelper _nonDialogWindowHelper;
        private readonly IFileManipulationService _fileManipulatorService;

        private int _currentPathIndex;

        private ICommand? _savePathCommand;
        private ICommand? _savePathAsCommand;
        private ICommand? _loadPathCommand;

        private ICommand? _createNewPathCommand;
        private ICommand? _addNewPathCommand;
        private ICommand? _removePathCommand;

        private ICommand? _selectPathCommand;
        private ICommand? _clearSelectedPathPointsCommand;

        private ICommand? _openHelpWindowCommand;
        private ICommand? _openAboutWindowCommand;

#if DEBUG
        public MainViewModel() { }
#endif

        public MainViewModel(ILogger logger,
                             IDataToStorageService dataToStorageService,
                             INotificationService notificationService,
                             INonDialogWindowHelper nonDialogWindowHelper,
                             IFileManipulationService fileManipulationService)
        {
            _logger = logger;
            _dataToStorageService = dataToStorageService;
            _notificationService = notificationService;
            _nonDialogWindowHelper = nonDialogWindowHelper;
            _fileManipulatorService = fileManipulationService;

            Paths = new ObservableCollection<PathEditorViewModel>();
            _currentPathIndex = -1;
        }

        public ObservableCollection<PathEditorViewModel> Paths { get; }

        public PathEditorViewModel? CurrentPath => _currentPathIndex == -1 ? null : Paths[_currentPathIndex];

        public ICommand SaveCurrentPathCommand => _savePathCommand ??=
            new AsyncRelayCommand(() => SavePathExecute(false), () => _currentPathIndex != -1);
        public ICommand SaveCurrentPathAsCommand => _savePathAsCommand ??=
            new AsyncRelayCommand(() => SavePathExecute(true), () => _currentPathIndex != -1);
        public ICommand LoadPathCommand => _loadPathCommand ??=
            new AsyncRelayCommand(LoadPathExecute);

        public ICommand AddNewPathCommand => _addNewPathCommand ??=
            new RelayCommand<PathEditorViewModel>(AddNewPathExecute, pathEditor => pathEditor is not null);
        public ICommand CreateNewPathCommand => _createNewPathCommand ??=
            new RelayCommand<string>(pathName =>
            {
                pathName ??= $"New path {++_pathCounter}";
                var newPath = new PathEditorViewModel(pathName);

                AddNewPathExecute(newPath);
            });
        public ICommand RemovePathCommand => _removePathCommand ??=
            new RelayCommand<PathEditorViewModel>(pathEditorModel =>
            {
                if (!Paths.Remove(pathEditorModel!))
                    return;

                PathRemoved?.Invoke(this, pathEditorModel!);
            }, pathEditorModel => pathEditorModel is not null && Paths.Count != 0);

        public ICommand SelectPathCommand => _selectPathCommand ??=
            new RelayCommand<object>(obj =>
            {
                var newIndex = -1;

                if (obj is int index)
                {
                    newIndex = index;
                }
                else if (obj is PathEditorViewModel pathEditorModel)
                {
                    newIndex = Paths.IndexOf(pathEditorModel);
                }

                CurrentPathIndex = newIndex;
            }, obj => obj is int or PathEditorViewModel);
        public ICommand ClearSelectedPathPointsCommand => _clearSelectedPathPointsCommand
            ??= new RelayCommand<PathEditorViewModel>(path => path?.Clear(), path => path is not null);

        public ICommand OpenHelpWindowCommand => _openHelpWindowCommand ??= new RelayCommand(_nonDialogWindowHelper.Show<HelpWindow>);
        public ICommand OpenAboutWindowCommand => _openAboutWindowCommand ??= new RelayCommand(_nonDialogWindowHelper.Show<AboutWindow>);

        public int CurrentPathIndex
        {
            get => _currentPathIndex;
            set
            {
                if (value < -1 || value >= Paths.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                PathSelected?.Invoke(this,
                    new PathSelectionArgs(value != -1 ? Paths[value] : null, _currentPathIndex, value));

                _currentPathIndex = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPath));
            }
        }

        public event Action<MainViewModel, PathEditorViewModel>? PathAdded;
        public event Action<MainViewModel, PathEditorViewModel>? PathRemoved;
        public event Action<MainViewModel, PathSelectionArgs>? PathSelected;

        private async Task LoadPathExecute()
        {
            _fileManipulatorService.Filter = "All files (*.*)|*.*";
            var path = _fileManipulatorService.OpenFile();

            if (path == string.Empty)
                return;

            var pointLoader = _dataToStorageService.CurrentPointLoader;
            pointLoader.FileName = path;
            try
            {
                var loadPointTask = pointLoader.LoadAsync();

                await Task.WhenAny(loadPointTask, Task.Delay(MaxWaitingTimeForSavingLoadingPaths_));

                if (loadPointTask.IsCompleted)
                {
                    var pathEditor = new PathEditorViewModel(Path.GetFileNameWithoutExtension(path), loadPointTask.Result);
                    pathEditor.PathFileName = path;

                    AddNewPathExecute(pathEditor);
                }
                else
                {
                    _notificationService.NotifyWarning(
                        "Cannot parse points due to timeout. If this will happen again, remove this custom saver and contact to creator.");

                    _logger.Warning(
                        "Points parsing finished by time out. Saver: {SaverName}; Destination: {DestinationPath}",
                        pointLoader.GetType().FullName, pointLoader.FileName);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _notificationService.NotifyError("You don't have permissions access to selected file.");
                _logger.Error(ex, "{ErrorMessage}", ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                _notificationService.NotifyError($"File by path {pointLoader.FileName} does not exist.");
                _logger.Error(ex, "{ErrorMessage}", ex.Message);
            }
            catch (Core.PointsLoadingException ex)
            {
                _notificationService.NotifyError($"Cannot parse file {ex.PointsFileName}\n\nAn error occured on line {ex.FileErrorLine}.");
                _logger.Error(ex, "{ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                _notificationService.NotifyError("An error occurred while loading points.");
                _logger.Error(ex, "{ErrorMessage}", ex.Message);
            }
        }

        private async Task SavePathExecute(bool saveAs)
        {
            if (CurrentPath == null)
                return;

            var filePath = CurrentPath.PathFileName;

            if (filePath == string.Empty || saveAs)
            {
                _fileManipulatorService.Filter = "DATA files (*.dat)|*.dat|All files (*.*)|*.*";
                _fileManipulatorService.FileName = CurrentPath.PathName + ".dat";
                filePath = _fileManipulatorService.OpenFile();

                if (filePath == string.Empty)
                    return;
            }

            var pointSaver = _dataToStorageService.CurrentPointSaver;
            pointSaver.CreateBackup = true;
            pointSaver.FileName = filePath;

            try
            {
                var saveTask = pointSaver.SaveAsync(CurrentPath.Points);

                await Task.WhenAny(saveTask, Task.Delay(MaxWaitingTimeForSavingLoadingPaths_));

                if (!saveTask.IsCompleted)
                {
                    _notificationService.NotifyWarning(
                        "Cannot save points due to timeout. If this will happen again, remove this custom saver and contact to creator.");

                    _logger.Warning(
                        "Points saving finished by time out. Saver: {SaverName}; Destination: {DestinationPath}",
                        pointSaver.GetType().FullName, pointSaver.FileName);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _notificationService.NotifyError("You don't have permissions access to selected file.");
                _logger.Error(ex, "{ErrorMessage}", ex.Message);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                _notificationService.NotifyError($"File by path {pointSaver.FileName} does not exist.");
                _logger.Error(ex, "{ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                _notificationService.NotifyError("An error occurred while saving points.");
                _logger.Error(ex, "{ErrorMessage}", ex.Message);
            }
        }

        private void AddNewPathExecute(PathEditorViewModel? pathEditorViewModel)
        {
            Paths.Add(pathEditorViewModel!);

            PathAdded?.Invoke(this, pathEditorViewModel!);

            CurrentPathIndex = Paths.Count - 1;
        }
    }
}