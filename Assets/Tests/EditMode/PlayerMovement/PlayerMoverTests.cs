using System.Numerics;
using Features.PlayerMovement.Domain;
using NUnit.Framework;

namespace Features.PlayerMovement.Tests
{
    public class PlayerMoverTests
    {
        [Test]
        public void ComputeVelocity_ZeroInput_ReturnsZero()
        {
            var mover = new PlayerMover(moveSpeed: 5f);

            var velocity = mover.ComputeVelocity(Vector2.Zero);

            Assert.AreEqual(Vector3.Zero, velocity);
        }

        [Test]
        public void ComputeVelocity_AxisAlignedInput_ScalesByMoveSpeed()
        {
            var mover = new PlayerMover(moveSpeed: 5f);

            var velocity = mover.ComputeVelocity(new Vector2(1f, 0f));

            Assert.AreEqual(new Vector3(5f, 0f, 0f), velocity);
        }

        [Test]
        public void ComputeVelocity_DiagonalInput_IsClampedToMoveSpeed()
        {
            var mover = new PlayerMover(moveSpeed: 5f);

            var velocity = mover.ComputeVelocity(new Vector2(1f, 1f));

            Assert.AreEqual(5f, velocity.Length(), 0.0001f);
        }

        [Test]
        public void ComputeVelocity_MapsInputYToWorldZ()
        {
            var mover = new PlayerMover(moveSpeed: 5f);

            var velocity = mover.ComputeVelocity(new Vector2(0f, 1f));

            Assert.AreEqual(new Vector3(0f, 0f, 5f), velocity);
        }
    }
}
