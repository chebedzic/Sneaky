using Features.Enemy.Domain;
using UnityEngine;

namespace Features.Enemy.Adapters
{
    public sealed class MockPlayerDetectionAdapter : MonoBehaviour, IPlayerDetectionPort
    {
        [SerializeField] private bool isAlerted;
        [SerializeField] private Transform player;

        public bool IsAlerted() => isAlerted;

        public System.Numerics.Vector3 GetPlayerPosition()
        {
            var position = player != null ? player.position : Vector3.zero;
            return new System.Numerics.Vector3(position.x, position.y, position.z);
        }
    }
}
