using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // Re-sort the in-trip filth queue produced by WorkGiver_CleanFilth.JobOnThing so
    // same-category filth gets cleaned first within a single trip, with distance as
    // the tie-break. Vanilla only sorts the queue by distance, and only when it has
    // 5+ entries — we sort whenever there are at least 2 so even a tiny mixed cluster
    // gets the bias.
    //
    // Bails when the pawn has no clear category preference (no janitor tool, or a
    // universal tool like the Glittervacuum). Mismatched filth in the queue is never
    // removed, just deferred — the pawn still cleans every cell that was already
    // queued. Companion to Patch_CleanFilthCategoryBias, which handles the initial
    // pick across the map.
    [HarmonyPatch(typeof(WorkGiver_CleanFilth), nameof(WorkGiver_CleanFilth.JobOnThing))]
    [UsedImplicitly]
    public static class Patch_CleanFilthQueueCategoryOrder
    {
        public static void Postfix(Pawn pawn, Job __result)
        {
            if (__result == null || pawn == null) return;
            var queue = __result.targetQueueA;
            if (queue == null || queue.Count < 2) return;

            var preferred = PawnToolPreference.TryResolvePreferredCategory(pawn);
            if (preferred == null) return;

            IntVec3 pawnPos = pawn.Position;
            // OrderBy / ThenBy is stable, so equal-category-equal-distance cells keep
            // their vanilla radial order. Reassign through Clear+AddRange to keep the
            // job's list reference intact.
            List<LocalTargetInfo> sorted = queue
                .OrderBy(t => FilthCategoryResolver.Resolve(t.Thing?.def) == preferred ? 0 : 1)
                .ThenBy(t => (t.Cell - pawnPos).LengthHorizontalSquared)
                .ToList();
            queue.Clear();
            queue.AddRange(sorted);
        }
    }
}
