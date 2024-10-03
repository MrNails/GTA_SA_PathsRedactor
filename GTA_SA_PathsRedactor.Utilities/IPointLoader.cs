using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTA_SA_PathsRedactor.Core
{
    public interface IPointLoader
    {
        string FileName { get; set; }

        Task<IEnumerable<Models.WorldPoint>> LoadAsync(CancellationToken cancellationToken = default);
    }
}