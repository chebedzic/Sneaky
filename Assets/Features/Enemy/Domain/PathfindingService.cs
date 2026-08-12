using System.Collections.Generic;
using System.Numerics;

namespace Features.Enemy.Domain
{
    public sealed class PathfindingService
    {
        private readonly IWalkabilityGridPort grid;

        public PathfindingService(IWalkabilityGridPort grid)
        {
            this.grid = grid;
        }

        public List<Vector3> FindPath(Vector3 start, Vector3 goal)
        {
            var startCoord = grid.WorldToGrid(start);
            var goalCoord = grid.WorldToGrid(goal);

            if (!grid.IsWalkable(startCoord) || !grid.IsWalkable(goalCoord))
            {
                return new List<Vector3>();
            }

            var open = new List<GridCoord> { startCoord };
            var cameFrom = new Dictionary<GridCoord, GridCoord>();
            var gCost = new Dictionary<GridCoord, float> { [startCoord] = 0f };

            while (open.Count > 0)
            {
                var current = LowestFCost(open, gCost, goalCoord);

                if (current == goalCoord)
                {
                    return ReconstructPath(cameFrom, current);
                }

                open.Remove(current);

                foreach (var neighbor in grid.GetNeighbors(current))
                {
                    var tentativeG = gCost[current] + StepCost(current, neighbor);
                    if (gCost.TryGetValue(neighbor, out var existingG) && tentativeG >= existingG)
                    {
                        continue;
                    }

                    cameFrom[neighbor] = current;
                    gCost[neighbor] = tentativeG;

                    if (!open.Contains(neighbor))
                    {
                        open.Add(neighbor);
                    }
                }
            }

            return new List<Vector3>();
        }

        private static GridCoord LowestFCost(List<GridCoord> open, Dictionary<GridCoord, float> gCost, GridCoord goal)
        {
            var best = open[0];
            var bestF = gCost[best] + Heuristic(best, goal);

            for (var i = 1; i < open.Count; i++)
            {
                var f = gCost[open[i]] + Heuristic(open[i], goal);
                if (f < bestF)
                {
                    best = open[i];
                    bestF = f;
                }
            }

            return best;
        }

        private static float Heuristic(GridCoord a, GridCoord b)
        {
            var dx = System.Math.Abs(a.X - b.X);
            var dy = System.Math.Abs(a.Y - b.Y);
            return dx + dy;
        }

        private static float StepCost(GridCoord a, GridCoord b)
        {
            return a.X != b.X && a.Y != b.Y ? 1.4142f : 1f;
        }

        private List<Vector3> ReconstructPath(Dictionary<GridCoord, GridCoord> cameFrom, GridCoord goal)
        {
            var coords = new List<GridCoord> { goal };
            var current = goal;

            while (cameFrom.TryGetValue(current, out var previous))
            {
                current = previous;
                coords.Add(current);
            }

            coords.Reverse();

            var path = new List<Vector3>(coords.Count);
            for (var i = 1; i < coords.Count; i++)
            {
                path.Add(grid.GridToWorld(coords[i]));
            }

            if (path.Count == 0)
            {
                path.Add(grid.GridToWorld(goal));
            }

            return path;
        }
    }
}
