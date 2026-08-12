using System.Numerics;

namespace Features.CameraControl.Domain
{
    public interface ICameraRigPort
    {
        void SetFreeLook(bool active);
        void Pan(Vector2 accumulatedPixelDelta);
    }
}
