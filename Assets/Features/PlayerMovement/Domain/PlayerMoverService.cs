using System.Numerics;

namespace Features.PlayerMovement.Domain
{
    public sealed class PlayerMoverService
    {
        private readonly float moveSpeed;

        public PlayerMoverService(float moveSpeed)
        {
            this.moveSpeed = moveSpeed;
        }

        public Vector3 ComputeVelocity(Vector2 moveInput)
        {
            var clamped = moveInput.LengthSquared() > 1f ? Vector2.Normalize(moveInput) : moveInput;
            return new Vector3(clamped.X, 0f, clamped.Y) * moveSpeed;
        }
    }
}
