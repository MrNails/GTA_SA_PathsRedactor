using GTA_SA_PathsRedactor.Core.Models;

namespace GTA_SA_PathsRedactor.Models.Dto;

public sealed class InsertPointDto(WorldPoint point, int indexToInsert)
{
    public WorldPoint Point { get; } = point;
    public int IndexToInsert { get; } = indexToInsert;
}