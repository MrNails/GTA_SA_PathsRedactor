using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTA_SA_PathsRedactor.Core
{
    public abstract class PointLoader : IDisposable
    {
        protected PointLoader()
        {
            FileName = string.Empty;
        }

        public virtual string FileName { get; set; }

        public abstract Task<IEnumerable<Models.WorldPoint>> LoadAsync();
        public abstract Task<IEnumerable<Models.WorldPoint>> LoadAsync(CancellationToken cancellationToken);

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
