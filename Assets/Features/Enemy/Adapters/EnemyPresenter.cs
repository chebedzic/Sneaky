using Features.Enemy.Domain;
using UnityEngine;
using VContainer;

namespace Features.Enemy.Adapters
{
    public sealed class EnemyPresenter : MonoBehaviour
    {
        private PathfindingService pathfinder;
        private Features.ClientConfig.ClientConfig clientConfig;

        private IPatrolRoutePort patrolRoute;
        private IPlayerDetectionPort detection;
        private IEnemyBodyPort body;
        private EnemyAiService ai;
        private readonly EnemyState state = new EnemyState();

        [Inject]
        public void Construct(PathfindingService pathfinder, Features.ClientConfig.ClientConfig clientConfig)
        {
            this.pathfinder = pathfinder;
            this.clientConfig = clientConfig;
        }

        private void Update()
        {
            if (ai == null)
            {
                patrolRoute = GetComponent<IPatrolRoutePort>();
                detection = GetComponent<IPlayerDetectionPort>();
                body = GetComponent<IEnemyBodyPort>();
                ai = new EnemyAiService(pathfinder, patrolRoute, detection, clientConfig.EnemyMoveSpeed);
            }

            var velocity = ai.Tick(state, body.Position, Time.deltaTime);
            body.Move(velocity, Time.deltaTime);
        }
    }
}
