using System.Collections.Generic;
using System.Numerics;
using Features.Enemy.Domain;

namespace Features.Enemy.Tests
{
    public sealed class FakeWalkabilityGrid : IWalkabilityGridPort
    {
        private readonly bool[,] cells;
        private readonly int size;

        public FakeWalkabilityGrid(int size, IEnumerable<(int x, int y)> blocked = null)
        {
            this.size = size;
            cells = new bool[size, size];

            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    cells[x, y] = true;
                }
            }

            if (blocked == null)
            {
                return;
            }

            foreach (var (x, y) in blocked)
            {
                cells[x, y] = false;
            }
        }

        public bool IsWalkable(GridCoord coord)
        {
            if (coord.X < 0 || coord.X >= size || coord.Y < 0 || coord.Y >= size)
            {
                return false;
            }

            return cells[coord.X, coord.Y];
        }

        public GridCoord WorldToGrid(Vector3 worldPosition) => new GridCoord((int)worldPosition.X, (int)worldPosition.Z);

        public Vector3 GridToWorld(GridCoord coord) => new Vector3(coord.X, 0f, coord.Y);

        public IEnumerable<GridCoord> GetNeighbors(GridCoord coord)
        {
            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    var neighbor = new GridCoord(coord.X + dx, coord.Y + dy);
                    if (!IsWalkable(neighbor))
                    {
                        continue;
                    }

                    if (dx != 0 && dy != 0)
                    {
                        var sideA = new GridCoord(coord.X + dx, coord.Y);
                        var sideB = new GridCoord(coord.X, coord.Y + dy);
                        if (!IsWalkable(sideA) || !IsWalkable(sideB))
                        {
                            continue;
                        }
                    }

                    yield return neighbor;
                }
            }
        }
    }
}
