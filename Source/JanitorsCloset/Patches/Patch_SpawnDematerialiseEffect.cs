using HarmonyLib;
using JanitorsCloset.Defs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // When a Glittervacuum-equipped pawn finishes cleaning a filth tile, fire a bright
    // field-snap glow plus a cool-tinted thick dust puff at the cell — reads as "reality
    // uncoupled, matter dispersed" rather than fire/welding/smoke. Mirrors Patch_SpawnMopMark
    // in hook and gating (same Filth.ThinFilth postfix, same Patch_TrackCurrentJobDriver
    // .Current check), but uses vanilla FleckMaker helpers so we don't have to ship custom
    // particle textures. Swap in a custom FleckDef later if the visual deserves its own look.
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

        // Glitterworld field-collapse palette: pale cyan-white, slight transparency. Stays
        // clearly synthetic/luminous against any floor and avoids the orange/grey read of
        // smoke or sparks.
        private static readonly Color GlitterTint = new Color(0.7f, 0.95f, 1.0f, 0.85f);

        private static void SpawnBurst(IntVec3 cell, Map map)
        {
            var center = cell.ToVector3Shifted();

            // The snap — one bright pulse, larger than the per-tick during-cleaning pulses
            // so the moment of dematerialisation reads as a discrete event.
            FleckMaker.ThrowLightningGlow(center, map, 1.7f);

            // A couple of tinted thick puffs at small offsets: the un-coupled matter
            // dispersing. Thick keeps them visible briefly; the cyan-white tint reads as
            // field energy rather than combustion.
            int puffCount = Rand.RangeInclusive(2, 3);
            for (int i = 0; i < puffCount; i++)
            {
                var offset = new Vector3(Rand.Range(-0.18f, 0.18f), 0f, Rand.Range(-0.18f, 0.18f));
                FleckMaker.ThrowDustPuffThick(center + offset, map, Rand.Range(0.7f, 1.0f), GlitterTint);
            }
        }
    }
}
