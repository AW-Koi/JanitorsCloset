using HarmonyLib;
using JanitorsCloset.Defs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // When a Glittervacuum-equipped pawn finishes cleaning a filth tile, spawn a small burst
    // of vanilla microsparks and a thin puff of smoke at the cell. Mirrors Patch_SpawnMopMark
    // in hook and gating — same Filth.ThinFilth postfix, same Patch_TrackCurrentJobDriver
    // .Current check — but uses vanilla FleckMaker helpers instead of a custom FleckDef so we
    // don't have to ship our own particle textures. Swap in a custom FleckDef later when art
    // is ready and the dematerialise visual deserves its own look.
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

        private static void SpawnBurst(IntVec3 cell, Map map)
        {
            var center = cell.ToVector3Shifted();

            // Microsparks at random offsets — reads as "matter coming apart at the seams."
            int sparkCount = Rand.RangeInclusive(3, 5);
            for (int i = 0; i < sparkCount; i++)
            {
                var offset = new Vector3(Rand.Range(-0.25f, 0.25f), 0f, Rand.Range(-0.25f, 0.25f));
                FleckMaker.ThrowMicroSparks(center + offset, map);
            }

            // A small puff of smoke under the sparks for a "the thing was here" trace that
            // dissipates with them.
            FleckMaker.ThrowSmoke(center, map, 0.6f);
        }
    }
}
