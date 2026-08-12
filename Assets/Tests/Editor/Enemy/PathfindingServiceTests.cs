using System.Collections.Generic;
using System.Numerics;
using Features.Enemy.Domain;
using NUnit.Framework;

namespace Features.Enemy.Tests
{
    public class PathfindingServiceTests
    {
        [Test]
        public void FindPath_OpenGrid_ReturnsPathToGoal()
        {
            var grid = new FakeWalkabilityGrid(5);
            var pathfinder = new PathfindingService(grid);

            var path = pathfinder.FindPath(new Vector3(0, 0, 0), new Vector3(4, 0, 4));

            Assert.IsNotEmpty(path);
            Assert.AreEqual(new Vector3(4, 0, 4), path[path.Count - 1]);
        }

        [Test]
        public void FindPath_WallWithGap_OnlyCrossesThroughGap()
        {
            var blocked = new List<(int, int)> { (2, 1), (2, 2), (2, 3), (2, 4) };
            var grid = new FakeWalkabilityGrid(5, blocked);
            var pathfinder = new PathfindingService(grid);

            var path = pathfinder.FindPath(new Vector3(0, 0, 2), new Vector3(4, 0, 2));

            Assert.IsNotEmpty(path);
            foreach (var point in path)
            {
                if (System.Math.Abs(point.X - 2f) < 0.01f)
                {
                    Assert.AreEqual(0f, point.Z, 0.01f, "Should only cross the wall through the unblocked gap");
                }
            }
        }

        [Test]
        public void FindPath_UnreachableGoal_ReturnsEmpty()
        {
            var blocked = new List<(int, int)> { (2, 0), (2, 1), (2, 2), (2, 3), (2, 4) };
            var grid = new FakeWalkabilityGrid(5, blocked);
            var pathfinder = new PathfindingService(grid);

            var path = pathfinder.FindPath(new Vector3(0, 0, 0), new Vector3(4, 0, 0));

            Assert.IsEmpty(path);
        }
    }
}
