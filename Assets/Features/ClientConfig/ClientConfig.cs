using UnityEngine;

namespace Features.ClientConfig
{
    [CreateAssetMenu(fileName = "ClientConfig", menuName = "Config/Client Config")]
    public sealed class ClientConfig : ScriptableObject
    {
        [SerializeField] private float playerMoveSpeed = 5f;

        public float PlayerMoveSpeed => playerMoveSpeed;
    }
}
