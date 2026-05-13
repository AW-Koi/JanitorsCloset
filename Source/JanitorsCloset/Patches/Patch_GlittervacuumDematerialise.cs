using System.Collections.Generic;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // When a Glittervacuum-equipped pawn finishes cleaning a filth tile:
    //   1. Drift a few cyan-tinted thick puffs upward at the cell — the un-coupled matter
    //      floating away. No glow flash, no sparks; the during-cleaning pulse glow
    //      (Patch_GlittervacuumCleaningGlow) already supplies the energy half.
    //   2. Dematerialise any *other* filth still on the same cell. Flavor: the field
    //      uncouples the whole tile, not just the one piece of dirt the toil was tracking.
    //      A single ThinFilth-to-destroy thus clears a whole co-located stack for free.
    //
    // Mirrors Patch_SpawnMopMark in hook and gating (Filth.ThinFilth postfix +
    // Patch_TrackCurrentJobDriver.Current check).
    [HarmonyPatch(typeof(Filth), nameof(Filth.ThinFilth))]
    public static class Patch_GlittervacuumDematerialise
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
            var weaponDef = driver.pawn?.equipment?.Primary?.def;
            if (weaponDef == null) return;
            var ext = weaponDef.GetModExtension<CleaningToolExtension>();
            if (ext == null || !ext.clearsFilthStack) return;

            SpawnBurst(__state.Cell, __state.Map);
            DematerialiseRemainingFilthInCell(__state.Cell, __state.Map);
        }

        // Destroy any filth still present on the cell. The originally-cleaned filth is
        // already destroyed (and out of the thingGrid) by the time we get here, so this
        // never re-touches it. MopMarks are deliberately skipped — they're a wet-floor
        // trace from a different tool and decay on their own.
        private static void DematerialiseRemainingFilthInCell(IntVec3 cell, Map map)
        {
            var things = map.thingGrid.ThingsListAtFast(cell);
            // Snapshot first because Destroy mutates the grid list under us.
            List<Filth> targets = null;
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is Filth f && !f.Destroyed && f.def != JanitorDefOf.Janitor_MopMark)
                {
                    if (targets == null) targets = new List<Filth>();
                    targets.Add(f);
                }
            }
            if (targets == null) return;
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].Destroy();
            }
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
