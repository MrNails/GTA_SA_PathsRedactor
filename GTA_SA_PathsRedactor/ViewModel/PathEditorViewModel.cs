using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GTA_SA_PathsRedactor.Core.Models;
using GTA_SA_PathsRedactor.Core;
using GTA_SA_PathsRedactor.Models.Dto;

namespace GTA_SA_PathsRedactor.ViewModel
{
    public sealed partial class PathEditorViewModel : Entity
    {
        private readonly ObservableCollection<WorldPoint> _points;
        
        private string _pathName;
        private string _pathFileName;
        private double _linesThickness;
        private Brush _pathColor;
        
        private WorldPoint? _selectedPoint;

        private ICommand? _addPointCommand;
        private ICommand? _removePointCommand;
        private ICommand? _insertPointCommand;
        private ICommand? _clearSelectionCommand;
        private ICommand? _selectPointsCommand;

        public PathEditorViewModel(string pathName) : this(pathName, Brushes.Red) { }

        public PathEditorViewModel(string pathName, SolidColorBrush linesColor)
        {
            _points = new ObservableCollection<WorldPoint>();

            _pathName = pathName;
            _pathFileName = string.Empty;
            _pathColor = linesColor;
            
            LinesThickness = 2;
        }
        
        public IReadOnlyList<WorldPoint> Points => _points;
        public IEnumerable<WorldPoint> SelectedPoints => _points.Where(point => point.IsSelected);

        public string PathName
        {
            get => _pathName;
            set
            {
                _errors[nameof(PathName)] = value.Length == 0 ? "Path name can't be empty." : string.Empty;

                _pathName = value;
                OnPropertyChanged();
            }
        }

        public string PathFileName
        {
            get => _pathFileName;
            set
            {
                _errors[nameof(PathFileName)] = value.Length == 0 ? "File path can't be empty." : string.Empty;

                _pathFileName = value;
                OnPropertyChanged();
            }
        }

        public double LinesThickness
        {
            get => _linesThickness;
            set
            {
                if (value < 0)
                {
                    _errors[nameof(LinesThickness)] = "Path's lines thickness can't be less than zero.";
                    _linesThickness = 0;
                } 
                
                _errors[nameof(LinesThickness)] = string.Empty;
                _linesThickness = value;
                OnPropertyChanged();
            }
        }

        public Brush Color
        {
            get => _pathColor;
            set
            {
                _pathColor = value;
                OnPropertyChanged();
            }
        }

        public WorldPoint? SelectedPoint
        {
            get => _selectedPoint ?? SelectedPoints.FirstOrDefault();
            set
            {
                if (value is not null)
                    ClearSelectionExecute();

                _selectedPoint = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedPoints));
            }
        }

        public ICommand AddPointCommand => _addPointCommand 
            ??= new RelayCommand<WorldPoint>(point => _points.Add(point!), point => point is not null);
        public ICommand RemovePointCommand => _removePointCommand 
            ??= new RelayCommand<WorldPoint>(point => _points.Remove(point!), point => point is not null);
        
        public ICommand InsertPointCommand => _insertPointCommand 
            ??= new RelayCommand<InsertPointDto>(dto => _points.Insert(dto!.IndexToInsert, dto.Point), 
                                                 dto => dto is not null && 
                                                                 dto.IndexToInsert >= 0 && 
                                                                 dto.IndexToInsert < _points.Count);

        public ICommand ClearSelectionCommand => _clearSelectionCommand
            ??= new RelayCommand(ClearSelectionExecute);

        public ICommand SelectPointsCommand => _selectPointsCommand
            ??= new RelayCommand<Rect>(SelectPointsExecute);
        
        public void Clear()
        {
            _points.Clear();
            
            OnPropertyChanged(nameof(Points));
            OnPropertyChanged(nameof(SelectedPoints));
        }

        private void ClearSelectionExecute()
        {
            foreach (var selectedPoint in SelectedPoints)
            {
                selectedPoint.IsSelected = false;
            }

            SelectedPoint = null;
            
            OnPropertyChanged(nameof(SelectedPoints));
        }

        private void SelectPointsExecute(Rect rect)
        {
            foreach (var point in _points)
            {
                point.IsSelected = point.X >= rect.TopLeft.X && point.X <= rect.TopRight.X &&
                                   point.Y >= rect.TopLeft.Y && point.Y <= rect.BottomLeft.Y;
            }

            SelectedPoint = null;
            OnPropertyChanged(nameof(SelectedPoints));
        }
    }
}