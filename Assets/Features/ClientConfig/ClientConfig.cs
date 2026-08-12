using UnityEngine;

namespace Features.ClientConfig
{
    [CreateAssetMenu(fileName = "ClientConfig", menuName = "Config/Client Config")]
    public sealed class ClientConfig : ScriptableObject
    {
        [SerializeField] private float playerMoveSpeed = 5f;
        [SerializeField] private float enemyMoveSpeed = 3f;

        public float PlayerMoveSpeed => playerMoveSpeed;
        public float EnemyMoveSpeed => enemyMoveSpeed;
    }
}
