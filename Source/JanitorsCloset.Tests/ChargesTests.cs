using JanitorsCloset.Carts;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class ChargesTests
    {
        private const int Capacity = 4;

        [Fact]
        public void TryUse_WithChargesLeft_UsesOne()
        {
            var charges = new Charges(Capacity);

            Assert.True(charges.TryUse());
            Assert.Equal(Capacity - 1, charges.Remaining);
        }

        [Fact]
        public void TryUse_WhenEmpty_FailsAndStaysEmpty()
        {
            var charges = new Charges(Capacity);
            for (int i = 0; i < Capacity; i++) charges.TryUse();

            Assert.False(charges.TryUse());
            Assert.Equal(0, charges.Remaining);
            Assert.True(charges.Empty);
        }

        [Fact]
        public void Refill_AfterUse_RestoresTheCapacity()
        {
            var charges = new Charges(Capacity);
            charges.TryUse();
            charges.TryUse();

            charges.Refill();

            Assert.Equal(Capacity, charges.Remaining);
        }
    }
}
