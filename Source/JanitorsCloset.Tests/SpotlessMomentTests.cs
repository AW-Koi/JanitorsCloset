using System.Collections.Generic;
using JanitorsCloset.Cleaning;
using Verse;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class SpotlessMomentTests
    {
        private const int JanitorId = 42;
        private const int Now = 100000;

        [Theory]
        [InlineData(6, 1)]
        [InlineData(36, 3)]
        [InlineData(120, 10)]
        [InlineData(240, 10)]
        public void ShimmerCount_ForAnIndoorRoom_ScalesWithSizeUpToTheCap(int cellCount, int expected)
        {
            Assert.Equal(expected, SpotlessMoment.ShimmerCount(cellCount));
        }

        [Fact]
        public void ShimmerCount_ForAVeryLargeRoom_IsZero()
        {
            Assert.Equal(0, SpotlessMoment.ShimmerCount(241));
        }

        [Fact]
        public void ShimmerQueue_BeforeAGlintIsDue_ReleasesNothing()
        {
            var queue = new ShimmerQueue();
            queue.Enqueue(new IntVec3(3, 0, 4), Now + 30);
            var released = new List<IntVec3>();

            queue.TakeDue(Now + 29, released);

            Assert.Empty(released);
            Assert.Equal(1, queue.Count);
        }

        [Fact]
        public void ShimmerQueue_OnceGlintsAreDue_ReleasesOnlyThose()
        {
            var early = new IntVec3(3, 0, 4);
            var late = new IntVec3(5, 0, 6);
            var queue = new ShimmerQueue();
            queue.Enqueue(late, Now + 90);
            queue.Enqueue(early, Now + 10);
            var released = new List<IntVec3>();

            queue.TakeDue(Now + 10, released);

            var cell = Assert.Single(released);
            Assert.Equal(early, cell);
            Assert.Equal(1, queue.Count);
        }

        [Fact]
        public void SpotlessCooldown_ForAFirstMoment_AllowsIt()
        {
            var cooldown = new SpotlessCooldown();

            Assert.True(cooldown.TryStart(JanitorId, Now));
        }

        [Fact]
        public void SpotlessCooldown_WithinTheCooldown_BlocksTheSameJanitor()
        {
            var cooldown = new SpotlessCooldown();
            cooldown.TryStart(JanitorId, Now);

            Assert.False(cooldown.TryStart(JanitorId, Now + SpotlessCooldown.CooldownTicks - 1));
        }

        [Fact]
        public void SpotlessCooldown_AfterTheCooldown_AllowsTheSameJanitorAgain()
        {
            var cooldown = new SpotlessCooldown();
            cooldown.TryStart(JanitorId, Now);

            Assert.True(cooldown.TryStart(JanitorId, Now + SpotlessCooldown.CooldownTicks));
        }

        [Fact]
        public void SpotlessCooldown_WithinOneJanitorsCooldown_AllowsAnotherJanitor()
        {
            var cooldown = new SpotlessCooldown();
            cooldown.TryStart(JanitorId, Now);

            Assert.True(cooldown.TryStart(JanitorId + 1, Now + 1));
        }
    }
}
