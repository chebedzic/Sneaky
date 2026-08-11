using Features.PlayerMovement.Adapters;
using Features.PlayerMovement.Domain;
using VContainer;
using VContainer.Unity;

public class PlayerLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<PlayerInputAdapter>().As<IMovementInputPort>();
        builder.RegisterComponentInHierarchy<CharacterControllerBodyAdapter>().As<IPlayerBodyPort>();
        builder.RegisterComponentInHierarchy<PlayerMovementPresenter>();
    }
}
