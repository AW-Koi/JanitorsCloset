using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // Mop marks are decorative traces, not chores. Keep them out of the cleaning work queue
    // entirely. Returning null from JobOnThing tells the WorkGiver scanner there's no job here,
    // so no pawn (janitor or otherwise) will walk over to "clean" the mark. The mark fades on
    // its own via the def's disappearsInDays.
    [HarmonyPatch(typeof(WorkGiver_CleanFilth), nameof(WorkGiver_CleanFilth.JobOnThing))]
    public static class Patch_SkipCleaningMopMark
    {
        public static void Postfix(Thing t, ref Job __result)
        {
            if (t?.def == JanitorDefOf.Janitor_MopMark)
                __result = null;
        }
    }
}
