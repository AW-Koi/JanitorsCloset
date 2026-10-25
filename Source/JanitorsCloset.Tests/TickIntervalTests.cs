using JanitorsCloset.Cleaning;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class TickIntervalTests
    {
        private const int Period = 30;

        [Fact]
        public void TickInterval_OnFirstElapse_Fires()
        {
            var interval = new TickInterval();

            Assert.True(interval.Elapse(1, Period));
        }

        [Theory]
        [InlineData(1, 30)]
        [InlineData(15, 2)]
        [InlineData(4, 8)]
        public void TickInterval_WhenDeltasSumToOnePeriod_FiresOnceMore(int delta, int callsPerPeriod)
        {
            var interval = new TickInterval();
            interval.Elapse(delta, Period);

            int fires = 0;
            for (int i = 0; i < callsPerPeriod; i++)
            {
                if (interval.Elapse(delta, Period)) fires++;
            }

            Assert.Equal(1, fires);
        }

        [Fact]
        public void TickInterval_WhenDeltaIsShortOfPeriod_DoesNotFire()
        {
            var interval = new TickInterval();
            interval.Elapse(1, Period);

            bool fired = interval.Elapse(Period - 1, Period);

            Assert.False(fired);
        }
    }
}
