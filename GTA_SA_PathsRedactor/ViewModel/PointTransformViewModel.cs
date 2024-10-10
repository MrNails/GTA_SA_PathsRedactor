using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using GTA_SA_PathsRedactor.Services;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using GTA_SA_PathsRedactor.Models;
using Microsoft.Win32;

namespace GTA_SA_PathsRedactor.ViewModel
{
    public class PointTransformViewModel : INotifyPropertyChanged
    {
        private int m_currentPointTransformDataIndex;

        private ICommand m_saveSetting;
        private ICommand m_loadSetting;
        private ICommand m_goToMainMenu;

        private ICommand m_addNewSetting;
        private ICommand m_removeCurrentSetting;

        private ObservableCollection<PointTransformationData> m_pointsTransformationDatas;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PointTransformViewModel()
        {
            m_pointsTransformationDatas = new ObservableCollection<PointTransformationData>();
            m_pointsTransformationDatas.CollectionChanged += PointsTransformationData_CollectionChanged;

            m_saveSetting = new RelayCommand(SaveSettingCommandHandler);
            m_loadSetting = new RelayCommand(LoadSettingCommandHandler);

            m_addNewSetting = new RelayCommand(() => m_pointsTransformationDatas.Add(new PointTransformationData()));
            m_removeCurrentSetting = new RelayCommand<PointTransformationData>(obj => m_pointsTransformationDatas.Remove(obj)
                                                      //obj => obj != null && obj is PointTransformationData && obj != GlobalSettings.GetInstance().DefaultPTD
                                                      );

            m_currentPointTransformDataIndex = -1;
        }
      
        public ICommand SaveSettingsCommand => m_saveSetting;
        public ICommand LoadSettingsCommand => m_loadSetting;

        public ICommand AddNewSettingCommand => m_addNewSetting;
        public ICommand RemoveCurrentSettingCommand => m_removeCurrentSetting;

        public PointTransformationData? CurrentPointTransformData 
        {
            get
            {
                if (m_currentPointTransformDataIndex == -1)
                {
                    return null;
                }

                return m_pointsTransformationDatas[m_currentPointTransformDataIndex];
            }
        }
        public ObservableCollection<PointTransformationData> PointTranformationDatas
        {
            get
            {
                return m_pointsTransformationDatas;
            }
        }

        public int CurrentPointTransformDataIndex
        {
            get { return m_currentPointTransformDataIndex; }
            set 
            {
                if (value < -1 || value >= m_pointsTransformationDatas.Count)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                m_currentPointTransformDataIndex = value;

                OnPropertyChanged("CurrentPointTransformDataIndex");
                OnPropertyChanged("CurrentPointTransformData");

                // GlobalSettings.GetInstance().PTD = CurrentPointTransformData;
            }
        }

        public ICommand GoToMainMenu
        {
            get => m_goToMainMenu;
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
            m_pointsTransformationDatas.Clear();
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
                m_pointsTransformationDatas.Add(new PointTransformationData());
            }
        }
        public void AddNewPointTransformationData(PointTransformationData pointTransformationData)
        {
            m_pointsTransformationDatas.Add(pointTransformationData);
        }

        public void AddNewPointTransformationDataRange(IEnumerable<PointTransformationData> pointsTransformationData)
        {
            foreach (var item in pointsTransformationData)
            {
                m_pointsTransformationDatas.Add(item);
            }

            if (m_pointsTransformationDatas.Any())
            {
                CurrentPointTransformDataIndex = m_pointsTransformationDatas.Count - 1;
            }
        }

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
                MessageBox.Show($"{ex.Message}\n{ex.StackTrace}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
#else
                MessageBox.Show("An error ocured while settings saving.", "Information",
                                MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void PointsTransformationData_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    CurrentPointTransformDataIndex = m_pointsTransformationDatas.Count - 1;
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (CurrentPointTransformDataIndex >= m_pointsTransformationDatas.Count)
                    {
                        CurrentPointTransformDataIndex = m_pointsTransformationDatas.Count - 1;
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
