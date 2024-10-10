using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Serilog;
using GTA_SA_PathsRedactor.Services;
using GTA_SA_PathsRedactor.Models;

namespace GTA_SA_PathsRedactor.ViewModel
{
    public class PathVM : INotifyPropertyChanged
    {
        private static int s_pathCounter = 0;

        private int m_currentPathIndex;
        private ObservableCollection<PathEditor> m_paths;

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

        public PathVM()
        {
            m_paths = new ObservableCollection<PathEditor>();
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
            m_removePointCommand = new RelayCommand<VisualObject>(obj => CurrentPath.RemovePoint(obj as Models.VisualObject),
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
                var newPath = new PathEditor(pathName);

                AddNewPathHelper(newPath);
                OnPropertyChanged("Paths");
            });
            m_addNewPath = new RelayCommand<PathEditor>(obj =>
            {
                AddNewPathHelper(obj as PathEditor);
            }, obj => obj is PathEditor);
            m_removePath = new RelayCommand<PathEditor>(obj =>
            {
                var pathEditor = obj as PathEditor;

                if (m_paths.Remove(pathEditor))
                {
                    PathRemoved?.Invoke(this, pathEditor);

                    CurrentPathIndex--;
                }
            }, obj => obj is PathEditor && m_paths.Count != 0);

            m_selectPath = new RelayCommand<object>(obj =>
            {
                var newIndex = -1;

                if (obj is int)
                {
                    newIndex = (int)obj;
                }
                else
                {
                    newIndex = m_paths.IndexOf((PathEditor)obj);
                }

                CurrentPathIndex = newIndex;
            }, obj => obj != null && (obj is int || obj is PathEditor));
        }

        public ObservableCollection<PathEditor> Paths => m_paths;

        public PathEditor? CurrentPath
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

                PathSelected?.Invoke(this, new PathSelectionArgs(value != -1 ? m_paths[value] : null, m_currentPathIndex, value));

                m_currentPathIndex = value;

                OnPropertyChanged();
                OnPropertyChanged("CurrentPath");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action<PathVM, PathEditor> PathAdded;
        public event Action<PathVM, PathEditor> PathRemoved;
        public event Action<PathVM, PathSelectionArgs> PathSelected;
        public event Action<PathVM, PathEditor> MapCleared;

        private async Task LoadPathHelper(string? path)
        {
            Core.IPointLoader? pointLoader = null;

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

            // pointLoader = GlobalSettings.GetInstance().CurrentLoader;
            //
            // var newPath = new PathEditor(path.Remove(0, path.LastIndexOf('\\') + 1));
            // newPath.PathFileName = path;
            // pointLoader.FileName = path;
            //
            // try
            // {
            //     var loadPointTask = pointLoader.LoadAsync();
            //
            //     await Task.WhenAny(loadPointTask, Task.Delay(30000));
            //
            //     if (loadPointTask.IsCompleted)
            //     {
            //         var points = loadPointTask.Result.Select(point =>
            //                                                  {
            //                                                      var dot = new DotVisual(point);
            //                                                      dot.Transform(GlobalSettings.GetInstance()
            //                                                                                  .GetCurrentTranfromationData());
            //                                                      return dot;
            //                                                  });
            //
            //         AddNewPathHelper(newPath);
            //
            //         newPath.AddRangePoint(points);
            //     }
            //     else App.LogErrorInfoAndShowMessageBox("Cannot parse points due to timeout. If this " +
            //                                            "will happen again, remove this custom loader and contact to creator.");
            // }
            // catch (System.IO.FileNotFoundException ex)
            // {
            //     App.LogErrorInfoAndShowMessageBox(ex.Message, ex);
            // }
            // catch (Core.PointsLoadingException ex)
            // {
            //     App.LogErrorInfoAndShowMessageBox($"Cannot parse file\n\n{ex.PointsFileName}\n\n" +
            //                                       $"An error occured on line {ex.FileErrorLine}.");
            // }
            // catch (Exception ex)
            // {
            //     App.LogErrorInfoAndShowMessageBox("An error occure while point loading.", ex);
            // }

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

            // var pointSaver = GlobalSettings.GetInstance().CurrentSaver;
            // pointSaver.CreateBackup = true;
            // pointSaver.FileName = filePath;
            //
            // try
            // {
            //     var saveTask = pointSaver.SaveAsync(CurrentPath.Dots.Select(dot => 
            //                                                                   { 
            //                                                                       dot.TransformBack(GlobalSettings.GetInstance()
            //                                                                                                       .GetCurrentTranfromationData()); 
            //                                                                       return dot.OriginPoint; 
            //                                                                   }));
            //
            //     await Task.WhenAny(saveTask, Task.Delay(30000));
            //
            //     if (!saveTask.IsCompleted)
            //         App.LogErrorInfoAndShowMessageBox("Cannot save points due to timeout. If this " +
            //                                           "will happen again, remove this custom saver and contact to creator.");
            // }
            // catch (UnauthorizedAccessException ex)
            // {
            //     App.LogErrorInfoAndShowMessageBox(ex.Message, ex);
            // }
            // catch (System.IO.FileNotFoundException ex)
            // {
            //     App.LogErrorInfoAndShowMessageBox(ex.Message, ex);
            // }
            // catch (Exception ex)
            // {
            //     App.LogErrorInfoAndShowMessageBox("An error occure while file opening.", ex);
            // }
        }

        private void AddNewPathHelper(PathEditor pathEditor)
        {
            m_paths.Add(pathEditor);

            PathAdded?.Invoke(this, pathEditor);

            CurrentPathIndex = m_paths.Count - 1;
        }

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}


//if (path.EndsWith(".exe"))
//{
//    var fileDirectory = path.Remove(path.LastIndexOf('\\')) + @"\data\Paths";

//    if (System.IO.Directory.Exists(fileDirectory))
//    {
//        var directoryFiles = System.IO.Directory.GetFiles(fileDirectory);
//        var trainPaths = directoryFiles.Where(path => path.Contains("tracks"));

//        if (!trainPaths.Any())
//        {
//            MessageBox.Show($"Cannot find game directory \"{fileDirectory}\".", "Error",
//                            MessageBoxButton.OK, MessageBoxImage.Error);
//            return;
//        }

//        pointLoader = new DefaultPointSaverLoader(trainPaths.First());

//        var pathsEnumerator = trainPaths.GetEnumerator();
//        pathsEnumerator.MoveNext();

//        do
//        {
//            var currentPath = pointLoader.FilePath;
//            var newPath = new PathEditor(currentPath.Remove(0, currentPath.LastIndexOf('\\') + 1));
//            var points = await pointLoader.LoadAsync();

//            AddNewPathHelper(newPath);

//            newPath.AddRangePoint(points);

//            if (pathsEnumerator.MoveNext())
//                pointLoader.FilePath = pathsEnumerator.Current;
//            else
//                break;
//        } while (true);
//    }
//    else
//    {
//        MessageBox.Show($"Cannot find game directory\n\n**GTA SA DIRECTORY**\\data\\Paths", "Error",
//                        MessageBoxButton.OK, MessageBoxImage.Error);
//    }
//}