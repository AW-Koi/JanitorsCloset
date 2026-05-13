using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    // Marker filth (visual traces dropped by tool use) is not a chore — keep these defs
    // out of every code path that would let a pawn try to clean them. Cleaning happens
    // from multiple entry points in RimWorld and each one needs its own gate:
    //
    //   1. Automatic work scanning — pawns picking up cleaning jobs themselves. Gated
    //      via WorkGiver_CleanFilth.HasJobOnThing. Returning false here also gates the
    //      single-filth "Prioritize cleaning X" right-click option, which calls this
    //      same method to decide whether to offer the menu item.
    //
    //   2. "Clean room" float menu — right-click a room with a colonist selected and
    //      pick "Clean [room]". This iterates room.ContainedAndAdjacentThings.OfType<Filth>()
    //      and queues *every* match directly, bypassing HasJobOnThing entirely. We have
    //      to filter the returned list from FloatMenuOptionProvider_CleanRoom's private
    //      helper to keep marker filth out of that queue too.
    //
    // Each tracked def fades on its own via disappearsInDays, so we never want a pawn to
    // bother with them at all.
    //
    // Covered defs:
    //   * Janitor_MopMark    — damp-floor trace left by mopping. Cosmetic only.
    //   * Janitor_HazmatFoam — decontaminant foam deposited by the Hazmat Sprayer.
    //                          Auto-decays in hours; cleaning would race the spawn loop.
    public static class Patch_SkipCleaningMarkerFilth
    {
        private static bool IsMarkerFilth(Thing t)
        {
            if (t == null) return false;
            var def = t.def;
            return def == JanitorDefOf.Janitor_MopMark || def == JanitorDefOf.Janitor_HazmatFoam;
        }

        // Auto-scan + single-target "Prioritize cleaning X" — both ride HasJobOnThing.
        // Patching this is the upstream gate; returning false here prevents JobOnThing
        // from ever being called and also avoids vanilla's "CanGiveJob and JobOnX may
        // not be synchronized" log spam (which fires when JobOnThing returns null after
        // HasJobOnThing said yes).
        [HarmonyPatch]
        public static class HasJobOnThingHook
        {
            public static MethodBase TargetMethod()
            {
                var m = AccessTools.DeclaredMethod(typeof(WorkGiver_CleanFilth), "HasJobOnThing");
                if (m == null)
                {
                    throw new InvalidOperationException(
                        "[Janitor's Closet] Could not find WorkGiver_CleanFilth.HasJobOnThing — " +
                        "RimWorld may have moved or renamed it. Marker filth will be cleaned by other pawns.");
                }
                return m;
            }

            public static void Postfix(Thing t, ref bool __result)
            {
                if (IsMarkerFilth(t)) __result = false;
            }
        }

        // "Clean room" float menu — strip marker filth from the list this helper returns
        // so the queued cleaning job never includes our deposits. Private static method,
        // located via AccessTools by string name.
        [HarmonyPatch]
        public static class CleanRoomFilthListHook
        {
            public static MethodBase TargetMethod()
            {
                var providerType = AccessTools.TypeByName("RimWorld.FloatMenuOptionProvider_CleanRoom");
                if (providerType == null)
                {
                    throw new InvalidOperationException(
                        "[Janitor's Closet] Could not find RimWorld.FloatMenuOptionProvider_CleanRoom — " +
                        "RimWorld may have moved or renamed it. \"Clean room\" right-click will try to clean marker filth.");
                }
                var m = AccessTools.DeclaredMethod(providerType, "GetRoomFilthCleanableByPawn");
                if (m == null)
                {
                    throw new InvalidOperationException(
                        "[Janitor's Closet] Could not find GetRoomFilthCleanableByPawn on " +
                        "FloatMenuOptionProvider_CleanRoom — RimWorld may have renamed it.");
                }
                return m;
            }

            // The helper returns a List<Filth>. We mutate it in place (it's vanilla's
            // own tmpFilth static buffer, so the next invocation reclears it anyway —
            // safe to remove entries here).
            public static void Postfix(List<Filth> __result)
            {
                if (__result == null || __result.Count == 0) return;
                for (int i = __result.Count - 1; i >= 0; i--)
                {
                    if (IsMarkerFilth(__result[i])) __result.RemoveAt(i);
                }
            }
        }
    }
}
