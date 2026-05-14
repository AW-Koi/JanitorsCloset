using System.Collections.Generic;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;
using RimWorld;
using UnityEngine;
using Verse;

namespace JanitorsCloset.Patches
{
    // When a Glittervacuum-equipped pawn finishes cleaning a filth tile:
    //   1. Drift a few cyan-tinted thick puffs upward at the cell — the un-coupled matter
    //      floating away. No glow flash, no sparks; the during-cleaning pulse glow
    //      (Patch_GlittervacuumCleaningGlow) already supplies the energy half.
    //   2. Dematerialise any *other* filth still on the same cell, plus all filth on
    //      the 8 adjacent cells. Flavor: the field uncouples a 3x3 patch of reality at
    //      once, not just the one piece of dirt the toil was tracking. A single
    //      ThinFilth-to-destroy clears the co-located stack and the neighbour stacks
    //      for free — paired with the removal of CleaningSpeed offset on the weapon,
    //      this is a coverage-for-per-tile-speed rebalance, not a flat buff.
    //
    // Layer accounting for the targeted filth is handled separately by
    // Patch_GlittervacuumLayerCollapse, which collapses thickness to 1 at toil init
    // so vanilla's totalCleaningWorkRequired is computed for a single layer — keeps
    // the progress bar in sync regardless of how many layers vanilla rolled onto
    // whichever filth the toil happened to target.
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
            DematerialiseFilthInCell(__state.Cell, __state.Map, isFocalCell: true);
            for (int i = 0; i < GenAdj.AdjacentCells.Length; i++)
            {
                var neighbour = __state.Cell + GenAdj.AdjacentCells[i];
                if (!neighbour.InBounds(__state.Map)) continue;
                DematerialiseFilthInCell(neighbour, __state.Map, isFocalCell: false);
            }
        }

        // Destroy filth on the cell and spawn a small puff burst when anything was actually
        // cleared. On the focal cell the originally-cleaned filth is already destroyed (and
        // out of the thingGrid) by the time we get here, so this never re-touches it; the
        // focal puff burst is handled separately by SpawnBurst regardless. MopMarks are
        // deliberately skipped — they're a wet-floor trace from a different tool and decay
        // on their own.
        private static void DematerialiseFilthInCell(IntVec3 cell, Map map, bool isFocalCell)
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
            // Only the neighbours need a courtesy puff — the focal cell already got its
            // full burst via SpawnBurst.
            if (!isFocalCell)
            {
                SpawnNeighbourPuff(cell, map);
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

        // One small puff per cleared neighbour cell. Kept deliberately sparser than the
        // focal burst so the eye still reads the toiled-on tile as the centre of the
        // effect rather than the whole 3x3 lighting up identically.
        private static void SpawnNeighbourPuff(IntVec3 cell, Map map)
        {
            var center = cell.ToVector3Shifted();
            var offset = new Vector3(Rand.Range(-0.15f, 0.15f), 0f, Rand.Range(-0.15f, 0.15f));
            FleckMaker.ThrowDustPuffThick(center + offset, map, Rand.Range(0.35f, 0.55f), GlitterTint);
        }
    }
}
