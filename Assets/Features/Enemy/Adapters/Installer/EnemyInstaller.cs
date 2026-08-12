using Features.Enemy.Domain;
using VContainer;
using VContainer.Unity;

namespace Features.Enemy.Adapters
{
    public static class EnemyInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<GridWalkabilityAdapter>().As<IWalkabilityGridPort>();
            builder.Register<PathfindingService>(Lifetime.Scoped);
        }
    }
}
