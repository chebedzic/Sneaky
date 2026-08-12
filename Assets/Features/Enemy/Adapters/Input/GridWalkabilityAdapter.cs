using System.Collections.Generic;
using Features.Enemy.Domain;
using UnityEngine;

namespace Features.Enemy.Adapters
{
    public sealed class GridWalkabilityAdapter : MonoBehaviour, IWalkabilityGridPort
    {
        [SerializeField] private Vector3 center = Vector3.zero;
        [SerializeField] private Vector3 size = new Vector3(10f, 1f, 10f);
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private LayerMask obstacleMask;

        private bool[,] walkable;
        private int columns;
        private int rows;

        private void Awake()
        {
            Bake();
        }

        private void Bake()
        {
            columns = Mathf.Max(1, Mathf.RoundToInt(size.x / cellSize));
            rows = Mathf.Max(1, Mathf.RoundToInt(size.z / cellSize));
            walkable = new bool[columns, rows];

            var origin = Origin();
            var halfExtents = new Vector3(cellSize, size.y, cellSize) * 0.5f;

            for (var x = 0; x < columns; x++)
            {
                for (var y = 0; y < rows; y++)
                {
                    var cellCenter = origin + new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);
                    walkable[x, y] = !Physics.CheckBox(cellCenter, halfExtents, Quaternion.identity, obstacleMask);
                }
            }
        }

        private Vector3 Origin()
        {
            return center - new Vector3(size.x, 0f, size.z) * 0.5f;
        }

        public bool IsWalkable(GridCoord coord)
        {
            if (coord.X < 0 || coord.X >= columns || coord.Y < 0 || coord.Y >= rows)
            {
                return false;
            }

            return walkable[coord.X, coord.Y];
        }

        public GridCoord WorldToGrid(System.Numerics.Vector3 worldPosition)
        {
            var origin = Origin();
            var x = Mathf.FloorToInt((worldPosition.X - origin.x) / cellSize);
            var y = Mathf.FloorToInt((worldPosition.Z - origin.z) / cellSize);
            return new GridCoord(Mathf.Clamp(x, 0, columns - 1), Mathf.Clamp(y, 0, rows - 1));
        }

        public System.Numerics.Vector3 GridToWorld(GridCoord coord)
        {
            var origin = Origin();
            var world = origin + new Vector3((coord.X + 0.5f) * cellSize, 0f, (coord.Y + 0.5f) * cellSize);
            return new System.Numerics.Vector3(world.x, world.y, world.z);
        }

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
