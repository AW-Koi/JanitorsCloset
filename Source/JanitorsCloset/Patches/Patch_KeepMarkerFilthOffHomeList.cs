using System.Collections.Generic;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    // Keeps marker filth off the home-area filth list, so idle cleaners never scan it as work.
    public static class Patch_KeepMarkerFilthOffHomeList
    {
        [HarmonyPatch(typeof(ListerFilthInHomeArea), nameof(ListerFilthInHomeArea.Notify_FilthSpawned))]
        public static class OnSpawned
        {
            public static bool Prefix(Filth f) => !MarkerFilth.IsMarker(f);
        }

        [HarmonyPatch(typeof(ListerFilthInHomeArea), nameof(ListerFilthInHomeArea.Notify_FilthDespawned))]
        public static class OnDespawned
        {
            public static bool Prefix(Filth f) => !MarkerFilth.IsMarker(f);
        }

        // Also covers RebuildAll, which calls this for every cell.
        [HarmonyPatch(typeof(ListerFilthInHomeArea), nameof(ListerFilthInHomeArea.Notify_HomeAreaChanged))]
        public static class OnHomeAreaChanged
        {
            public static void Prefix(List<Thing> ___filthInHomeArea, out int __state)
            {
                __state = ___filthInHomeArea.Count;
            }

            public static void Postfix(List<Thing> ___filthInHomeArea, int __state)
            {
                MarkerFilth.RemoveMarkersAddedSince(___filthInHomeArea, __state);
            }
        }
    }
}
