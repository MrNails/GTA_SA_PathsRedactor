using System.Collections.Generic;
using System.Threading.Tasks;

namespace GTA_SA_PathsRedactor.Core
{
    public interface IPointSaverLoader
    {
        string FriendlyName { get; }
        string Description { get; }
        string CreatedBy { get; }
        bool CreateBackup { get; set; }

        abstract Task SaveAsync(IEnumerable<Models.GTA_SA_Point> points);

        abstract Task<IEnumerable<Models.GTA_SA_Point>> LoadAsync();
    }
}
