using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GTA_SA_PathsRedactor.Services;
using Microsoft.Win32;

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
                var dot = obj as Models.VisualObject ??
                          new Models.DotVisual(obj as Core.Models.GTA_SA_Point);

                CurrentPath.AddPoint(dot);
            }, obj => obj != null && m_paths.Count != 0 && 
                                                                   (obj is Models.VisualObject || 
                                                                    obj is Core.Models.GTA_SA_Point));
            m_removePointCommand = new RelayCommand(obj => CurrentPath.RemovePoint(obj as Models.VisualObject), 
                                                    obj => obj is Models.VisualObject);
            m_removeSelectedPointsCommand = new RelayCommand(obj => CurrentPath.RemoveSelectedPoint(), 
                                                             obj => CurrentPath != null && CurrentPath.SelectedDots.Count != 0);

            m_clearPointsCommand = new RelayCommand(obj => CurrentPath.Clear(), obj => CurrentPath != null && CurrentPath.PointCount != 0);

            m_loadPath = new RelayCommand(async obj =>
            {
                PointSaverLoader? pointLoader = null;
                string? path = obj as string;

                if (path == null)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "exe files (*.exe) |*.exe";

                    if (openFileDialog.ShowDialog() == true)
                    {
                        path = openFileDialog.FileName;
                    }
                    else
                    {
                        return;
                    }
                }

                pointLoader = new PointSaverLoader(path);

                var newPath = new PathEditor(path.Remove(0, path.LastIndexOf('\\') + 1));

                AddNewPathHelper(newPath);

                newPath.AddRangePoint(await pointLoader.LoadAsync());
            });

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
                    CurrentPathIndex--;

                    PathRemoved?.Invoke(this, pathEditor);
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

        private void AddNewPathHelper(PathEditor pathEditor)
        {
            m_paths.Add(pathEditor);
            CurrentPathIndex = m_paths.Count - 1;

            PathAdded?.Invoke(this, pathEditor);
        }

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
