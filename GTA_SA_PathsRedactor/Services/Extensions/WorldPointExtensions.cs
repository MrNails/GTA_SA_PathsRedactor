using System;
using System.Windows;
using GTA_SA_PathsRedactor.Core.Models;

namespace GTA_SA_PathsRedactor.Services.Extensions;

public static class WorldPointExtensions
{
    public static double DistanceTo(this WorldPoint left, Point right)
    {
        return Math.Sqrt(Math.Pow(left.X - right.X, 2) + Math.Pow(left.Y - right.Y, 2));
    }
}