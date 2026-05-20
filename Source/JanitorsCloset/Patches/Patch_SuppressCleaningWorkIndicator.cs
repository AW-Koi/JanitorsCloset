using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using Verse;

namespace JanitorsCloset.Patches
{
    // Vanilla spawns small dustpan-and-brush style effecters at the cell during cleaning
    // work — "Clean" for filth, "ClearSnow"/"ClearSand" for weather buildup. Those are
    // redundant and visually wrong when our pawn is already swinging a real tool. Clear
    // the spawned effecter's children when the active cleaner is wielding the right kind
    // of tool: any CleaningToolExtension covers the filth effecter, while the snow/sand
    // effecters only get suppressed for tools that actually declare the WeatherBuildup
    // category (so a mop somehow on a snow job still gets the vanilla icon).
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

            var driver = Patch_TrackCurrentJobDriver.Current;
            var primary = driver?.pawn?.equipment?.Primary;
            if (primary == null) return;
            var ext = primary.def.GetModExtension<CleaningToolExtension>();
            if (ext == null) return;

            switch (__instance.defName)
            {
                case "Clean":
                    __result.children?.Clear();
                    break;
                case "ClearSnow":
                case "ClearSand":
                    if (ext.Matches(CleaningCategory.WeatherBuildup)) __result.children?.Clear();
                    break;
            }
        }
    }
}
