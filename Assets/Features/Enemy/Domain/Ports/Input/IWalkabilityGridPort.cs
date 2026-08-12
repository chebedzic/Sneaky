using System.Collections.Generic;
using System.Numerics;

namespace Features.Enemy.Domain
{
    public interface IWalkabilityGridPort
    {
        bool IsWalkable(GridCoord coord);
        GridCoord WorldToGrid(Vector3 worldPosition);
        Vector3 GridToWorld(GridCoord coord);
        IEnumerable<GridCoord> GetNeighbors(GridCoord coord);
    }
}
