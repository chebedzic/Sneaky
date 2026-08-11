using Features.PlayerMovement.Domain;
using VContainer;
using VContainer.Unity;

namespace Features.PlayerMovement.Adapters
{
    public static class PlayerMovementInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<PlayerInputAdapter>().As<IMovementInputPort>();
            builder.RegisterComponentInHierarchy<CharacterControllerBodyAdapter>().As<IPlayerBodyPort>();
            builder.Register<PlayerMoverService>(resolver => new PlayerMoverService(resolver.Resolve<Features.ClientConfig.ClientConfig>().PlayerMoveSpeed), Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<PlayerMovementPresenter>();
        }
    }
}
