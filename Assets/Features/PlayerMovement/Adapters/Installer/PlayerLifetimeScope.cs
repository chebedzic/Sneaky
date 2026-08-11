using Core.Scopes;
using Features.PlayerMovement.Adapters;
using Features.PlayerMovement.Domain;
using VContainer;
using VContainer.Unity;

namespace Features.PlayerMovement.Installer
{
    public class PlayerLifetimeScope : LifetimeScope
    {
        protected override LifetimeScope FindParent() => Find<LevelLifetimeScope>();

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<PlayerInputAdapter>().As<IMovementInputPort>();
            builder.RegisterComponentInHierarchy<CharacterControllerBodyAdapter>().As<IPlayerBodyPort>();
            builder.Register<PlayerMoverService>(resolver => new PlayerMoverService(resolver.Resolve<Features.ClientConfig.ClientConfig>().PlayerMoveSpeed), Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<PlayerMovementPresenter>();
        }
    }
}
