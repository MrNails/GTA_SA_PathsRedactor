using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GTA_SA_PathsRedactor.Services
{
    public class HistoryController : INotifyPropertyChanged
    {
        private static readonly int s_maxHistoryElems = 75;
        
        private List<IStorableValue> m_historyList;
        private IStorableValue? m_newThresholdValue;
        private int m_currentPos;
        private bool m_isOverThreshold;

        public event PropertyChangedEventHandler? PropertyChanged;

        public HistoryController()
        {
            m_historyList = new List<IStorableValue>();
            m_currentPos = -1;
        }

        public int HistoryCount => m_historyList.Count;

        public bool IsPositionOnStart => m_currentPos == -1;

        public bool IsPositionOnEnd => m_currentPos == m_historyList.Count - 1;

        public bool IsOverThreshold => m_isOverThreshold;

        public bool HasChagned => m_isOverThreshold || CurrentElement != m_newThresholdValue;

        public IStorableValue? CurrentElement
        {
            get
            {
                if (m_currentPos == -1)
                    return default;

                return m_historyList[m_currentPos];
            }
        }

        public int CurrentPosition
        {
            get => m_currentPos;
        }

        public void AddNew(IStorableValue elem)
        {
            if (m_currentPos != m_historyList.Count - 1)
            {
                var tresholdValueIndex = m_historyList.IndexOf(m_newThresholdValue);

                if (tresholdValueIndex > m_currentPos)
                    m_newThresholdValue = null;

                if (m_currentPos != -1)
                    m_historyList.RemoveRange(m_currentPos + 1, m_historyList.Count - m_currentPos - 1);
                else
                    m_historyList.RemoveRange(0, m_historyList.Count);
            }

            if (m_historyList.Count == s_maxHistoryElems)
            {
                if (m_newThresholdValue == null)
                {
                    m_isOverThreshold = true;
                }
                else if (m_historyList[0] == m_newThresholdValue)
                {
                    m_newThresholdValue = null;
                    m_isOverThreshold = true;
                }

                m_historyList.RemoveAt(0);
            }

            m_historyList.Add(elem);
            m_currentPos = m_historyList.Count - 1;

            OnPropertyChanged("HistoryCount");
            OnPropertyChanged("CurrentElement");
            OnPropertyChanged("HasChagned");
        }

        public bool MoveLeft()
        {
            if (m_currentPos == -1)
                return false;

            m_currentPos--;

            OnPropertyChanged("CurrentPosition");
            OnPropertyChanged("CurrentElement");
            OnPropertyChanged("HasChagned");
            OnPropertyChanged("IsPositionOnEnd");
            OnPropertyChanged("IsPositionOnStart");

            return true;
        }

        public bool MoveRight()
        {
            if (m_currentPos == m_historyList.Count - 1 || m_historyList.Count == 0)
                return false;

            m_currentPos++;

            OnPropertyChanged("CurrentPosition");
            OnPropertyChanged("CurrentElement");
            OnPropertyChanged("HasChagned");
            OnPropertyChanged("IsPositionOnEnd");
            OnPropertyChanged("IsPositionOnStart");

            return true;
        }

        public bool RemoveLast()
        {
            if (m_historyList.Count == 0)
                return false;

            if (m_newThresholdValue == m_historyList[m_historyList.Count - 1])
                m_newThresholdValue = null;

            m_historyList.RemoveAt(m_historyList.Count - 1);

            if (m_currentPos == m_historyList.Count)
            {
                m_currentPos--;
                OnPropertyChanged("CurrentPosition");
            }

            OnPropertyChanged("HistoryCount");
            OnPropertyChanged("CurrentElement");
            OnPropertyChanged("HasChagned");
            OnPropertyChanged("IsPositionOnEnd");
            OnPropertyChanged("IsPositionOnStart");

            return true;
        }

        public void ClearHistory()
        {
            m_currentPos = -1;
            m_historyList.Clear();
            m_isOverThreshold = false;

            OnPropertyChanged("CurrentPosition");
            OnPropertyChanged("HistoryCount");
            OnPropertyChanged("CurrentElement");
            OnPropertyChanged("HasChagned");
            OnPropertyChanged("IsPositionOnEnd");
            OnPropertyChanged("IsPositionOnStart");
        }

        public void SetNewOverloadThresholdElem(int index)
        {
            if (index < 0 || index >= m_historyList.Count)
                throw new ArgumentOutOfRangeException("index");

            m_newThresholdValue = m_historyList[index];
            m_isOverThreshold = false;

            OnPropertyChanged("HasChagned");
        }

        protected void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
