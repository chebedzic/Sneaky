using Features.PlayerMovement.Domain;
using UnityEngine;
using VContainer;

namespace Features.PlayerMovement.Adapters
{
    public sealed class PlayerMovementPresenter : MonoBehaviour
    {
        private IMovementInputPort input;
        private IPlayerBodyPort body;
        private PlayerMoverService mover;

        [Inject]
        public void Construct(IMovementInputPort input, IPlayerBodyPort body, PlayerMoverService mover)
        {
            this.input = input;
            this.body = body;
            this.mover = mover;
        }

        private void Update()
        {
            var velocity = mover.ComputeVelocity(input.GetMoveInput());
            body.Move(velocity, Time.deltaTime);
        }
    }
}
