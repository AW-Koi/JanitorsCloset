using HarmonyLib;
using JanitorsCloset.Defs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // When a Glittervacuum-equipped pawn finishes cleaning a filth tile, drift a few
    // cyan-tinted thick puffs upward at the cell — the un-coupled matter floating away.
    // Deliberately subtle: no glow flash, no sparks, just the puffs. The "during cleaning"
    // pulse glow (Patch_GlittervacuumCleaningGlow) already ends naturally when the job
    // does, which provides the energy half of the visual. Mirrors Patch_SpawnMopMark in
    // hook and gating (Filth.ThinFilth postfix + Patch_TrackCurrentJobDriver.Current check).
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
            if (__state.OriginalDef == JanitorDefOf.Janitor_MopMark) return;

            var driver = Patch_TrackCurrentJobDriver.Current as JobDriver_CleanFilth;
            if (driver == null) return;
            if (driver.pawn?.equipment?.Primary?.def != JanitorDefOf.Janitor_Glittervacuum) return;

            SpawnBurst(__state.Cell, __state.Map);
        }

        // Glitterworld field-collapse palette: pale cyan-white, low alpha so the puffs
        // dissolve rather than smother. Same colour family as Janitor_GlittervacuumPulse
        // so the during/end visuals read as the same effect winding down.
        private static readonly Color GlitterTint = new Color(0.7f, 0.95f, 1.0f, 0.55f);

        private static void SpawnBurst(IntVec3 cell, Map map)
        {
            var center = cell.ToVector3Shifted();

            // 2-3 small tinted puffs drifting up at slight offsets: the un-coupled matter
            // floating away. Sizes are deliberately small so the moment reads as "it
            // dissolved" rather than "something exploded."
            int puffCount = Rand.RangeInclusive(2, 3);
            for (int i = 0; i < puffCount; i++)
            {
                var offset = new Vector3(Rand.Range(-0.15f, 0.15f), 0f, Rand.Range(-0.15f, 0.15f));
                FleckMaker.ThrowDustPuffThick(center + offset, map, Rand.Range(0.45f, 0.7f), GlitterTint);
            }
        }
    }
}
