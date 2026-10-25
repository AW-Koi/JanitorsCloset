using JanitorsCloset.Carts;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class CartPushRuleTests
    {
        [Fact]
        public void NeedsPush_WhenTheCartIsInAnotherRoom_IsTrue()
        {
            Assert.True(CartPushRule.NeedsPush(cartInClusterRoom: false, distanceToCluster: 3f));
        }

        [Theory]
        [InlineData(4f, false)]
        [InlineData(12f, false)]
        [InlineData(12.5f, true)]
        public void NeedsPush_WhenTheCartIsInTheClusterRoom_IsTrueOnlyBeyondTwelveCells(float distanceToCluster, bool expected)
        {
            Assert.Equal(expected, CartPushRule.NeedsPush(cartInClusterRoom: true, distanceToCluster));
        }
    }
}
