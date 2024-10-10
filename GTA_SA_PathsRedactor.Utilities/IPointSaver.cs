using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTA_SA_PathsRedactor.Core
{
    public interface IPointSaver : IDisposable
    {
        string FileName { get; set; }
        bool CreateBackup { get; set; }
        
        Task SaveAsync(IEnumerable<Models.WorldPoint> points,
                       CancellationToken cancellationToken = default);
    }
}
