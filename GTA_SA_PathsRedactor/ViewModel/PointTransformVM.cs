using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GTA_SA_PathsRedactor.Services;
using System.ComponentModel;

namespace GTA_SA_PathsRedactor.ViewModel
{
    internal class PointTransformVM : INotifyPropertyChanged
    {
        private int m_currentPointTransformDataIndex;

        private RelayCommand m_saveSettings;
        private RelayCommand m_loadSettings;
        private RelayCommand m_goToMainMenu;

        private List<PointTransformationData> m_pointsTransformationData;

        public event PropertyChangedEventHandler PropertyChanged;

        public PointTransformVM()
        {
            m_pointsTransformationData = new List<PointTransformationData>();

            m_saveSettings = new RelayCommand(SaveSettingCommandHandler);
            m_loadSettings = new RelayCommand(LoadSettingCommandHandler);
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

        public int CurrentPointTransformDataIndex
        {
            get { return m_currentPointTransformDataIndex; }
            set 
            { 
                if (m_currentPointTransformDataIndex < -1 || 
                    m_currentPointTransformDataIndex >= m_pointsTransformationData.Count)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (m_currentPointTransformDataIndex == -1)
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
            get { return m_goToMainMenu ?? new RelayCommand(obj => { }); }
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
        }

        public void SaveSettingsToFile()
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            string filePath = @$"{Environment.CurrentDirectory}\PointsSetting.json";

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Truncate, FileAccess.Write))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    jsonSerializer.Serialize(streamWriter, m_pointsTransformationData);
                }
            }
        }
        public void LoadSettingsToFile()
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            string filePath = @$"{Environment.CurrentDirectory}\PointsSetting.json";

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    Clear();
                    AddNewPointTransformationData((List<PointTransformationData>)jsonSerializer.Deserialize(streamReader, typeof(List<PointTransformationData>)));
                }
            }
        }

        private void SaveSettingCommandHandler(object obj)
        {
            SaveSettingsToFile();
        }
        private void LoadSettingCommandHandler(object obj)
        {

        }

        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

    
}
