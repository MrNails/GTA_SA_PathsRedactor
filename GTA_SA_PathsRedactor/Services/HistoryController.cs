using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GTA_SA_PathsRedactor.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace GTA_SA_PathsRedactor.Services
{
    public sealed class HistoryController : ObservableObject, IHistoryController
    {
        private static readonly int MaxHistoryElems_ = 75;

        private readonly List<IStorableValue> _historyList;

        private IStorableValue? _newThresholdValue;
        private int _currentPos;
        private bool _isOverThreshold;

        private ICommand? _undoCommand;
        private ICommand? _redoCommand;

        public HistoryController()
        {
            _historyList = new List<IStorableValue>();
            _currentPos = -1;
        }

        public int HistoryCount => _historyList.Count;

        public bool IsPositionOnStart => _currentPos == -1;

        public bool IsPositionOnEnd => _currentPos == _historyList.Count - 1;

        public bool IsOverThreshold => _isOverThreshold;

        public bool HasChagned => _isOverThreshold || CurrentElement != _newThresholdValue;

        public IStorableValue? CurrentElement
        {
            get
            {
                if (_currentPos == -1)
                    return default;

                return _historyList[_currentPos];
            }
        }

        public int CurrentPosition
        {
            get => _currentPos;
        }

        public ICommand UndoCommand => _undoCommand ??= new RelayCommand(() => Undo());
        public ICommand RedoCommand => _redoCommand ??= new RelayCommand(() => Redo());

        public void AddNew(IStorableValue elem)
        {
            if (_currentPos != _historyList.Count - 1)
            {
                var tresholdValueIndex = _historyList.IndexOf(_newThresholdValue);

                if (tresholdValueIndex > _currentPos)
                    _newThresholdValue = null;

                if (_currentPos != -1)
                    _historyList.RemoveRange(_currentPos + 1, _historyList.Count - _currentPos - 1);
                else
                    _historyList.RemoveRange(0, _historyList.Count);
            }

            if (_historyList.Count == MaxHistoryElems_)
            {
                if (_newThresholdValue == null)
                {
                    _isOverThreshold = true;
                }
                else if (_historyList[0] == _newThresholdValue)
                {
                    _newThresholdValue = null;
                    _isOverThreshold = true;
                }

                _historyList.RemoveAt(0);
            }

            _historyList.Add(elem);
            _currentPos = _historyList.Count - 1;

            OnPropertyChanged("HistoryCount");
            OnPropertyChanged("CurrentElement");
            OnPropertyChanged("HasChagned");
        }

        public bool Undo()
        {
            if (_currentPos == -1)
                return false;

            _currentPos--;

            OnPropertyChanged("CurrentPosition");
            OnPropertyChanged("CurrentElement");
            OnPropertyChanged("HasChagned");
            OnPropertyChanged("IsPositionOnEnd");
            OnPropertyChanged("IsPositionOnStart");

            return true;
        }

        public bool Redo()
        {
            if (_currentPos == _historyList.Count - 1 || _historyList.Count == 0)
                return false;

            _currentPos++;

            OnPropertyChanged("CurrentPosition");
            OnPropertyChanged("CurrentElement");
            OnPropertyChanged("HasChagned");
            OnPropertyChanged("IsPositionOnEnd");
            OnPropertyChanged("IsPositionOnStart");

            return true;
        }

        public bool RemoveLast()
        {
            if (_historyList.Count == 0)
                return false;

            if (_newThresholdValue == _historyList[_historyList.Count - 1])
                _newThresholdValue = null;

            _historyList.RemoveAt(_historyList.Count - 1);

            if (_currentPos == _historyList.Count)
            {
                _currentPos--;
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
            _currentPos = -1;
            _historyList.Clear();
            _isOverThreshold = false;

            OnPropertyChanged("CurrentPosition");
            OnPropertyChanged("HistoryCount");
            OnPropertyChanged("CurrentElement");
            OnPropertyChanged("HasChagned");
            OnPropertyChanged("IsPositionOnEnd");
            OnPropertyChanged("IsPositionOnStart");
        }

        /// <summary>
        /// Set new theshold element that represent current system state i.e. system has been saved and current element is main element
        /// </summary>
        /// <param name="index">New element index</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetNewOverloadThresholdElem(int index)
        {
            if (index < 0 || index >= _historyList.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _newThresholdValue = _historyList[index];
            _isOverThreshold = false;

            OnPropertyChanged("HasChagned");
        }
    }
}
