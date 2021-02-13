using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using GTA_SA_PathsRedactor.Services;
using System.ComponentModel;
using System.Windows;

namespace GTA_SA_PathsRedactor.ViewModel
{
    public class PointTransformVM : INotifyPropertyChanged
    {
        private int m_currentPointTransformDataIndex;

        private RelayCommand m_saveSettings;
        private RelayCommand m_loadSettings;
        private RelayCommand m_goToMainMenu;

        private TransformSettingLoader transformSettingLoader;
        private List<PointTransformationData> m_pointsTransformationData;

        public event PropertyChangedEventHandler PropertyChanged;

        public PointTransformVM()
        {
            m_pointsTransformationData = new List<PointTransformationData>();

            m_saveSettings = new RelayCommand(SaveSettingCommandHandler);
            m_loadSettings = new RelayCommand(LoadSettingCommandHandler);

            transformSettingLoader = new TransformSettingLoader( @$"{Environment.CurrentDirectory}\PointsSetting.json");
        }

        public RelayCommand SaveSettings => m_saveSettings;
        public RelayCommand LoadSettings => m_loadSettings;
        public PointTransformationData CurrentPointTransformData 
        {
            get
            {
                if (m_currentPointTransformDataIndex == -1)
                {
                    return null;
                }

                return m_pointsTransformationData[m_currentPointTransformDataIndex];
            }
        }
        public System.Collections.ObjectModel.ReadOnlyCollection<PointTransformationData> PointTranformationDatas
        {
            get
            {
                return m_pointsTransformationData.AsReadOnly();
            }
        }

        public int CurrentPointTransformDataIndex
        {
            get { return m_currentPointTransformDataIndex; }
            set 
            { 
                if (value < -1 || value >= m_pointsTransformationData.Count)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (value == -1)
                {
                    m_pointsTransformationData.Clear();
                }

                m_currentPointTransformDataIndex = value;

                OnPropertyChanged("CurrentPointTransformDataIndex");
                OnPropertyChanged("CurrentPointTransformData");
            }
        }

        public RelayCommand GoToMainMenu
        {
            get { return m_goToMainMenu ?? (m_goToMainMenu = new RelayCommand(obj => { })); }
            set 
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_goToMainMenu = value;
                OnPropertyChanged("GoToMainMenu");
            }
        }

        public void Clear()
        {
            m_pointsTransformationData.Clear();
            CurrentPointTransformDataIndex = -1;
        }

        public void AddNewPointTransformationData()
        {
            AddNewPointTransformationData(new PointTransformationData());
        }
        public void AddNewPointTransformationData(int count)
        {
            for (int i = 0; i < count; i++)
            {
                m_pointsTransformationData.Add(new PointTransformationData());
            }
        }
        public void AddNewPointTransformationData(PointTransformationData pointTransformationData)
        {
            if (pointTransformationData == null)
            {
                throw new ArgumentNullException("pointTransformationData");
            }

            m_pointsTransformationData.Add(pointTransformationData);
        }
        public void AddNewPointTransformationData(IEnumerable<PointTransformationData> pointsTransformationData)
        {
            if (pointsTransformationData == null)
            {
                throw new ArgumentNullException("pointsTransformationData");
            }

            m_pointsTransformationData.AddRange(pointsTransformationData);

            if (m_pointsTransformationData.Any())
            {
                CurrentPointTransformDataIndex = 0;
            }
        }

        private void SaveSettingCommandHandler(object  obj)
        {
            try
            {
                transformSettingLoader.SaveSettings(m_pointsTransformationData);

                MessageBox.Show("Settings saved succesfully", "Information",
                                MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show($"{ex.Message}\n{ex.StackTrace}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
#else
                MessageBox.Show("An error ocured while settings saving.", "Information",
                                MessageBoxButton.OK, MessageBoxImage.Information);
#endif

            }
        }
        private void LoadSettingCommandHandler(object obj)
        {
            try
            {
                var settings = transformSettingLoader.LoadSettings();

                foreach (var setting in settings)
                {
                    setting.PropertyChanged += Setting_PropertyChanged;
                }

                Clear();
                AddNewPointTransformationData(settings);

                MessageBox.Show("Settings loaded succesfully", "Information",
                                MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Specified file not found.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show($"{ex.Message}\n{ex.StackTrace}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
#else
                MessageBox.Show("An error ocured while settings loading.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
#endif

            }
        }

        private void Setting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
