using Features.CameraControl.Domain;
using Unity.Cinemachine;
using UnityEngine;

namespace Features.CameraControl.Adapters
{
    public sealed class CinemachineCameraRigAdapter : MonoBehaviour, ICameraRigPort
    {
        [SerializeField] private CinemachineCamera followCamera;
        [SerializeField] private CinemachineCamera dragCamera;
        [SerializeField] private float dragSensitivity = 0.02f;
        [SerializeField] private int activePriority = 20;

        private Vector3 dragBasePosition;
        private bool wasFreeLook;

        public void SetFreeLook(bool active)
        {
            if (active && !wasFreeLook)
            {
                dragBasePosition = followCamera.transform.position;
                dragCamera.transform.rotation = followCamera.transform.rotation;
            }

            dragCamera.Priority = active ? activePriority : 0;
            wasFreeLook = active;
        }

        public void Pan(System.Numerics.Vector2 accumulatedPixelDelta)
        {
            if (!wasFreeLook) return;

            var rot = dragCamera.transform.rotation;
            var offset = rot * Vector3.right * (-accumulatedPixelDelta.X * dragSensitivity)
                + rot * Vector3.up * (-accumulatedPixelDelta.Y * dragSensitivity);
            dragCamera.transform.position = dragBasePosition + offset;
        }
    }
}
