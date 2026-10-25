using JanitorsCloset.Carts;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class CartIdleRuleTests
    {
        private const int FourHours = 4 * 2500;

        [Theory]
        [InlineData(FourHours - 1, false)]
        [InlineData(FourHours, true)]
        public void ShouldGoHome_AwayFromHome_IsTrueFromFourHoursIdle(int ticksIdle, bool expected)
        {
            Assert.Equal(expected, CartIdleRule.ShouldGoHome(ticksIdle, atHome: false));
        }

        [Fact]
        public void ShouldGoHome_WhenAlreadyHome_IsFalse()
        {
            Assert.False(CartIdleRule.ShouldGoHome(FourHours * 3, atHome: true));
        }
    }
}
