using Features.PlayerMovement.Domain;
using UnityEngine;

namespace Features.PlayerMovement.Adapters
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterControllerBodyAdapter : MonoBehaviour, IPlayerBodyPort
    {
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundedStickVelocity = -2f;

        private CharacterController controller;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        public void Move(System.Numerics.Vector3 planarVelocity, float deltaTime)
        {
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickVelocity;
            }

            verticalVelocity += gravity * deltaTime;

            var motion = new Vector3(planarVelocity.X, verticalVelocity, planarVelocity.Z);
            controller.Move(motion * deltaTime);

            var facing = new Vector3(planarVelocity.X, 0f, planarVelocity.Z);
            if (facing.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(facing);
            }
        }
    }
}
