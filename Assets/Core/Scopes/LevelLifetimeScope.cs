using Features.CameraControl.Adapters;
using Features.PlayerMovement.Adapters;
using VContainer;
using VContainer.Unity;

namespace Core.Scopes
{
    public class LevelLifetimeScope : LifetimeScope
    {
        protected override LifetimeScope FindParent()
        {
            var parent = Find<SessionLifetimeScope>();
            if (parent != null && parent.Container == null) parent.Build();
            return parent;
        }

        protected override void Configure(IContainerBuilder builder)
        {
            PlayerMovementInstaller.Install(builder);
            CameraControlInstaller.Install(builder);
        }
    }
}
