using System;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    // Mop marks are decorative traces, not chores — keep them out of the cleaning work queue
    // entirely. Patching HasJobOnThing (rather than JobOnThing) is the right hook: it's the
    // upstream gate the work scanner uses to decide whether a thing is even worth pursuing.
    // Returning false here means JobOnThing is never called for mop marks, which both prevents
    // any cleaning job from forming AND avoids vanilla's "CanGiveJob and JobOnX may not be
    // synchronized" error spam (which fires when JobOnThing returns null after HasJobOnThing
    // said yes). The mark fades on its own via the def's disappearsInDays.
    [HarmonyPatch]
    public static class Patch_SkipCleaningMopMark
    {
        public static MethodBase TargetMethod()
        {
            var m = AccessTools.DeclaredMethod(typeof(WorkGiver_CleanFilth), "HasJobOnThing");
            if (m == null)
            {
                throw new InvalidOperationException(
                    "[Janitor's Closet] Could not find WorkGiver_CleanFilth.HasJobOnThing — " +
                    "RimWorld may have moved or renamed it. Mop marks will be cleaned by other pawns.");
            }
            return m;
        }

        public static void Postfix(Thing t, ref bool __result)
        {
            if (t?.def == JanitorDefOf.Janitor_MopMark)
                __result = false;
        }
    }
}
