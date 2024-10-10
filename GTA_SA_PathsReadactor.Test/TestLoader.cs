using System;
using System.Collections.Generic;
using System.Linq ;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GTA_SA_PathsRedactor.Core;
using GTA_SA_PathsRedactor.Core.Models;

namespace GTA_SA_PathsRedactor.Test
{
    public sealed partial class TestLoader : Entity, IPointLoader
    {
        [ObservableProperty]
        private string _fileName;

        public async Task<IEnumerable<WorldPoint>> LoadAsync(CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debug.WriteLine($"TestLoader Invoked LoadAsync(CancellationToken) ({DateTime.Now})");

            await Task.Delay(50000, cancellationToken);

            return new List<WorldPoint>().AsEnumerable();
        }

        public void Dispose()
        {
            FileName = string.Empty;
        }
    }
}
