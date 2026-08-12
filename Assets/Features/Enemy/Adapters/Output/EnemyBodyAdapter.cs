using Features.Enemy.Domain;
using UnityEngine;

namespace Features.Enemy.Adapters
{
    public sealed class EnemyBodyAdapter : MonoBehaviour, IEnemyBodyPort
    {
        public System.Numerics.Vector3 Position
        {
            get
            {
                var position = transform.position;
                return new System.Numerics.Vector3(position.x, position.y, position.z);
            }
        }

        public void Move(System.Numerics.Vector3 velocity, float deltaTime)
        {
            var motion = new Vector3(velocity.X, velocity.Y, velocity.Z) * deltaTime;
            transform.position += motion;

            var facing = new Vector3(velocity.X, 0f, velocity.Z);
            if (facing.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(facing);
            }
        }
    }
}
