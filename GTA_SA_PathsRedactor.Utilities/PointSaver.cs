using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GTA_SA_PathsRedactor.Core
{
    public abstract class PointSaver : MarshalByRefObject, IDisposable
    {
        public PointSaver(string filePath) 
        {
            FilePath = filePath;
        }

        public virtual string FilePath { get; set; }
        public virtual bool CreateBackup { get; set; }

        public abstract Task SaveAsync(IEnumerable<Models.GTA_SA_Point> points);
        public abstract Task SaveAsync(IEnumerable<Models.GTA_SA_Point> points,
                                       CancellationToken cancellationToken);

        public virtual void Dispose()
        {
            System.GC.SuppressFinalize(this);
        }
    }
}
