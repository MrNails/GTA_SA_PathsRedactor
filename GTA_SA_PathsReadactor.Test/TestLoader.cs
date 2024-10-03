using System;
using System.Collections.Generic;
using System.Linq ;
using System.Threading;
using System.Threading.Tasks;
using GTA_SA_PathsRedactor.Core;
using GTA_SA_PathsRedactor.Core.Models;

namespace GTA_SA_PathsRedactor.Test
{
    public class TestLoader : IPointLoader
    {
        public override Task<IEnumerable<WorldPoint>> LoadAsync()
        {
            System.Diagnostics.Debug.WriteLine($"TestLoader Invoked LoadAsync() ({DateTime.Now})");
            return LoadAsync(CancellationToken.None);
        }

        public override async Task<IEnumerable<WorldPoint>> LoadAsync(CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"TestLoader Invoked LoadAsync(CancellationToken) ({DateTime.Now})");

            await Task.Delay(50000);

            return new List<WorldPoint>().AsEnumerable();
        }
    }
}
