using JanitorsCloset.Defs;
using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning.ToolEffects
{
    // A damp mark on each mopped tile, and a splash for anyone standing beside the mopper.
    public static class MopMarks
    {
        public static void Leave(IntVec3 cell, Map map, Pawn mopper)
        {
            // Soft floors soak the water up.
            if (SoftFloorResolver.IsSoftFloor(map.terrainGrid.TerrainAt(cell))) return;
            if (HasMarkAt(cell, map)) return;

            FilthMaker.TryMakeFilth(cell, map, JanitorDefOf.Janitor_MopMark);
            SplashAdjacentPawns(mopper, map);
        }

        private static bool HasMarkAt(IntVec3 cell, Map map)
        {
            var things = map.thingGrid.ThingsListAtFast(cell);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i].def == JanitorDefOf.Janitor_MopMark) return true;
            }
            return false;
        }

        private static void SplashAdjacentPawns(Pawn mopper, Map map)
        {
            var origin = mopper.Position;
            for (int i = 0; i < GenAdj.AdjacentCells.Length; i++)
            {
                var cell = origin + GenAdj.AdjacentCells[i];
                if (!cell.InBounds(map)) continue;
                var things = map.thingGrid.ThingsListAtFast(cell);
                for (int j = 0; j < things.Count; j++)
                {
                    if (!(things[j] is Pawn bystander) || bystander == mopper) continue;
                    var memories = bystander.needs?.mood?.thoughts.memories;
                    if (memories == null) continue;
                    memories.TryGainMemoryFast(JanitorDefOf.Janitor_SplashedByMop);
                    memories.TryGainMemory(JanitorDefOf.Janitor_AnnoyedByMopSplash, mopper);
                }
            }
        }
    }
}
