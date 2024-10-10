using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GTA_SA_PathsRedactor.Core;
using GTA_SA_PathsRedactor.Core.Models;

namespace OtherNameSpace.Test
{
    public sealed partial class TestSaver2 : ObservableObject, IPointSaver
    {
        [ObservableProperty]
        private string _fileName;
        
        [ObservableProperty]
        private bool _createBackup;
        
        public Task SaveAsync(IEnumerable<WorldPoint> points, CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debug.WriteLine($"TestSaver2 Invoked SaveAsync(IEnumerable<GTA_SA_Point>, CancellationToken) ({DateTime.Now})");
            return Task.Delay(1000, cancellationToken);
        }
        
        public void Dispose()
        {
            FileName = string.Empty;
        }
    }
}
