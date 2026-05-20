using System.Collections.Generic;
using System.Text;
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
    // universal tool like the Glittervacuum). When the queue contains at least one
    // matching-category entry, the mismatched entries are *dropped* so the pawn ends
    // the job after the matches and JobGiver_Work re-scans — that lets a mop walk
    // off after the local blood to find more blood elsewhere instead of grinding
    // through queued trash. When nothing in the queue matches, we leave it alone:
    // the pawn is already doing fallback work, stripping it would just stall them.
    // Companion to Patch_CleanFilthCategoryBias, which handles the initial pick.
    [HarmonyPatch(typeof(WorkGiver_CleanFilth), nameof(WorkGiver_CleanFilth.JobOnThing))]
    [UsedImplicitly]
    public static class Patch_CleanFilthQueueCategoryOrder
    {
        // Per-call scratch buffer. JobOnThing is single-threaded inside the main think
        // loop; reusing the list avoids one allocation per pick.
        [System.ThreadStatic] private static List<LocalTargetInfo> _scratch;

        public static void Postfix(Pawn pawn, Job __result)
        {
            if (__result == null || pawn == null) return;
            var queue = __result.targetQueueA;
            if (queue == null || queue.Count < 2) return;

            var preferred = PawnToolPreference.TryResolvePreferredCategory(pawn);
            if (preferred == null) return;

            int n = queue.Count;
            int matchedCount = 0;
            for (int i = 0; i < n; i++)
            {
                if (FilthCategoryResolver.Resolve(queue[i].Thing?.def) == preferred) matchedCount++;
            }

            var buf = _scratch ?? (_scratch = new List<LocalTargetInfo>(32));
            buf.Clear();
            if (matchedCount > 0)
            {
                // Drop mismatches so the pawn ends and re-scans after the matches.
                for (int i = 0; i < n; i++)
                {
                    if (FilthCategoryResolver.Resolve(queue[i].Thing?.def) == preferred) buf.Add(queue[i]);
                }
            }
            else
            {
                // Nothing matches — keep the whole queue as fallback work.
                for (int i = 0; i < n; i++) buf.Add(queue[i]);
            }

            // Distance-sort. List<T>.Sort is in-place; the captured pawnPos is the only
            // closure allocation, and it's amortised over n*log(n) comparisons.
            IntVec3 pawnPos = pawn.Position;
            buf.Sort((a, b) =>
                (a.Cell - pawnPos).LengthHorizontalSquared
                    .CompareTo((b.Cell - pawnPos).LengthHorizontalSquared));

            if (JanitorMod.Settings?.LogAI == true)
            {
                LogQueueDiagnostic(pawn, preferred.Value, queue, buf, matchedCount);
            }

            queue.Clear();
            queue.AddRange(buf);
            buf.Clear();
        }

        // Diagnostic-only path; allocation cost is acceptable here because the gate
        // is off in normal play. Kept out of the hot path so JIT can inline the caller.
        private static void LogQueueDiagnostic(
            Pawn pawn, CleaningCategory preferred, List<LocalTargetInfo> original,
            List<LocalTargetInfo> sorted, int matched)
        {
            var counts = new Dictionary<ThingDef, int>();
            for (int i = 0; i < original.Count; i++)
            {
                var d = original[i].Thing?.def;
                counts.TryGetValue(d, out var c);
                counts[d] = c + 1;
            }

            var sb = new StringBuilder(128);
            sb.Append("[JC queue] pawn='").Append(pawn.LabelShort)
              .Append("' tool='").Append(pawn.equipment?.Primary?.def?.defName ?? "<none>")
              .Append("' preferred=").Append(preferred)
              .Append(" queue=").Append(original.Count)
              .Append(" matched=").Append(matched)
              .Append(" dropped=").Append(original.Count - sorted.Count)
              .Append(" breakdown=[");
            bool first = true;
            foreach (var kv in counts)
            {
                if (!first) sb.Append(", ");
                first = false;
                sb.Append(kv.Key?.defName ?? "<null>")
                  .Append('=').Append(kv.Value)
                  .Append('(').Append(FilthCategoryResolver.Resolve(kv.Key)?.ToString() ?? "<none>")
                  .Append(')');
            }
            sb.Append(']');
            Log.Message(sb.ToString());
        }
    }
}
