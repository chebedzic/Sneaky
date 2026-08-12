using System;
using System.Collections.Generic;
using System.Numerics;

namespace Features.Enemy.Domain
{
    public sealed class EnemyAiService
    {
        private const float ArrivalTolerance = 0.2f;
        private const float ChaseReplanDistance = 1f;

        private readonly PathfindingService pathfinder;
        private readonly IPatrolRoutePort patrolRoute;
        private readonly IPlayerDetectionPort detection;
        private readonly float moveSpeed;

        public EnemyAiService(PathfindingService pathfinder, IPatrolRoutePort patrolRoute, IPlayerDetectionPort detection, float moveSpeed)
        {
            this.pathfinder = pathfinder;
            this.patrolRoute = patrolRoute;
            this.detection = detection;
            this.moveSpeed = moveSpeed;
        }

        public Vector3 Tick(EnemyState state, Vector3 currentPosition, float deltaTime)
        {
            UpdateBehavior(state);

            return state.Behavior == EnemyBehaviorState.Chase
                ? TickChase(state, currentPosition)
                : TickPatrol(state, currentPosition, deltaTime);
        }

        private void UpdateBehavior(EnemyState state)
        {
            var desired = detection.IsAlerted() ? EnemyBehaviorState.Chase : EnemyBehaviorState.Patrol;
            if (desired != state.Behavior)
            {
                state.Behavior = desired;
                state.HasPlanned = false;
                state.PauseTimer = 0f;
            }
        }

        private Vector3 TickChase(EnemyState state, Vector3 currentPosition)
        {
            var target = detection.GetPlayerPosition();

            if (!state.HasPlanned || state.PathIndex >= state.Path.Count
                || Vector3.Distance(target, state.LastPlannedTarget) > ChaseReplanDistance)
            {
                Replan(state, currentPosition, target);
            }

            return FollowPath(state, currentPosition, onArrival: null);
        }

        private Vector3 TickPatrol(EnemyState state, Vector3 currentPosition, float deltaTime)
        {
            var steps = patrolRoute.GetSteps();
            if (steps.Count == 0)
            {
                return Vector3.Zero;
            }

            var step = steps[state.PatrolStepIndex % steps.Count];

            if (step.Kind == PatrolStepKind.Pause)
            {
                state.PauseTimer -= deltaTime;
                if (state.PauseTimer <= 0f)
                {
                    AdvancePatrolStep(state, steps);
                }

                return Vector3.Zero;
            }

            if (!state.HasPlanned || state.PathIndex >= state.Path.Count)
            {
                Replan(state, currentPosition, step.Waypoint);
            }

            return FollowPath(state, currentPosition, onArrival: () => AdvancePatrolStep(state, steps));
        }

        private void Replan(EnemyState state, Vector3 currentPosition, Vector3 target)
        {
            state.Path = pathfinder.FindPath(currentPosition, target);
            state.PathIndex = 0;
            state.LastPlannedTarget = target;
            state.HasPlanned = true;
        }

        private Vector3 FollowPath(EnemyState state, Vector3 currentPosition, Action onArrival)
        {
            if (state.PathIndex >= state.Path.Count)
            {
                return Vector3.Zero;
            }

            var toWaypoint = state.Path[state.PathIndex] - currentPosition;
            if (toWaypoint.Length() <= ArrivalTolerance)
            {
                state.PathIndex++;

                if (state.PathIndex >= state.Path.Count)
                {
                    onArrival?.Invoke();
                }

                return Vector3.Zero;
            }

            return Vector3.Normalize(toWaypoint) * moveSpeed;
        }

        private static void AdvancePatrolStep(EnemyState state, IReadOnlyList<PatrolStep> steps)
        {
            state.PatrolStepIndex = (state.PatrolStepIndex + 1) % steps.Count;
            var next = steps[state.PatrolStepIndex];
            state.PauseTimer = next.Kind == PatrolStepKind.Pause ? next.PauseDuration : 0f;
            state.HasPlanned = false;
        }
    }
}
