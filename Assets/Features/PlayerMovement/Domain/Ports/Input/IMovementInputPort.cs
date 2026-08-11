using System.Numerics;

namespace Features.PlayerMovement.Domain
{
    public interface IMovementInputPort
    {
        Vector2 GetMoveInput();
    }
}
