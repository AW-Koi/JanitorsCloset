using JanitorsCloset.Carts;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class CartErrandsTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Next_WhenTheCartIsDryAndHasAHome_GoesHome(bool mopSpent)
        {
            var situation = new CartSituation { MessSuitsTool = true, MopSpent = mopSpent, CartDry = true, CartFarFromMess = true, HasHome = true };

            Assert.Equal(CartErrand.ReturnHome, CartErrands.Next(situation));
        }

        [Fact]
        public void Next_WhenTheCartIsDryWithNoHome_JustCleans()
        {
            var situation = new CartSituation { MessSuitsTool = true, MopSpent = true, CartDry = true, CartFarFromMess = true, HasHome = false };

            Assert.Equal(CartErrand.None, CartErrands.Next(situation));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Next_WhenTheCartHasWaterButIsFarFromTheMess_PushesIt(bool mopSpent)
        {
            var situation = new CartSituation { MessSuitsTool = true, MopSpent = mopSpent, CartDry = false, CartFarFromMess = true, HasHome = true };

            Assert.Equal(CartErrand.Push, CartErrands.Next(situation));
        }

        [Fact]
        public void Next_WhenTheMopIsSpentBesideACartWithWater_DunksTheMop()
        {
            var situation = new CartSituation { MessSuitsTool = true, MopSpent = true, CartDry = false, CartFarFromMess = false, HasHome = true };

            Assert.Equal(CartErrand.Dunk, CartErrands.Next(situation));
        }

        [Fact]
        public void Next_WhenTheMopIsWetBesideACartWithWater_JustCleans()
        {
            var situation = new CartSituation { MessSuitsTool = true, MopSpent = false, CartDry = false, CartFarFromMess = false, HasHome = true };

            Assert.Equal(CartErrand.None, CartErrands.Next(situation));
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void Next_WhenTheMessIsNothingTheToolIsFor_JustCleans(bool cartDry, bool cartFarFromMess)
        {
            var situation = new CartSituation { MessSuitsTool = false, MopSpent = true, CartDry = cartDry, CartFarFromMess = cartFarFromMess, HasHome = true };

            Assert.Equal(CartErrand.None, CartErrands.Next(situation));
        }
    }
}
