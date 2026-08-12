using System.Collections.Generic;

namespace Features.Enemy.Domain
{
    public interface IPatrolRoutePort
    {
        IReadOnlyList<PatrolStep> GetSteps();
    }
}
