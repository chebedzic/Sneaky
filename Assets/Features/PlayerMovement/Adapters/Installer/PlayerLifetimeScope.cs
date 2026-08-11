using Features.PlayerMovement.Adapters;
using Features.PlayerMovement.Domain;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PlayerLifetimeScope : LifetimeScope
{
    [SerializeField] private float moveSpeed = 5f;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<PlayerInputAdapter>().As<IMovementInputPort>();
        builder.RegisterComponentInHierarchy<CharacterControllerBodyAdapter>().As<IPlayerBodyPort>();
        builder.Register<PlayerMover>(Lifetime.Scoped).WithParameter("moveSpeed", moveSpeed);
        builder.RegisterComponentInHierarchy<PlayerMovementPresenter>();
    }
}
