using System.Collections.Generic;
using HarmonyLib;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // Lays down Janitor_HazmatFoam on whatever tile a Hazmat Sprayer-equipped pawn is
    // working — cleaning filth, clearing pollution, doesn't matter. The foam is what
    // visually "sells" the sprayer at zoom-out scales: motes vanish at distance, but a
    // real filth thing renders through the normal terrain layer and stays put while the
    // pawn works the tile. Auto-decays in a few in-game hours via the def's
    // disappearsInDays so the colony doesn't become permanent foam-marked.
    //
    // Hook is identical to Patch_HazmatSprayerVFX (postfix on JobDriver.DriverTickInterval)
    // — same per-tick run, separate patch so the visual concerns stay split cleanly.
    [HarmonyPatch(typeof(JobDriver), "DriverTickInterval")]
    public static class Patch_SpawnHazmatFoam
    {
        // Re-check the target cell every N ticks. The foam, once placed, lives for hours
        // (disappearsInDays 0.08~0.12 ≈ 2-3 in-game hours), so once spawned we mostly
        // skip the work. The interval just throttles the "is foam already here?" cell
        // scan so we're not hitting it 60×/sec per pawn.
        private const int CheckIntervalTicks = 30;

        public static void Postfix(JobDriver __instance)
        {
            // No Biotech → no sprayer def → nothing to do. Without this guard,
            // (someDef != null) would pass for any equipped weapon and we'd try to
            // spawn a null FilthDef.
            if (JanitorDefOf.Janitor_HazmatSprayer == null) return;
            if (!(__instance is JobDriver_ClearPollution) && !(__instance is JobDriver_CleanFilth)) return;

            var pawn = __instance.pawn;
            if (pawn?.equipment?.Primary?.def != JanitorDefOf.Janitor_HazmatSprayer) return;
            if (pawn.Map == null) return;
            // Don't spawn while pathing — we want the foam to show up at the worked
            // tile, not as a breadcrumb trail across the floor on the way there.
            if (pawn.pather != null && pawn.pather.Moving) return;

            var job = __instance.job;
            if (job == null || !job.targetA.IsValid) return;

            int ticks = Find.TickManager.TicksGame;
            int phase = pawn.thingIDNumber & 0xFF;
            if ((ticks + phase) % CheckIntervalTicks != 0) return;

            var cell = job.targetA.Cell;
            if (!cell.InBounds(pawn.Map)) return;

            // Spray-cone bystander check runs every interval regardless of whether new
            // foam is placed — anyone lingering next to an active sprayer keeps catching
            // overspray, refreshing their "stings" memory.
            DouseAdjacentPawns(pawn);

            // One foam per cell. The filth's default thickness behavior would let
            // TryMakeFilth stack up to 3 layers; we don't want that — a single deposit
            // is the visual we're after and stacking would darken the tile beyond the
            // intended low-alpha look.
            if (HasFoamAt(cell, pawn.Map)) return;

            FilthMaker.TryMakeFilth(cell, pawn.Map, JanitorDefOf.Janitor_HazmatFoam);
        }

        private static void DouseAdjacentPawns(Pawn sprayer)
        {
            if (JanitorDefOf.Janitor_DousedInDeconFoam == null) return;
            var map = sprayer.Map;
            if (map == null) return;

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
