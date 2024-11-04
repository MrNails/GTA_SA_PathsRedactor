using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using GTA_SA_PathsRedactor.Services;
using GTA_SA_PathsRedactor.Models;
using Serilog;

namespace GTA_SA_PathsRedactor.ViewModel
{
    public class PathViewModel : INotifyPropertyChanged
    {
        private static int s_pathCounter = 0;

        private readonly DataToStorageService _dataToStorageService;
        private readonly NotificationService _notificationService;
        private readonly ILogger _logger;

        private readonly ObservableCollection<PathEditorViewModel> _paths;
        
        private int _currentPathIndex;

        private ICommand _addPointCommand;
        private ICommand _insertPointCommand;
        private ICommand _removePointCommand;
        private ICommand _removeSelectedPointsCommand;
        private ICommand _clearPointsCommand;

        private ICommand _savePath;
        private ICommand _savePathAs;
        private ICommand _loadPath;

        private ICommand _createNewPath;
        private ICommand _addNewPath;
        private ICommand _removePath;

        private ICommand _selectPath;

        public PathViewModel(DataToStorageService dataToStorageService, NotificationService notificationService,
            ILogger logger)
        {
            _dataToStorageService = dataToStorageService;
            _notificationService = notificationService;
            _logger = logger;

            _paths = new ObservableCollection<PathEditorViewModel>();
            _currentPathIndex = -1;

            _addPointCommand = new RelayCommand<object>(obj =>
            {
                System.Diagnostics.Debug.WriteLine(obj);

                var dot = obj as Models.VisualObject ??
                          new Models.DotVisual(obj as Core.Models.WorldPoint);

                // CurrentPath.AddPoint(dot);
            }, obj => obj != null && _paths.Count != 0 &&
                      (obj is Models.VisualObject ||
                       obj is Core.Models.WorldPoint));
            _removePointCommand = new RelayCommand<VisualObject>(
                obj => {},
                obj => obj is Models.VisualObject);
            _removeSelectedPointsCommand = new RelayCommand(() => { },
                () => CurrentPath != null);

            _clearPointsCommand = new RelayCommand(() =>
                {
                    CurrentPath.Clear();
                    MapCleared?.Invoke(this, CurrentPath);
                },
                () => CurrentPath != null );

            _loadPath = new AsyncRelayCommand<string>(obj => LoadPathHelper(obj as string));
            _savePath = new AsyncRelayCommand(() => SavePathHelper(false), () => _currentPathIndex != -1);
            _savePathAs = new AsyncRelayCommand(() => SavePathHelper(true), () => _currentPathIndex != -1);

            _createNewPath = new RelayCommand<string>(obj =>
            {
                string pathName = obj as string ?? "New path " + (++s_pathCounter).ToString();
                var newPath = new PathEditorViewModel(pathName);

                AddNewPathHelper(newPath);
                OnPropertyChanged("Paths");
            });
            _addNewPath = new RelayCommand<PathEditorViewModel>(
                obj => { AddNewPathHelper(obj as PathEditorViewModel); }, obj => obj is PathEditorViewModel);
            _removePath = new RelayCommand<PathEditorViewModel>(obj =>
            {
                var pathEditor = obj as PathEditorViewModel;

                if (_paths.Remove(pathEditor))
                {
                    PathRemoved?.Invoke(this, pathEditor);

                    CurrentPathIndex--;
                }
            }, obj => obj is PathEditorViewModel && _paths.Count != 0);

            _selectPath = new RelayCommand<object>(obj =>
            {
                var newIndex = -1;

                if (obj is int)
                {
                    newIndex = (int)obj;
                }
                else
                {
                    newIndex = _paths.IndexOf((PathEditorViewModel)obj);
                }

                CurrentPathIndex = newIndex;
            }, obj => obj != null && (obj is int || obj is PathEditorViewModel));
        }

        public ObservableCollection<PathEditorViewModel> Paths => _paths;

        public PathEditorViewModel? CurrentPath
        {
            get
            {
                if (_currentPathIndex == -1)
                {
                    return null;
                }

                return _paths[_currentPathIndex];
            }
        }

        public ICommand AddPointCommand => _addPointCommand;
        public ICommand InsertPointCommand => _insertPointCommand;
        public ICommand RemovePointCommand => _removePointCommand;
        public ICommand RemoveSelectedPointsCommand => _removeSelectedPointsCommand;
        public ICommand ClearPointsCommand => _clearPointsCommand;

        public ICommand SaveCurrentPath => _savePath;
        public ICommand SaveCurrentPathAs => _savePathAs;
        public ICommand LoadPath => _loadPath;

        public ICommand AddNewPathCommand => _addNewPath;
        public ICommand CreateNewPathCommand => _createNewPath;
        public ICommand RemovePathCommand => _removePath;

        public ICommand SelectPathCommand => _selectPath;

        public int CurrentPathIndex
        {
            get => _currentPathIndex;
            set
            {
                if (value < -1 || value >= _paths.Count)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                PathSelected?.Invoke(this,
                    new PathSelectionArgs(value != -1 ? _paths[value] : null, _currentPathIndex, value));

                _currentPathIndex = value;

                OnPropertyChanged();
                OnPropertyChanged("CurrentPath");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action<PathViewModel, PathEditorViewModel> PathAdded;
        public event Action<PathViewModel, PathEditorViewModel> PathRemoved;
        public event Action<PathViewModel, PathSelectionArgs> PathSelected;
        public event Action<PathViewModel, PathEditorViewModel> MapCleared;

        private async Task LoadPathHelper(string? path)
        {
            if (path == null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    path = openFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }

            var pointLoader = _dataToStorageService.CurrentPointLoader;

            var newPath = new PathEditorViewModel(path.Remove(0, path.LastIndexOf('\\') + 1));
            newPath.PathFileName = path;
            pointLoader.FileName = path;

            try
            {
                var loadPointTask = pointLoader.LoadAsync();

                await Task.WhenAny(loadPointTask, Task.Delay(30000));

                if (loadPointTask.IsCompleted)
                {
                    var points = loadPointTask.Result;

                    AddNewPathHelper(newPath);

                    
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
            catch (System.IO.FileNotFoundException ex)
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
                _notificationService.NotifyError("An error occurred while saving points.");
                _logger.Error(ex, "{ErrorMessage}", ex.Message);
            }
        }

        private async Task SavePathHelper(bool saveAs)
        {
            if (CurrentPath == null)
                return;

            string filePath = CurrentPath.PathFileName;

            if (filePath == string.Empty || saveAs)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "DATA files (*.dat)|*.dat|All files (*.*)|*.*";
                saveFileDialog.FileName = CurrentPath.PathName + ".dat";

                if (saveFileDialog.ShowDialog() == true)
                    filePath = saveFileDialog.FileName;
                else
                    return;
            }

            var pointSaver = _dataToStorageService.CurrentPointSaver;
            pointSaver.CreateBackup = true;
            pointSaver.FileName = filePath;

            try
            {
                var saveTask = pointSaver.SaveAsync(CurrentPath.Points);

                await Task.WhenAny(saveTask, Task.Delay(30000));

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

        private void AddNewPathHelper(PathEditorViewModel pathEditorViewModel)
        {
            _paths.Add(pathEditorViewModel);

            PathAdded?.Invoke(this, pathEditorViewModel);

            CurrentPathIndex = _paths.Count - 1;
        }

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}