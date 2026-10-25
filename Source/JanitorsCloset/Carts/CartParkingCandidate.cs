using Verse;

namespace JanitorsCloset.Carts
{
    /// <summary>A cell the cart could park on, with what the map says about it.</summary>
    public class CartParkingCandidate
    {
        public IntVec3 Cell { get; set; }
        public bool IsDoor { get; set; }
        public bool HasItem { get; set; }
        public bool HasBuilding { get; set; }
        public bool InStockpile { get; set; }
        public bool Unreachable { get; set; }
        public bool AgainstWall { get; set; }
        public float DistanceToCluster { get; set; }
        public float DistanceToCart { get; set; }

        public bool Parkable => !IsDoor && !HasItem && !HasBuilding && !InStockpile && !Unreachable;
    }
}
