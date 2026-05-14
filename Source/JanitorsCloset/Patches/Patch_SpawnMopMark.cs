using System.Collections.Generic;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    // When a mop-equipped pawn finishes cleaning a filth tile, drop a Janitor_MopMark on that cell.
    // Hooks Filth.ThinFilth, which is the cleaning toil's per-thickness reduction call. We act only when
    // the filth was destroyed (thickness hit 0), and only when the active cleaning JobDriver
    // belongs to a pawn carrying the mop. Cell + map are captured in the prefix because the
    // filth's Map goes null on Destroy.
    [HarmonyPatch(typeof(Filth), nameof(Filth.ThinFilth))]
    public static class Patch_SpawnMopMark
    {
        public class State
        {
            public Map Map;
            public IntVec3 Cell;
            public ThingDef OriginalDef;
        }

        public static void Prefix(Filth __instance, out State __state)
        {
            __state = new State
            {
                Map = __instance.Map,
                Cell = __instance.Position,
                OriginalDef = __instance.def,
            };
        }

        public static void Postfix(Filth __instance, State __state)
        {
            if (!__instance.Destroyed) return;
            if (__state.Map == null) return;

            // Defensive recursion guard against spawning a mop mark when the cleaned filth
            // was itself a mop mark.
            if (__state.OriginalDef == JanitorDefOf.Janitor_MopMark) return;

            // Filth can be removed by rain/age/fire too — only spawn marks for cleaning work
            // performed by a mop-equipped cleaner.
            var driver = Patch_TrackCurrentJobDriver.Current as JobDriver_CleanFilth;
            if (driver == null) return;
            if (driver.pawn?.equipment?.Primary?.def != JanitorDefOf.Janitor_Mop) return;

            // Soft floors (carpet, wool, leathery) absorb the mop's water rather than
            // pooling it, so we skip the visible damp patch
            var terrain = __state.Map.terrainGrid.TerrainAt(__state.Cell);
            if (SoftFloorResolver.IsSoftFloor(terrain)) return;

            // One mop mark per tile per cleaning pass — if the cell already has a mark
            // (because the cell had multiple filths and we just cleaned a second one),
            // don't spawn a redundant stacked mark.
            var things = __state.Cell.GetThingList(__state.Map);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i].def == JanitorDefOf.Janitor_MopMark) return;
            }

            FilthMaker.TryMakeFilth(__state.Cell, __state.Map, JanitorDefOf.Janitor_MopMark);

            SplashAdjacentPawns(driver.pawn, __state.Map);
        }

        // Each completed mop swing flicks water onto anyone standing within one tile of
        // the mopper. stackLimit=1 on the thought means repeated splashes refresh the
        // same memory instead of stacking — the bystander keeps the "eww" mood as long
        // as they linger next to the work.
        private static void SplashAdjacentPawns(Pawn mopper, Map map)
        {
            if (mopper == null || map == null) return;
            if (JanitorDefOf.Janitor_SplashedByMop == null) return;

            var origin = mopper.Position;
            for (int i = 0; i < GenAdj.AdjacentCells.Length; i++)
            {
                var cell = origin + GenAdj.AdjacentCells[i];
                if (!cell.InBounds(map)) continue;
                var things = map.thingGrid.ThingsListAtFast(cell);
                for (int j = 0; j < things.Count; j++)
                {
                    var bystander = things[j] as Pawn;
                    if (bystander == null || bystander == mopper) continue;
                    if (bystander.needs?.mood == null) continue;
                    bystander.needs.mood.thoughts.memories.TryGainMemoryFast(JanitorDefOf.Janitor_SplashedByMop);
                }
            }
        }
    }
}
