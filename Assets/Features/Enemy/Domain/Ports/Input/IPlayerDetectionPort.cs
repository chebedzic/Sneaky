using System.Numerics;

namespace Features.Enemy.Domain
{
    public interface IPlayerDetectionPort
    {
        bool IsAlerted();
        Vector3 GetPlayerPosition();
    }
}
