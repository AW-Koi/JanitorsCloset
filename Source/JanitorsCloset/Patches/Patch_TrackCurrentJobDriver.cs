using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // Records the active JobDriver_CleanFilth in a thread-static so other patches
    // (effecter suppression, mop-mark spawn) can identify "this filth is being cleaned
    // by a mop-equipped pawn right now."
    //
    // RimWorld 1.6 split per-tick driver work between DriverTick (some setup paths)
    // and DriverTickInterval (the per-tick toil action). The cleaning effecter is
    // spawned during the DriverTick path, but Filth.ThinFilth fires from
    // DriverTickInterval — confirmed via stack trace. Patch both so Current is valid
    // in either; the save/restore pattern handles nested invocation cleanly if one
    // method calls the other.
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
            if (__instance is JobDriver_CleanFilth)
                Current = __instance;
        }

        public static void Postfix(JobDriver __state)
        {
            Current = __state;
        }
    }
}
