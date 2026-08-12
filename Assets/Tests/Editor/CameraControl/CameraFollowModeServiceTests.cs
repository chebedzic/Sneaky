using System.Numerics;
using Features.CameraControl.Domain;
using NUnit.Framework;

namespace Features.CameraControl.Tests
{
    public class CameraFollowModeServiceTests
    {
        [Test]
        public void InitialMode_IsFollowing()
        {
            var service = new CameraFollowModeService();

            Assert.AreEqual(CameraFollowMode.Following, service.Mode);
            Assert.AreEqual(Vector2.Zero, service.DragOffset);
        }

        [Test]
        public void BeginDrag_SwitchesToFreeLook()
        {
            var service = new CameraFollowModeService();

            service.BeginDrag();

            Assert.AreEqual(CameraFollowMode.FreeLook, service.Mode);
        }

        [Test]
        public void ApplyDragDelta_WhileFreeLook_AccumulatesOffset()
        {
            var service = new CameraFollowModeService();
            service.BeginDrag();

            service.ApplyDragDelta(new Vector2(3f, -2f));
            service.ApplyDragDelta(new Vector2(1f, 1f));

            Assert.AreEqual(new Vector2(4f, -1f), service.DragOffset);
        }

        [Test]
        public void ApplyDragDelta_WhileFollowing_IsIgnored()
        {
            var service = new CameraFollowModeService();

            service.ApplyDragDelta(new Vector2(3f, -2f));

            Assert.AreEqual(Vector2.Zero, service.DragOffset);
        }

        [Test]
        public void NotifyPlayerMoved_WhileFreeLook_SnapsBackToFollowingAndResetsOffset()
        {
            var service = new CameraFollowModeService();
            service.BeginDrag();
            service.ApplyDragDelta(new Vector2(5f, 5f));

            service.NotifyPlayerMoved();

            Assert.AreEqual(CameraFollowMode.Following, service.Mode);
            Assert.AreEqual(Vector2.Zero, service.DragOffset);
        }

        [Test]
        public void NotifyPlayerMoved_WhileAlreadyFollowing_IsNoOp()
        {
            var service = new CameraFollowModeService();

            service.NotifyPlayerMoved();

            Assert.AreEqual(CameraFollowMode.Following, service.Mode);
        }
    }
}
