using System.Collections.Generic;
using JanitorsCloset.Cleaning;
using Verse;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class FreshRoomStampsTests
    {
        private const int KitchenId = 7;
        private const int ReloadedKitchenId = 2;
        private const int CleanedTick = 60000;
        private static readonly IntVec3 KitchenCell = new IntVec3(10, 0, 12);

        [Fact]
        public void Stamps_AfterReloadWithNewRoomIds_ReportTheReloadedRoomStamped()
        {
            var before = new FreshRoomStamps();
            before.Stamp(KitchenId, KitchenCell, CleanedTick);
            var after = new FreshRoomStamps();

            after.Restore(before.Anchors(), before.Ticks(), _ => ReloadedKitchenId);

            Assert.True(after.IsStamped(ReloadedKitchenId));
            Assert.False(after.IsStamped(KitchenId));
        }

        [Fact]
        public void Restore_WhenAnAnchorHasNoRoom_DropsTheStamp()
        {
            var stamps = new FreshRoomStamps();

            stamps.Restore(new List<IntVec3> { KitchenCell }, new List<int> { CleanedTick }, _ => null);

            Assert.Equal(0, stamps.Count);
        }

        [Fact]
        public void Prune_WhenTheAnchorNowSitsInAnotherRoom_DropsTheStaleStamp()
        {
            var stamps = new FreshRoomStamps();
            stamps.Stamp(KitchenId, KitchenCell, CleanedTick);

            stamps.Prune(_ => ReloadedKitchenId);

            Assert.False(stamps.IsStamped(KitchenId));
        }

        [Fact]
        public void Prune_WhenTheAnchorIsStillInTheRoom_KeepsTheStamp()
        {
            var stamps = new FreshRoomStamps();
            stamps.Stamp(KitchenId, KitchenCell, CleanedTick);

            stamps.Prune(_ => KitchenId);

            Assert.True(stamps.IsStamped(KitchenId));
        }

        [Fact]
        public void Expire_ForAStampedRoom_RemovesOnlyThatRoom()
        {
            var stamps = new FreshRoomStamps();
            stamps.Stamp(KitchenId, KitchenCell, CleanedTick);
            stamps.Stamp(ReloadedKitchenId, new IntVec3(30, 0, 30), CleanedTick);

            stamps.Expire(KitchenId);

            Assert.False(stamps.IsStamped(KitchenId));
            Assert.True(stamps.IsStamped(ReloadedKitchenId));
        }
    }
}
