using System;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    // Marker filth (visual traces dropped by tool use) is not a chore — keep these defs
    // out of the cleaning work queue entirely. Patching HasJobOnThing (rather than
    // JobOnThing) is the right hook: it's the upstream gate the work scanner uses to
    // decide whether a thing is even worth pursuing. Returning false here means
    // JobOnThing is never called for these defs, which both prevents any cleaning job
    // from forming AND avoids vanilla's "CanGiveJob and JobOnX may not be synchronized"
    // error spam (which fires when JobOnThing returns null after HasJobOnThing said yes).
    // Each tracked def fades on its own via disappearsInDays.
    //
    // Covered defs:
    //   * Janitor_MopMark    — damp-floor trace left by mopping. Cosmetic only.
    //   * Janitor_HazmatFoam — decontaminant foam deposited by the Hazmat Sprayer.
    //                          Auto-decays in hours; cleaning would race the spawn loop.
    [HarmonyPatch]
    public static class Patch_SkipCleaningMarkerFilth
    {
        public static MethodBase TargetMethod()
        {
            var m = AccessTools.DeclaredMethod(typeof(WorkGiver_CleanFilth), "HasJobOnThing");
            if (m == null)
            {
                throw new InvalidOperationException(
                    "[Janitor's Closet] Could not find WorkGiver_CleanFilth.HasJobOnThing — " +
                    "RimWorld may have moved or renamed it. Decorative filth marks will be cleaned by other pawns.");
            }
            return m;
        }

        public static void Postfix(Thing t, ref bool __result)
        {
            if (t == null) return;
            var def = t.def;
            if (def == JanitorDefOf.Janitor_MopMark || def == JanitorDefOf.Janitor_HazmatFoam)
                __result = false;
        }
    }
}
