using Features.CameraControl.Domain;
using VContainer;
using VContainer.Unity;

namespace Features.CameraControl.Adapters
{
    public static class CameraControlInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<PointerDragInputAdapter>().As<IDragInputPort>();
            builder.RegisterComponentInHierarchy<MoveIntentInputAdapter>().As<IMoveIntentInputPort>();
            builder.RegisterComponentInHierarchy<CinemachineCameraRigAdapter>().As<ICameraRigPort>();
            builder.Register<CameraFollowModeService>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<CameraModePresenter>();
        }
    }
}
