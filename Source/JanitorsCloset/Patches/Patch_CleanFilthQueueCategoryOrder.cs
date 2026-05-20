using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;
using JanitorMod = JanitorsCloset.JanitorsCloset;

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

            if (JanitorMod.Settings != null && JanitorMod.Settings.DebugLogging)
            {
                int matched = sorted.Count(t => FilthCategoryResolver.Resolve(t.Thing?.def) == preferred);
                bool changed = !sorted.SequenceEqual(queue);
                var breakdown = sorted
                    .GroupBy(t => t.Thing?.def)
                    .Select(g => string.Format("{0}={1}({2})",
                        g.Key?.defName ?? "<null>",
                        g.Count(),
                        FilthCategoryResolver.Resolve(g.Key)?.ToString() ?? "<none>"));
                Log.Message(string.Format(
                    "[JC queue] pawn='{0}' tool='{1}' preferred={2} queue={3} matched={4} reordered={5} breakdown=[{6}]",
                    pawn.LabelShort,
                    pawn.equipment?.Primary?.def?.defName ?? "<none>",
                    preferred.Value,
                    queue.Count,
                    matched,
                    changed,
                    string.Join(", ", breakdown)));
            }

            queue.Clear();
            queue.AddRange(sorted);
        }
    }
}
