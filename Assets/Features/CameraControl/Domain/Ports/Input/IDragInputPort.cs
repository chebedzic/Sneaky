using System.Numerics;

namespace Features.CameraControl.Domain
{
    public interface IDragInputPort
    {
        bool IsPressed { get; }
        Vector2 GetPointerPosition();
    }
}
