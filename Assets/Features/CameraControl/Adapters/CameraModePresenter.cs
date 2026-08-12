using Features.CameraControl.Domain;
using UnityEngine;
using VContainer;

namespace Features.CameraControl.Adapters
{
    public sealed class CameraModePresenter : MonoBehaviour
    {
        private IDragInputPort dragInput;
        private IMoveIntentInputPort moveInput;
        private ICameraRigPort cameraRig;
        private CameraFollowModeService modeService;

        private bool wasPressed;
        private System.Numerics.Vector2 lastPointerPosition;

        [Inject]
        public void Construct(IDragInputPort dragInput, IMoveIntentInputPort moveInput, ICameraRigPort cameraRig, CameraFollowModeService modeService)
        {
            this.dragInput = dragInput;
            this.moveInput = moveInput;
            this.cameraRig = cameraRig;
            this.modeService = modeService;
        }

        private void Update()
        {
            if (moveInput.HasMoveInput())
            {
                modeService.NotifyPlayerMoved();
            }

            var pointerPosition = dragInput.GetPointerPosition();
            if (dragInput.IsPressed)
            {
                if (!wasPressed)
                {
                    modeService.BeginDrag();
                }
                else
                {
                    modeService.ApplyDragDelta(pointerPosition - lastPointerPosition);
                }
            }
            wasPressed = dragInput.IsPressed;
            lastPointerPosition = pointerPosition;

            cameraRig.SetFreeLook(modeService.Mode == CameraFollowMode.FreeLook);
            cameraRig.Pan(modeService.DragOffset);
        }
    }
}
