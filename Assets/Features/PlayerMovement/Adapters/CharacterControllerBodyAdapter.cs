using Features.PlayerMovement.Domain;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace Features.PlayerMovement.Adapters
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterControllerBodyAdapter : MonoBehaviour, IPlayerBodyPort
    {
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundedStickVelocity = -2f;

        private CharacterController _controller;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        public void Move(Vector3 planarVelocity, float deltaTime)
        {
            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = groundedStickVelocity;
            }

            _verticalVelocity += gravity * deltaTime;

            var motion = new UnityEngine.Vector3(planarVelocity.X, _verticalVelocity, planarVelocity.Z);
            _controller.Move(motion * deltaTime);

            var facing = new UnityEngine.Vector3(planarVelocity.X, 0f, planarVelocity.Z);
            if (facing.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(facing);
            }
        }
    }
}
