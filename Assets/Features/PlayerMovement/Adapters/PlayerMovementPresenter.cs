using Features.PlayerMovement.Domain;
using UnityEngine;
using VContainer;

namespace Features.PlayerMovement.Adapters
{
    public sealed class PlayerMovementPresenter : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private IMovementInputPort _input;
        private IPlayerBodyPort _body;
        private PlayerMover _mover;

        [Inject]
        public void Construct(IMovementInputPort input, IPlayerBodyPort body)
        {
            _input = input;
            _body = body;
            _mover = new PlayerMover(moveSpeed);
        }

        private void Update()
        {
            var velocity = _mover.ComputeVelocity(_input.GetMoveInput());
            _body.Move(velocity, Time.deltaTime);
        }
    }
}
