using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    // Hides vanilla's cleaning effecter while a tool holder works; snow and sand only for weather-buildup tools.
    [HarmonyPatch]
    public static class Patch_SuppressCleaningWorkIndicator
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(EffecterDef).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => (m.Name == "Spawn" || m.Name == "SpawnMaintained" || m.Name == "SpawnAttached")
                            && m.ReturnType == typeof(Effecter));
        }

        public static void Postfix(EffecterDef __instance, Effecter __result)
        {
            if (__result == null || __instance == null) return;

            var driver = ActiveCleaningDriver.Current;
            var primary = driver?.pawn?.equipment?.Primary;
            if (primary == null) return;
            var ext = primary.def.GetModExtension<CleaningToolExtension>();
            if (ext == null) return;

            if (__instance == EffecterDefOf.Clean)
            {
                __result.children?.Clear();
            }
            else if ((__instance == EffecterDefOf.ClearSnow || __instance == EffecterDefOf.ClearSand)
                     && ext.Matches(CleaningCategory.WeatherBuildup))
            {
                __result.children?.Clear();
            }
        }
    }
}
