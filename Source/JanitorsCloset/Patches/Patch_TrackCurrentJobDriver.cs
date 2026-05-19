using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // Records the active cleaning/pollution JobDriver in a thread-static so other patches
    // can identify "this filth/cell is being worked by a tool-equipped pawn right now."
    //
    // RimWorld 1.6 splits per-tick driver work between DriverTick (cleaning effecter spawn)
    // and DriverTickInterval (Filth.ThinFilth). Patch both so Current is valid in either;
    // the save/restore pattern handles nested invocation if one method calls the other.
    [HarmonyPatch]
    public static class Patch_TrackCurrentJobDriver
    {
        [ThreadStatic]
        public static JobDriver Current;

        public static IEnumerable<MethodBase> TargetMethods()
        {
            var dti = AccessTools.Method(typeof(JobDriver), "DriverTickInterval");
            if (dti != null) yield return dti;
            var dt = AccessTools.Method(typeof(JobDriver), "DriverTick");
            if (dt != null) yield return dt;
        }

        public static void Prefix(JobDriver __instance, out JobDriver __state)
        {
            __state = Current;
            // JobDriver_ClearPollution drives terrain pollution cleanup (Biotech).
            // JobDriver_ClearSnowAndSand drives weather-buildup-layer cleanup
            // (vanilla snow + Odyssey sand share one driver and one depth grid concept).
            // Both are terrain work scaled by GeneralLaborSpeed; the bonus StatParts gate
            // on tool category and the matching driver type. JobDriver_CleanFilth covers
            // the filth case via the CleaningSpeed StatPart.
            if (__instance is JobDriver_CleanFilth
                || __instance is JobDriver_ClearPollution
                || __instance is JobDriver_ClearSnowAndSand)
                Current = __instance;
        }

        public static void Postfix(JobDriver __state)
        {
            Current = __state;
        }
    }
}
