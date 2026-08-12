using System.Numerics;

namespace Features.Enemy.Domain
{
    public interface IEnemyBodyPort
    {
        Vector3 Position { get; }
        void Move(Vector3 velocity, float deltaTime);
    }
}
