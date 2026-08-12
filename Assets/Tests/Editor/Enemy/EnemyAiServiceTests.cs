using System.Collections.Generic;
using System.Numerics;
using Features.Enemy.Domain;
using NUnit.Framework;

namespace Features.Enemy.Tests
{
    public class EnemyAiServiceTests
    {
        private sealed class FakePatrolRoute : IPatrolRoutePort
        {
            private readonly List<PatrolStep> steps;

            public FakePatrolRoute(List<PatrolStep> steps)
            {
                this.steps = steps;
            }

            public IReadOnlyList<PatrolStep> GetSteps() => steps;
        }

        private sealed class FakeDetection : IPlayerDetectionPort
        {
            public bool Alerted;
            public Vector3 PlayerPosition;

            public bool IsAlerted() => Alerted;

            public Vector3 GetPlayerPosition() => PlayerPosition;
        }

        [Test]
        public void Tick_StartsInPatrol_MovesTowardFirstWaypoint()
        {
            var pathfinder = new PathfindingService(new FakeWalkabilityGrid(10));
            var patrol = new FakePatrolRoute(new List<PatrolStep> { PatrolStep.MoveTo(new Vector3(5, 0, 0)), PatrolStep.MoveTo(new Vector3(5, 0, 5)) });
            var detection = new FakeDetection { Alerted = false };
            var ai = new EnemyAiService(pathfinder, patrol, detection, moveSpeed: 2f);
            var state = new EnemyState();

            var velocity = ai.Tick(state, new Vector3(0, 0, 0), 0.1f);

            Assert.AreEqual(EnemyBehaviorState.Patrol, state.Behavior);
            Assert.Greater(velocity.Length(), 0f);
        }

        [Test]
        public void Tick_WhenAlerted_SwitchesToChase()
        {
            var pathfinder = new PathfindingService(new FakeWalkabilityGrid(10));
            var patrol = new FakePatrolRoute(new List<PatrolStep> { PatrolStep.MoveTo(new Vector3(5, 0, 0)) });
            var detection = new FakeDetection { Alerted = true, PlayerPosition = new Vector3(3, 0, 3) };
            var ai = new EnemyAiService(pathfinder, patrol, detection, moveSpeed: 2f);
            var state = new EnemyState();

            ai.Tick(state, new Vector3(0, 0, 0), 0.1f);

            Assert.AreEqual(EnemyBehaviorState.Chase, state.Behavior);
        }

        [Test]
        public void Tick_WhenAlertClears_ReturnsToPatrol()
        {
            var pathfinder = new PathfindingService(new FakeWalkabilityGrid(10));
            var patrol = new FakePatrolRoute(new List<PatrolStep> { PatrolStep.MoveTo(new Vector3(5, 0, 0)) });
            var detection = new FakeDetection { Alerted = true, PlayerPosition = new Vector3(3, 0, 3) };
            var ai = new EnemyAiService(pathfinder, patrol, detection, moveSpeed: 2f);
            var state = new EnemyState();

            ai.Tick(state, new Vector3(0, 0, 0), 0.1f);
            detection.Alerted = false;
            ai.Tick(state, new Vector3(0, 0, 0), 0.1f);

            Assert.AreEqual(EnemyBehaviorState.Patrol, state.Behavior);
        }

        [Test]
        public void Tick_ReachingWaypoint_AdvancesToNextPatrolStep()
        {
            var pathfinder = new PathfindingService(new FakeWalkabilityGrid(10));
            var patrol = new FakePatrolRoute(new List<PatrolStep> { PatrolStep.MoveTo(new Vector3(1, 0, 0)), PatrolStep.MoveTo(new Vector3(5, 0, 5)) });
            var detection = new FakeDetection { Alerted = false };
            var ai = new EnemyAiService(pathfinder, patrol, detection, moveSpeed: 2f);
            var state = new EnemyState();
            var position = new Vector3(0, 0, 0);

            for (var i = 0; i < 50 && state.PatrolStepIndex == 0; i++)
            {
                var velocity = ai.Tick(state, position, 0.1f);
                position += velocity * 0.1f;
            }

            Assert.AreEqual(1, state.PatrolStepIndex);
        }

        [Test]
        public void Tick_ReachesPauseStep_HoldsThenAdvancesToNextWaypoint()
        {
            var pathfinder = new PathfindingService(new FakeWalkabilityGrid(10));
            var patrol = new FakePatrolRoute(new List<PatrolStep>
            {
                PatrolStep.MoveTo(new Vector3(1, 0, 0)),
                PatrolStep.Pause(1f),
                PatrolStep.MoveTo(new Vector3(5, 0, 5))
            });
            var detection = new FakeDetection { Alerted = false };
            var ai = new EnemyAiService(pathfinder, patrol, detection, moveSpeed: 2f);
            var state = new EnemyState();
            var position = new Vector3(0, 0, 0);

            for (var i = 0; i < 50 && state.PatrolStepIndex == 0; i++)
            {
                var velocity = ai.Tick(state, position, 0.1f);
                position += velocity * 0.1f;
            }

            Assert.AreEqual(1, state.PatrolStepIndex);
            Assert.Greater(state.PauseTimer, 0f);

            var velocityWhilePaused = ai.Tick(state, position, 0.1f);
            Assert.AreEqual(Vector3.Zero, velocityWhilePaused);

            var velocityAfterPause = Vector3.Zero;
            for (var i = 0; i < 20 && velocityAfterPause == Vector3.Zero; i++)
            {
                velocityAfterPause = ai.Tick(state, position, 0.1f);
            }

            Assert.AreEqual(2, state.PatrolStepIndex);
            Assert.Greater(velocityAfterPause.Length(), 0f);
        }
    }
}
