using System.Numerics;

namespace Features.PlayerMovement.Domain
{
    public interface IPlayerBodyPort
    {
        void Move(Vector3 planarVelocity, float deltaTime);
    }
}
