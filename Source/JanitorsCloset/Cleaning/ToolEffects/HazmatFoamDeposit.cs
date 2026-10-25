using System.Collections.Generic;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Cleaning.ToolEffects
{
    // Lays one decontaminant foam filth on the worked tile and douses pawns standing beside the sprayer.
    public static class HazmatFoamDeposit
    {
        private const int CheckIntervalTicks = 30;

        public static void Tick(CleaningToilSession session, Job job, int delta)
        {
            if (!session.FoamInterval.Elapse(delta, CheckIntervalTicks)) return;

            var pawn = session.Pawn;
            var map = pawn.Map;
            var cell = job.targetA.Cell;
            if (!cell.InBounds(map)) return;

            DouseAdjacentPawns(pawn);
            if (HasFoamAt(cell, map)) return;
            FilthMaker.TryMakeFilth(cell, map, JanitorDefOf.Janitor_HazmatFoam);
        }

        private static void DouseAdjacentPawns(Pawn sprayer)
        {
            if (JanitorDefOf.Janitor_DousedInDeconFoam == null) return;
            var map = sprayer.Map;
            var origin = sprayer.Position;
            for (int i = 0; i < GenAdj.AdjacentCells.Length; i++)
            {
                var cell = origin + GenAdj.AdjacentCells[i];
                if (!cell.InBounds(map)) continue;
                var things = map.thingGrid.ThingsListAtFast(cell);
                for (int j = 0; j < things.Count; j++)
                {
                    var bystander = things[j] as Pawn;
                    if (bystander == null || bystander == sprayer) continue;
                    if (bystander.needs?.mood == null) continue;
                    bystander.needs.mood.thoughts.memories.TryGainMemoryFast(JanitorDefOf.Janitor_DousedInDeconFoam);
                    if (JanitorDefOf.Janitor_AnnoyedByDeconFoam != null)
                    {
                        bystander.needs.mood.thoughts.memories.TryGainMemory(JanitorDefOf.Janitor_AnnoyedByDeconFoam, sprayer);
                    }
                }
            }
        }

        private static bool HasFoamAt(IntVec3 cell, Map map)
        {
            List<Thing> things = map.thingGrid.ThingsListAtFast(cell);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i].def == JanitorDefOf.Janitor_HazmatFoam) return true;
            }
            return false;
        }
    }
}
