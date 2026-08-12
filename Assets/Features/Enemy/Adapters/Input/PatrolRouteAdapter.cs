using System.Collections.Generic;
using Features.Enemy.Domain;
using UnityEngine;

namespace Features.Enemy.Adapters
{
    public sealed class PatrolRouteAdapter : MonoBehaviour, IPatrolRoutePort
    {
        [System.Serializable]
        private struct StepDefinition
        {
            public PatrolStepKind kind;
            public Transform waypoint;
            public float pauseSeconds;
        }

        [SerializeField] private StepDefinition[] steps = System.Array.Empty<StepDefinition>();

        private List<PatrolStep> cachedSteps;

        public IReadOnlyList<PatrolStep> GetSteps()
        {
            if (cachedSteps == null)
            {
                cachedSteps = new List<PatrolStep>(steps.Length);
                foreach (var step in steps)
                {
                    if (step.kind == PatrolStepKind.Pause)
                    {
                        cachedSteps.Add(PatrolStep.Pause(step.pauseSeconds));
                    }
                    else if (step.waypoint != null)
                    {
                        var position = step.waypoint.position;
                        cachedSteps.Add(PatrolStep.MoveTo(new System.Numerics.Vector3(position.x, position.y, position.z)));
                    }
                }
            }

            return cachedSteps;
        }

        private void OnDrawGizmos()
        {
            if (steps == null || steps.Length == 0)
            {
                return;
            }

            Transform first = null;
            Transform previous = null;

            foreach (var step in steps)
            {
                if (step.kind == PatrolStepKind.Pause)
                {
                    if (previous != null)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireCube(previous.position + Vector3.up * 0.6f, Vector3.one * 0.3f);
                    }

                    continue;
                }

                if (step.waypoint == null)
                {
                    continue;
                }

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(step.waypoint.position, 0.3f);
                if (previous != null)
                {
                    Gizmos.DrawLine(previous.position, step.waypoint.position);
                }

                first = first != null ? first : step.waypoint;
                previous = step.waypoint;
            }

            if (first != null && previous != null && previous != first)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(previous.position, first.position);
            }
        }
    }
}
