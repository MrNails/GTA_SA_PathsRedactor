using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using GTA_SA_PathsRedactor.Core;

namespace GTA_SA_PathsRedactor.Services
{
    public sealed partial  class PointTransformationData : Entity
    {
        private Matrix _matrixTransform;
        
        [ObservableProperty]
        private double _offsetX;
        [ObservableProperty]
        private double _offsetY;
        [ObservableProperty]
        private double _pointScaleX;
        [ObservableProperty]
        private double _pointScaleY;
        
        private double _originalMapWidth;
        private double _originalMapHeight;
        private string _transformName;

        public PointTransformationData()
        {
            _transformName = string.Empty;
            _matrixTransform = new Matrix();
        }
        
        public double OriginalMapWidth
        {
            get => _originalMapWidth;
            set
            {
                _errors[nameof(OriginalMapWidth)] = value <= 0 
                    ? "Map original Width cannot be less than 0." 
                    : string.Empty;
                
                _originalMapWidth = value;
                OnPropertyChanged();
            }
        }
        public double OriginalMapHeight
        {
            get => _originalMapHeight;
            set
            {
                _errors[nameof(OriginalMapHeight)] = value <= 0 
                        ? "Map original Height cannot be less than 0." 
                        : string.Empty;

                _originalMapHeight = value;
                OnPropertyChanged();
            }
        }
        public string TransformName
        {
            get => _transformName;
            set
            {
                _errors["TransformName"] = string.IsNullOrEmpty(value) 
                    ? "Transformation name can't be empty." 
                    : string.Empty;

                _transformName = value;
                OnPropertyChanged();
            }
        }
    }
}
