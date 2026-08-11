using Features.PlayerMovement.Domain;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = System.Numerics.Vector2;

namespace Features.PlayerMovement.Adapters
{
    public sealed class PlayerInputAdapter : MonoBehaviour, IMovementInputPort
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string moveActionName = "Move";

        private InputAction _moveAction;

        private void Awake()
        {
            _moveAction = inputActions.FindActionMap(actionMapName).FindAction(moveActionName);
        }

        private void OnEnable() => _moveAction.Enable();

        private void OnDisable() => _moveAction.Disable();

        public Vector2 GetMoveInput()
        {
            var value = _moveAction.ReadValue<UnityEngine.Vector2>();
            return new Vector2(value.x, value.y);
        }
    }
}
