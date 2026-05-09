using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Defs;
using Verse;

namespace JanitorsCloset.Patches
{
    [HarmonyPatch]
    public static class Patch_SuppressCleaningWorkIndicator
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(EffecterDef).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => (m.Name == "Spawn" || m.Name == "SpawnMaintained" || m.Name == "SpawnAttached")
                            && m.ReturnType == typeof(Effecter))
                .Cast<MethodBase>();
        }

        public static void Postfix(EffecterDef __instance, Effecter __result)
        {
            if (__result == null) return;
            if (__instance?.defName != "Clean") return;

            var driver = Patch_TrackCurrentJobDriver.Current;
            if (driver?.pawn?.equipment?.Primary?.def != JanitorDefOf.Janitor_Mop) return;

            __result.children?.Clear();
        }
    }
}
