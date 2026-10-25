using System.Collections.Generic;
using Verse;

namespace JanitorsCloset.Carts
{
    /// <summary>Picks where a janitor parks the cart: against a wall if possible, then nearest the mess, then nearest the cart.</summary>
    public static class CartParking
    {
        public static IntVec3? Choose(IEnumerable<CartParkingCandidate> candidates)
        {
            CartParkingCandidate best = null;
            foreach (var candidate in candidates)
            {
                if (!candidate.Parkable) continue;
                if (best == null || Beats(candidate, best)) best = candidate;
            }
            return best?.Cell;
        }

        private static bool Beats(CartParkingCandidate a, CartParkingCandidate b)
        {
            if (a.AgainstWall != b.AgainstWall) return a.AgainstWall;
            int byCluster = a.DistanceToCluster.CompareTo(b.DistanceToCluster);
            if (byCluster != 0) return byCluster < 0;
            return a.DistanceToCart < b.DistanceToCart;
        }
    }
}
