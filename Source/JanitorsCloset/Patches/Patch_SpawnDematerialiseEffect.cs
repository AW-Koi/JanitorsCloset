using HarmonyLib;
using JanitorsCloset.Defs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // When a Glittervacuum-equipped pawn finishes cleaning a filth tile, throw a small burst
    // of shimmer particles outward from the cell. Mirrors Patch_SpawnMopMark in hook and
    // gating — same Filth.ThinFilth postfix, same Patch_TrackCurrentJobDriver.Current check —
    // but spawns transient flecks instead of a persistent damp-floor filth.
    //
    // The particle count, scatter, and velocity are randomised per-spawn so repeated cleans
    // don't look mechanically identical. The fleck itself controls fade timing and rotation
    // in Defs/FleckDefs/FleckDefs_Janitor.xml — tune the look there, not here.
    [HarmonyPatch(typeof(Filth), nameof(Filth.ThinFilth))]
    public static class Patch_SpawnDematerialiseEffect
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

            // Don't double-effect on damp-floor marks (they shouldn't normally be cleaned by
            // the Glittervacuum either, but defence in depth never hurts).
            if (__state.OriginalDef == JanitorDefOf.Janitor_MopMark) return;

            var driver = Patch_TrackCurrentJobDriver.Current as JobDriver_CleanFilth;
            if (driver == null) return;
            if (driver.pawn?.equipment?.Primary?.def != JanitorDefOf.Janitor_Glittervacuum) return;

            SpawnBurst(__state.Cell, __state.Map);
        }

        private static void SpawnBurst(IntVec3 cell, Map map)
        {
            var center = cell.ToVector3Shifted();
            int particleCount = Rand.RangeInclusive(3, 5);
            for (int i = 0; i < particleCount; i++)
            {
                var offset = new Vector3(Rand.Range(-0.25f, 0.25f), 0f, Rand.Range(-0.25f, 0.25f));
                var data = FleckMaker.GetDataStatic(
                    center + offset,
                    map,
                    JanitorDefOf.Janitor_DematerialiseShimmer,
                    Rand.Range(0.7f, 1.1f));
                data.velocityAngle = Rand.Range(0f, 360f);
                data.velocitySpeed = Rand.Range(0.4f, 0.8f);
                data.rotation = Rand.Range(0f, 360f);
                data.rotationRate = Rand.Range(-180f, 180f);
                map.flecks.CreateFleck(data);
            }
        }
    }
}
