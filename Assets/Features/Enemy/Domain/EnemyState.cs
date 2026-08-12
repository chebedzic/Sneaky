using System.Collections.Generic;
using System.Numerics;

namespace Features.Enemy.Domain
{
    public enum EnemyBehaviorState
    {
        Patrol,
        Chase
    }

    public sealed class EnemyState
    {
        public EnemyBehaviorState Behavior = EnemyBehaviorState.Patrol;
        public int PatrolStepIndex;
        public List<Vector3> Path = new List<Vector3>();
        public int PathIndex;
        public Vector3 LastPlannedTarget;
        public bool HasPlanned;
        public float PauseTimer;
    }
}
