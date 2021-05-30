using System;
using System.Collections.Generic;
using System.Linq ;
using System.Threading;
using System.Threading.Tasks;
using GTA_SA_PathsRedactor.Core;
using GTA_SA_PathsRedactor.Core.Models;

namespace GTA_SA_PathsRedactor.Test
{
    public class TestLoader : PointLoader
    {
        public override Task<IEnumerable<GTA_SA_Point>> LoadAsync()
        {
            System.Diagnostics.Debug.WriteLine($"TestLoader Invoked LoadAsync() ({DateTime.Now})");
            return LoadAsync(CancellationToken.None);
        }

        public override Task<IEnumerable<GTA_SA_Point>> LoadAsync(CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"TestLoader Invoked LoadAsync(CancellationToken) ({DateTime.Now})");
            return Task.Run(() => new List<GTA_SA_Point>().AsEnumerable());
        }
    }
}
