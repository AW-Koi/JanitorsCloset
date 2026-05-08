using System;
using HarmonyLib;
using RimWorld;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.DriverTick))]
    public static class Patch_TrackCurrentJobDriver
    {
        [ThreadStatic]
        public static JobDriver Current;

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
