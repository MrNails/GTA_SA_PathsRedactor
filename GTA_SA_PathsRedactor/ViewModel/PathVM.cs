using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
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

        private RelayCommand m_addPointCommand;
        private RelayCommand m_insertPointCommand;
        private RelayCommand m_removePointCommand;
        private RelayCommand m_removeSelectedPointsCommand;
        private RelayCommand m_clearPointsCommand;

        private RelayCommand m_savePath;
        private RelayCommand m_savePathAs;
        private RelayCommand m_loadPath;

        private RelayCommand m_createNewPath;
        private RelayCommand m_addNewPath;
        private RelayCommand m_removePath;

        private RelayCommand m_selectPath;

        public PathVM()
        {
            m_paths = new ObservableCollection<PathEditor>();
            m_currentPathIndex = -1;

            m_addPointCommand = new RelayCommand(obj =>
            {
                System.Diagnostics.Debug.WriteLine(obj);

                var dot = obj as Models.VisualObject ??
                          new Models.DotVisual(obj as Core.Models.WorldPoint);

                CurrentPath.AddPoint(dot);
            }, obj => obj != null && m_paths.Count != 0 &&
                      (obj is Models.VisualObject ||
                       obj is Core.Models.WorldPoint));
            m_removePointCommand = new RelayCommand(obj => CurrentPath.RemovePoint(obj as Models.VisualObject),
                                                    obj => obj is Models.VisualObject);
            m_removeSelectedPointsCommand = new RelayCommand(obj => CurrentPath.RemoveSelectedPoints(),
                                                             obj => CurrentPath != null && CurrentPath.SelectedDots.Count != 0);

            m_clearPointsCommand = new RelayCommand(obj =>
                                                    {
                                                        CurrentPath.Clear();
                                                        MapCleared?.Invoke(this, CurrentPath);
                                                    }, 
                                                    obj => CurrentPath != null && CurrentPath.PointCount != 0);

            m_loadPath = new RelayCommand(async obj => await LoadPathHelper(obj as string));
            m_savePath = new RelayCommand(async obj => await SavePathHelper(false), obj => m_currentPathIndex != -1);
            m_savePathAs = new RelayCommand(async obj => await SavePathHelper(true), obj => m_currentPathIndex != -1);

            m_createNewPath = new RelayCommand(obj =>
            {
                string pathName = obj as string ?? "New path " + (++s_pathCounter).ToString();
                var newPath = new PathEditor(pathName);

                AddNewPathHelper(newPath);
                OnPropertyChanged("Paths");
            });
            m_addNewPath = new RelayCommand(obj =>
            {
                AddNewPathHelper(obj as PathEditor);
            }, obj => obj is PathEditor);
            m_removePath = new RelayCommand(obj =>
            {
                var pathEditor = obj as PathEditor;

                if (m_paths.Remove(pathEditor))
                {
                    PathRemoved?.Invoke(this, pathEditor);

                    CurrentPathIndex--;
                }
            }, obj => obj is PathEditor && m_paths.Count != 0);

            m_selectPath = new RelayCommand(obj =>
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

        public RelayCommand AddPointCommand => m_addPointCommand;
        public RelayCommand InsertPointCommand => m_insertPointCommand;
        public RelayCommand RemovePointCommand => m_removePointCommand;
        public RelayCommand RemoveSelectedPointsCommand => m_removeSelectedPointsCommand;
        public RelayCommand ClearPointsCommand => m_clearPointsCommand;

        public RelayCommand SaveCurrentPath => m_savePath;
        public RelayCommand SaveCurrentPathAs => m_savePathAs;
        public RelayCommand LoadPath => m_loadPath;

        public RelayCommand AddNewPathCommand => m_addNewPath;
        public RelayCommand CreateNewPathCommand => m_createNewPath;
        public RelayCommand RemovePathCommand => m_removePath;

        public RelayCommand SelectPathCommand => m_selectPath;

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
            Core.PointLoader? pointLoader = null;

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

            pointLoader = GlobalSettings.GetInstance().CurrentLoader;

            var newPath = new PathEditor(path.Remove(0, path.LastIndexOf('\\') + 1));
            newPath.PathFileName = path;
            pointLoader.FileName = path;

            try
            {
                var loadPointTask = pointLoader.LoadAsync();

                await Task.WhenAny(loadPointTask, Task.Delay(30000));

                if (loadPointTask.IsCompleted)
                {
                    var points = loadPointTask.Result.Select(point =>
                                                             {
                                                                 var dot = new DotVisual(point);
                                                                 dot.Transform(GlobalSettings.GetInstance()
                                                                                             .GetCurrentTranfromationData());
                                                                 return dot;
                                                             });

                    AddNewPathHelper(newPath);

                    newPath.AddRangePoint(points);
                }
                else App.LogErrorInfoAndShowMessageBox("Cannot parse points due to timeout. If this " +
                                                       "will happen again, remove this custom loader and contact to creator.");
            }
            catch (System.IO.FileNotFoundException ex)
            {
                App.LogErrorInfoAndShowMessageBox(ex.Message, ex);
            }
            catch (Core.PointsLoadingException ex)
            {
                App.LogErrorInfoAndShowMessageBox($"Cannot parse file\n\n{ex.PointsFileName}\n\n" +
                                                  $"An error occured on line {ex.FileErrorLine}.");
            }
            catch (Exception ex)
            {
                App.LogErrorInfoAndShowMessageBox("An error occure while point loading.", ex);
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

            var pointSaver = GlobalSettings.GetInstance().CurrentSaver;
            pointSaver.CreateBackup = true;
            pointSaver.FileName = filePath;

            try
            {
                var saveTask = pointSaver.SaveAsync(CurrentPath.Dots.Select(dot => 
                                                                              { 
                                                                                  dot.TransformBack(GlobalSettings.GetInstance()
                                                                                                                  .GetCurrentTranfromationData()); 
                                                                                  return dot.OriginPoint; 
                                                                              }));

                await Task.WhenAny(saveTask, Task.Delay(30000));

                if (!saveTask.IsCompleted)
                    App.LogErrorInfoAndShowMessageBox("Cannot save points due to timeout. If this " +
                                                      "will happen again, remove this custom saver and contact to creator.");
            }
            catch (UnauthorizedAccessException ex)
            {
                App.LogErrorInfoAndShowMessageBox(ex.Message, ex);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                App.LogErrorInfoAndShowMessageBox(ex.Message, ex);
            }
            catch (Exception ex)
            {
                App.LogErrorInfoAndShowMessageBox("An error occure while file opening.", ex);
            }
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