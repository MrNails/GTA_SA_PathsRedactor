using System;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;

#pragma warning disable MVVMTK0034
namespace GTA_SA_PathsRedactor.Core.Models
{
    public sealed partial class WorldPoint : Entity, ICloneable, IEquatable<WorldPoint>
    {
        [ObservableProperty]
        private Vector3 _position;
        
        [ObservableProperty]
        private bool _isStopPoint;

        [ObservableProperty]
        private bool _isSelected;

        public WorldPoint() : this(0, 0, 0, false)
        { }
        public WorldPoint(float x, float y, float z, bool isStopPoint)
        {
            X = x;
            Y = y;
            Z = z;
            IsStopPoint = isStopPoint;
        }

        public float X
        {
            get => _position.X;
            set
            {
                _position.X = value;
                OnPropertyChanged();
            }
        }
        public float Y
        {
            get => _position.Y;
            set
            {
                _position.Y = value;
                OnPropertyChanged();
            }
        }
        public float Z
        {
            get => _position.Z;
            set
            {
                _position.Z = value;
                OnPropertyChanged();
            }
        }

        public object Clone()
        {
            return new WorldPoint();
        }
        
        public override int GetHashCode()
        {
            return _position.GetHashCode() ^ _isStopPoint.GetHashCode();
        }
        public override string ToString()
        {
            return $"Position: {_position}; IsStop = {_isStopPoint}";
        }

        public override bool Equals(object? obj)
        {
            return obj is WorldPoint worldPoint && Equals(worldPoint);
        }
        public bool Equals(WorldPoint? other)
        {
            if (other is null)
                return false;

            return _position == other._position &&
                   _isStopPoint == other._isStopPoint;
        }

        public void CopyTo(WorldPoint other)
        {
            other._position = _position;
            other.IsStopPoint = _isStopPoint;
        }

        public double DistanceTo(WorldPoint other)
        {
            return Math.Sqrt(Math.Pow(_position.X - other._position.X, 2) + 
                             Math.Pow(_position.Y - other._position.Y, 2) +
                             Math.Pow(_position.Z - other._position.Z, 2));
        }

        public static bool operator ==(WorldPoint? left, WorldPoint? right)
        {
            return left?.Equals(right) ?? ReferenceEquals(right, null);
        }
        public static bool operator !=(WorldPoint? left, WorldPoint? right)
        {
            return !(left == right);
        }
    }
}
#pragma warning restore MVVMTK0034
