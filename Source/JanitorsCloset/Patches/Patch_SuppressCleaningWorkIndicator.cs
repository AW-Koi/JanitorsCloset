using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
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
            if (!IsAnyMopWielderCleaning()) return;

            __result.children?.Clear();
        }

        private static bool IsAnyMopWielderCleaning()
        {
            var maps = Find.Maps;
            if (maps == null) return false;

            for (int i = 0; i < maps.Count; i++)
            {
                var pawns = maps[i].mapPawns?.AllPawnsSpawned;
                if (pawns == null) continue;

                for (int j = 0; j < pawns.Count; j++)
                {
                    var pawn = pawns[j];
                    if (pawn.CurJobDef != JobDefOf.Clean) continue;
                    if (pawn.equipment?.Primary?.def != JanitorDefOf.Janitor_Mop) continue;
                    return true;
                }
            }
            return false;
        }
    }
}
