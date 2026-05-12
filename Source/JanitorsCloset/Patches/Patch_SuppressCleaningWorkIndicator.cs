using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using Verse;

namespace JanitorsCloset.Patches
{
    // Vanilla spawns a small dustpan-and-brush effecter at the cell when any pawn cleans
    // filth. That's redundant — and visually wrong — when our pawn is already swinging a
    // real tool. Clear the effecter's children when the active cleaner is wielding any
    // tool with a CleaningToolExtension.
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
            if (__result == null) return;
            if (__instance?.defName != "Clean") return;

            var driver = Patch_TrackCurrentJobDriver.Current;
            var primary = driver?.pawn?.equipment?.Primary;
            if (primary == null) return;
            if (primary.def.GetModExtension<CleaningToolExtension>() == null) return;

            __result.children?.Clear();
        }
    }
}
