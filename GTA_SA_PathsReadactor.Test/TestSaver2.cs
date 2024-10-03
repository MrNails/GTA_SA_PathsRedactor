using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GTA_SA_PathsRedactor.Core;
using GTA_SA_PathsRedactor.Core.Models;

namespace OtherNameSpace.Test
{
    public class TestSaver2 : PointSaver
    {
        public override Task SaveAsync(IEnumerable<GTA_SA_Point> points)
        {
            System.Diagnostics.Debug.WriteLine($"TestSaver2 Invoked SaveAsync(IEnumerable<GTA_SA_Point>) ({DateTime.Now})");
            return SaveAsync(points, CancellationToken.None);
        }

        public override Task SaveAsync(IEnumerable<GTA_SA_Point> points, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"TestSaver2 Invoked SaveAsync(IEnumerable<GTA_SA_Point>, CancellationToken) ({DateTime.Now})");
            return Task.Delay(1000);
        }
    }
}
