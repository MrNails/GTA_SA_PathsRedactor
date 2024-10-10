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

        private int m_currentPathIndex;
        private ObservableCollection<PathEditorViewModel> m_paths;

        private ICommand m_addPointCommand;
        private ICommand m_insertPointCommand;
        private ICommand m_removePointCommand;
        private ICommand m_removeSelectedPointsCommand;
        private ICommand m_clearPointsCommand;

        private ICommand m_savePath;
        private ICommand m_savePathAs;
        private ICommand m_loadPath;

        private ICommand m_createNewPath;
        private ICommand m_addNewPath;
        private ICommand m_removePath;

        private ICommand m_selectPath;

        public PathViewModel(DataToStorageService dataToStorageService, NotificationService notificationService,
            ILogger logger)
        {
            _dataToStorageService = dataToStorageService;
            _notificationService = notificationService;
            _logger = logger;

            m_paths = new ObservableCollection<PathEditorViewModel>();
            m_currentPathIndex = -1;

            m_addPointCommand = new RelayCommand<object>(obj =>
            {
                System.Diagnostics.Debug.WriteLine(obj);

                var dot = obj as Models.VisualObject ??
                          new Models.DotVisual(obj as Core.Models.WorldPoint);

                CurrentPath.AddPoint(dot);
            }, obj => obj != null && m_paths.Count != 0 &&
                      (obj is Models.VisualObject ||
                       obj is Core.Models.WorldPoint));
            m_removePointCommand = new RelayCommand<VisualObject>(
                obj => CurrentPath.RemovePoint(obj as Models.VisualObject),
                obj => obj is Models.VisualObject);
            m_removeSelectedPointsCommand = new RelayCommand(() => CurrentPath.RemoveSelectedPoints(),
                () => CurrentPath != null && CurrentPath.SelectedDots.Count != 0);

            m_clearPointsCommand = new RelayCommand(() =>
                {
                    CurrentPath.Clear();
                    MapCleared?.Invoke(this, CurrentPath);
                },
                () => CurrentPath != null && CurrentPath.PointCount != 0);

            m_loadPath = new AsyncRelayCommand<string>(obj => LoadPathHelper(obj as string));
            m_savePath = new AsyncRelayCommand(() => SavePathHelper(false), () => m_currentPathIndex != -1);
            m_savePathAs = new AsyncRelayCommand(() => SavePathHelper(true), () => m_currentPathIndex != -1);

            m_createNewPath = new RelayCommand<string>(obj =>
            {
                string pathName = obj as string ?? "New path " + (++s_pathCounter).ToString();
                var newPath = new PathEditorViewModel(pathName);

                AddNewPathHelper(newPath);
                OnPropertyChanged("Paths");
            });
            m_addNewPath = new RelayCommand<PathEditorViewModel>(
                obj => { AddNewPathHelper(obj as PathEditorViewModel); }, obj => obj is PathEditorViewModel);
            m_removePath = new RelayCommand<PathEditorViewModel>(obj =>
            {
                var pathEditor = obj as PathEditorViewModel;

                if (m_paths.Remove(pathEditor))
                {
                    PathRemoved?.Invoke(this, pathEditor);

                    CurrentPathIndex--;
                }
            }, obj => obj is PathEditorViewModel && m_paths.Count != 0);

            m_selectPath = new RelayCommand<object>(obj =>
            {
                var newIndex = -1;

                if (obj is int)
                {
                    newIndex = (int)obj;
                }
                else
                {
                    newIndex = m_paths.IndexOf((PathEditorViewModel)obj);
                }

                CurrentPathIndex = newIndex;
            }, obj => obj != null && (obj is int || obj is PathEditorViewModel));
        }

        public ObservableCollection<PathEditorViewModel> Paths => m_paths;

        public PathEditorViewModel? CurrentPath
        {
            get
            {
                if (m_currentPathIndex == -1)
                {
                    return null;
                }

                return m_paths[m_currentPathIndex];
            }
        }

        public ICommand AddPointCommand => m_addPointCommand;
        public ICommand InsertPointCommand => m_insertPointCommand;
        public ICommand RemovePointCommand => m_removePointCommand;
        public ICommand RemoveSelectedPointsCommand => m_removeSelectedPointsCommand;
        public ICommand ClearPointsCommand => m_clearPointsCommand;

        public ICommand SaveCurrentPath => m_savePath;
        public ICommand SaveCurrentPathAs => m_savePathAs;
        public ICommand LoadPath => m_loadPath;

        public ICommand AddNewPathCommand => m_addNewPath;
        public ICommand CreateNewPathCommand => m_createNewPath;
        public ICommand RemovePathCommand => m_removePath;

        public ICommand SelectPathCommand => m_selectPath;

        public int CurrentPathIndex
        {
            get => m_currentPathIndex;
            set
            {
                if (value < -1 || value >= m_paths.Count)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                PathSelected?.Invoke(this,
                    new PathSelectionArgs(value != -1 ? m_paths[value] : null, m_currentPathIndex, value));

                m_currentPathIndex = value;

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

                    newPath.AddRangePoint(points);
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
                var saveTask = pointSaver.SaveAsync(CurrentPath.Dots.Select(d => d.Point));

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
            m_paths.Add(pathEditorViewModel);

            PathAdded?.Invoke(this, pathEditorViewModel);

            CurrentPathIndex = m_paths.Count - 1;
        }

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}