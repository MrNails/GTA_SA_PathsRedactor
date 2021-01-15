using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTA_SA_PathsRedactor.ViewModel
{
    public class PathEditor : INotifyPropertyChanged
    {
        private PathVisualizer.Path m_path;
        private PathVisualizer.VisualObject m_currentVisualObject;

        private Services.PointLoader m_pointLoader;

        private Services.RelayCommand m_addPointCommand;
        private Services.RelayCommand m_removePointCommand;

        public PathEditor(string pathName)
        {
            m_path = new PathVisualizer.Path(pathName);

            m_addPointCommand = new Services.RelayCommand(AddPoint, CanExecuteCommand);
            m_removePointCommand = new Services.RelayCommand(RemovePoint, CanExecuteCommand);
        }

        public Services.RelayCommand AddPointCommand { get => m_addPointCommand; }
        public Services.RelayCommand RemovePointCommand { get => m_removePointCommand; }

        public PathVisualizer.Path Path
        {
            get { return m_path; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_path = value;
                OnPropertyChanged("Path");
            }
        }
        public PathVisualizer.VisualObject CurrentVisualObject
        {
            get { return m_currentVisualObject; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_currentVisualObject = value;
                OnPropertyChanged("CurrentVisualObject");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void AddPoint(object visualObject)
        {
            if (CanExecuteCommand(visualObject))
            {
                m_path.AddPoint((PathVisualizer.VisualObject)visualObject);
            }
        }
        public void RemovePoint(object visualObject)
        {
            if (CanExecuteCommand(visualObject))
            {
                m_path.RemovePoint((PathVisualizer.VisualObject)visualObject);
            }
        }
        public async Task LoadPointAsync()
        {
            if (m_pointLoader == null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "exe files (*.ext) |*.exe";

                if (openFileDialog.ShowDialog() == true)
                {
                    if (m_pointLoader == null)
                    {
                        m_pointLoader = new Services.PointLoader(openFileDialog.FileName);
                    }
                }
                else
                {
                    return;
                }
            }

            m_path.AddRangePoint(await m_pointLoader.LoadPointsAsync());
        }

        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private bool CanExecuteCommand(object visualObject)
        {
            return visualObject != null && visualObject is PathVisualizer.VisualObject;
        }
    }
}
