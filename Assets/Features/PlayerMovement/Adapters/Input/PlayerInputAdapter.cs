using Features.PlayerMovement.Domain;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.PlayerMovement.Adapters
{
    public sealed class PlayerInputAdapter : MonoBehaviour, IMovementInputPort
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

        public System.Numerics.Vector2 GetMoveInput()
        {
            Vector2 value = moveAction.ReadValue<Vector2>();
            return new System.Numerics.Vector2(value.x, value.y);
        }
    }
}
