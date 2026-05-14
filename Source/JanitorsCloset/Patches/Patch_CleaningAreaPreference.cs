using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    // Honours CompCleaningPreference on the pawn's equipped tool. The comp lives on the
    // weapon, so non-janitors (no janitor tool equipped → no comp) bypass every check
    // here and behave like vanilla.
    //
    // Two entry points, mirroring Patch_SkipCleaningMarkerFilth:
    //   1. WorkGiver_CleanFilth.HasJobOnThing — automatic work scanning and the
    //      single-filth "Prioritize cleaning X" right-click option.
    //   2. FloatMenuOptionProvider_CleanRoom.GetRoomFilthCleanableByPawn — the bulk
    //      "Clean room" right-click that queues every filth in a room at once.
    public static class Patch_CleaningAreaPreference
    {
        private static CompCleaningPreference EquippedPrefComp(Pawn pawn)
        {
            return pawn?.equipment?.Primary?.GetComp<CompCleaningPreference>();
        }

        // Vanilla Pawn_EquipmentTracker.GetGizmos only invokes CompGetEquippedGizmosExtra on
        // CompEquippable — it never iterates other comps on the weapon. So we inject the
        // preference gizmo here for whichever piece of equipment carries our comp.
        [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.GetGizmos))]
        public static class GetGizmosHook
        {
            public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn_EquipmentTracker __instance)
            {
                foreach (var g in __result) yield return g;
                var eq = __instance?.AllEquipmentListForReading;
                if (eq == null) yield break;
                for (int i = 0; i < eq.Count; i++)
                {
                    var comp = eq[i]?.GetComp<CompCleaningPreference>();
                    if (comp == null) continue;
                    foreach (var gizmo in comp.BuildGizmos()) yield return gizmo;
                }
            }
        }

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
                        "RimWorld may have moved or renamed it. Cleaning area preference will not be enforced.");
                }
                return m;
            }

            public static void Postfix(Pawn pawn, Thing t, ref bool __result)
            {
                if (!__result) return;
                if (!(t is Filth filth)) return;
                var comp = EquippedPrefComp(pawn);
                if (comp == null) return;
                if (!comp.Matches(filth.Position, filth.Map)) __result = false;
            }
        }

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
                        "RimWorld may have moved or renamed it. \"Clean room\" right-click will ignore the cleaning area preference.");
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

            public static void Postfix(Pawn pawn, List<Filth> __result)
            {
                if (__result == null || __result.Count == 0) return;
                var comp = EquippedPrefComp(pawn);
                if (comp == null) return;
                for (int i = __result.Count - 1; i >= 0; i--)
                {
                    var f = __result[i];
                    if (f != null && !comp.Matches(f.Position, f.Map)) __result.RemoveAt(i);
                }
            }
        }
    }
}
