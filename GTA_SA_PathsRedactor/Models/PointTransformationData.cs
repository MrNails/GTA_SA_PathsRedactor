using System.Numerics;
using System.Windows.Media;
using GTA_SA_PathsRedactor.Core;

namespace GTA_SA_PathsRedactor.Services
{
    public sealed partial class PointTransformationData : Entity
    {
        private Matrix _matrixTransform;
        private double _offsetX;
        private double _offsetY;
        private double _pointScaleX;
        private double _pointScaleY;

        private double _originalMapWidth;
        private double _originalMapHeight;

        public PointTransformationData()
        {
            _matrixTransform = Matrix.Identity;
        }

        public Matrix MatrixTransform
        {
            get => _matrixTransform;
            private set => SetProperty(ref _matrixTransform, value);
        }

        public double OffsetX
        {
            get => _offsetX;
            set
            {
                SetProperty(ref _offsetX, value);

                _matrixTransform.OffsetX = value;
                OnPropertyChanged(nameof(MatrixTransform));
            }
        }
        public double OffsetY
        {
            get => _offsetY;
            set
            {
                SetProperty(ref _offsetY, value);

                _matrixTransform.OffsetY = value;
                OnPropertyChanged(nameof(MatrixTransform));
            }
        }
        public double PointScaleX
        {
            get => _pointScaleX;
            set
            {
                SetProperty(ref _pointScaleX, value);
                ApplyScale(in _matrixTransform);
            }
        }
        public double PointScaleY
        {
            get => _pointScaleY;
            set
            {
                SetProperty(ref _pointScaleY, value);
                ApplyScale(in _matrixTransform);
            }
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

        private void ApplyScale(in Matrix matrix)
        {
            matrix.Scale(PointScaleX, PointScaleY);
            MatrixTransform = matrix;
        }
    }
}
