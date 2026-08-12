using Features.CameraControl.Domain;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.CameraControl.Adapters
{
    public sealed class MoveIntentInputAdapter : MonoBehaviour, IMoveIntentInputPort
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string moveActionName = "Move";

        private InputAction moveAction;

        private void Awake()
        {
            moveAction = inputActions.FindActionMap(actionMapName).FindAction(moveActionName);
        }

        private void OnEnable() => moveAction.Enable();

        private void OnDisable() => moveAction.Disable();

        public bool HasMoveInput() => moveAction.ReadValue<Vector2>().sqrMagnitude > 0.0001f;
    }
}
