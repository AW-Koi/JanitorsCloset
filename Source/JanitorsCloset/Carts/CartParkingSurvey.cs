using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Carts
{
    /// <summary>Reads the map around a cluster of filth into parking candidates for the cart.</summary>
    public static class CartParkingSurvey
    {
        private const float Radius = 4.9f;

        public static IEnumerable<CartParkingCandidate> Around(Pawn pawn, Thing cart, IntVec3 anchor, IntVec3 clusterCentre)
        {
            var map = pawn.Map;
            var room = anchor.GetRoom(map);
            foreach (var cell in GenRadial.RadialCellsAround(anchor, Radius, true))
            {
                if (!cell.InBounds(map) || !cell.Standable(map) || cell.GetRoom(map) != room) continue;
                yield return new CartParkingCandidate
                {
                    Cell = cell,
                    IsDoor = cell.GetDoor(map) != null,
                    HasItem = HasItemOtherThan(cell, map, cart),
                    HasBuilding = cell.GetFirstBuilding(map) != null,
                    InStockpile = map.zoneManager.ZoneAt(cell) is Zone_Stockpile,
                    Unreachable = !pawn.CanReach(cell, PathEndMode.OnCell, Danger.Some),
                    AgainstWall = AgainstWall(cell, map),
                    DistanceToCluster = cell.DistanceTo(clusterCentre),
                    DistanceToCart = cell.DistanceTo(cart.Position),
                };
            }
        }

        private static bool HasItemOtherThan(IntVec3 cell, Map map, Thing cart)
        {
            var things = cell.GetThingList(map);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] != cart && things[i].def.category == ThingCategory.Item) return true;
            }
            return false;
        }

        private static bool AgainstWall(IntVec3 cell, Map map)
        {
            for (int i = 0; i < 4; i++)
            {
                var neighbour = cell + GenAdj.CardinalDirections[i];
                if (neighbour.InBounds(map) && neighbour.GetEdifice(map)?.def.passability == Traversability.Impassable) return true;
            }
            return false;
        }
    }
}
