using Features.CameraControl.Domain;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.CameraControl.Adapters
{
    public sealed class PointerDragInputAdapter : MonoBehaviour, IDragInputPort
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "UI";
        [SerializeField] private string pointAction = "Point";
        [SerializeField] private string clickAction = "Click";

        private InputAction point;
        private InputAction click;

        private void Awake()
        {
            var map = inputActions.FindActionMap(actionMapName);
            point = map.FindAction(pointAction);
            click = map.FindAction(clickAction);
        }

        private void OnEnable()
        {
            point.Enable();
            click.Enable();
        }

        private void OnDisable()
        {
            point.Disable();
            click.Disable();
        }

        public bool IsPressed => click.IsPressed();

        public System.Numerics.Vector2 GetPointerPosition()
        {
            Vector2 value = point.ReadValue<Vector2>();
            return new System.Numerics.Vector2(value.x, value.y);
        }
    }
}
