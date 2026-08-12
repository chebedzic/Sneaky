using System.Numerics;

namespace Features.CameraControl.Domain
{
    public enum CameraFollowMode
    {
        Following,
        FreeLook
    }

    public sealed class CameraFollowModeService
    {
        public CameraFollowMode Mode { get; private set; } = CameraFollowMode.Following;
        public Vector2 DragOffset { get; private set; }

        public void BeginDrag()
        {
            Mode = CameraFollowMode.FreeLook;
        }

        public void ApplyDragDelta(Vector2 delta)
        {
            if (Mode == CameraFollowMode.FreeLook)
            {
                DragOffset += delta;
            }
        }

        public void NotifyPlayerMoved()
        {
            if (Mode != CameraFollowMode.FreeLook) return;
            Mode = CameraFollowMode.Following;
            DragOffset = Vector2.Zero;
        }
    }
}
