using JanitorsCloset.Carts;
using Verse;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class CartParkingTests
    {
        private static readonly IntVec3 NearCell = new IntVec3(10, 0, 10);
        private static readonly IntVec3 FarCell = new IntVec3(14, 0, 10);
        private static readonly IntVec3 OtherFarCell = new IntVec3(10, 0, 14);

        public static TheoryData<CartParkingCandidate> DisqualifiedCells => new TheoryData<CartParkingCandidate>
        {
            new CartParkingCandidate { Cell = NearCell, IsDoor = true },
            new CartParkingCandidate { Cell = NearCell, HasItem = true },
            new CartParkingCandidate { Cell = NearCell, HasBuilding = true },
            new CartParkingCandidate { Cell = NearCell, InStockpile = true },
            new CartParkingCandidate { Cell = NearCell, Unreachable = true },
        };

        [Fact]
        public void Choose_WhenEveryCellIsOpenFloor_PicksTheCellNearestTheCluster()
        {
            var near = new CartParkingCandidate { Cell = NearCell, DistanceToCluster = 1f };
            var far = new CartParkingCandidate { Cell = FarCell, DistanceToCluster = 4f };

            var chosen = CartParking.Choose(new[] { far, near });

            Assert.Equal(NearCell, chosen);
        }

        [Fact]
        public void Choose_WhenAWallSideCellIsFurther_PrefersTheWallSideCell()
        {
            var openFloor = new CartParkingCandidate { Cell = NearCell, DistanceToCluster = 1f };
            var wallSide = new CartParkingCandidate { Cell = FarCell, DistanceToCluster = 3f, AgainstWall = true };

            var chosen = CartParking.Choose(new[] { openFloor, wallSide });

            Assert.Equal(FarCell, chosen);
        }

        [Fact]
        public void Choose_WhenTwoCellsAreEquallyNearTheCluster_PicksTheOneNearerTheCart()
        {
            var awayFromCart = new CartParkingCandidate { Cell = FarCell, DistanceToCluster = 2f, DistanceToCart = 9f };
            var towardCart = new CartParkingCandidate { Cell = OtherFarCell, DistanceToCluster = 2f, DistanceToCart = 3f };

            var chosen = CartParking.Choose(new[] { awayFromCart, towardCart });

            Assert.Equal(OtherFarCell, chosen);
        }

        [Theory]
        [MemberData(nameof(DisqualifiedCells))]
        public void Choose_WhenTheOnlyCellIsDisqualified_FindsNoCell(CartParkingCandidate disqualified)
        {
            var chosen = CartParking.Choose(new[] { disqualified });

            Assert.Null(chosen);
        }

        [Fact]
        public void Choose_WithNoCandidates_FindsNoCell()
        {
            var chosen = CartParking.Choose(new CartParkingCandidate[0]);

            Assert.Null(chosen);
        }
    }
}
