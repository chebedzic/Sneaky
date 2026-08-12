using System.Numerics;

namespace Features.Enemy.Domain
{
    public enum PatrolStepKind
    {
        Waypoint,
        Pause
    }

    public readonly struct PatrolStep
    {
        public readonly PatrolStepKind Kind;
        public readonly Vector3 Waypoint;
        public readonly float PauseDuration;

        private PatrolStep(PatrolStepKind kind, Vector3 waypoint, float pauseDuration)
        {
            Kind = kind;
            Waypoint = waypoint;
            PauseDuration = pauseDuration;
        }

        public static PatrolStep MoveTo(Vector3 waypoint) => new PatrolStep(PatrolStepKind.Waypoint, waypoint, 0f);

        public static PatrolStep Pause(float duration) => new PatrolStep(PatrolStepKind.Pause, Vector3.Zero, duration);
    }
}
