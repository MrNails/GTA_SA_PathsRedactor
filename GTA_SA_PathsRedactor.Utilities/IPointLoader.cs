using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTA_SA_PathsRedactor.Core
{
    public interface IPointLoader : IDisposable
    {
        string FileName { get; set; }

        Task<IEnumerable<Models.WorldPoint>> LoadAsync(CancellationToken cancellationToken = default);
    }
}