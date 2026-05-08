using System.Collections;
using System.Collections.Generic;
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
            yield return AccessTools.Method(typeof(EffecterDef), "Spawn",
                new[] { typeof(Thing), typeof(Map), typeof(float) });
            yield return AccessTools.Method(typeof(EffecterDef), "SpawnMaintained",
                new[] { typeof(Thing), typeof(Map), typeof(float) });
        }

        public static void Postfix(EffecterDef __instance, Thing target, Map map, Effecter __result)
        {
            if (__result == null) return;
            if (__instance?.defName != "Clean") return;
            if (target == null || map == null) return;

            if (!IsBeingCleanedByMopHolder(target, map)) return;

            var children = Traverse.Create(__result).Field("children").GetValue<IList>();
            children?.Clear();
        }

        private static bool IsBeingCleanedByMopHolder(Thing target, Map map)
        {
            foreach (var pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (pawn.CurJobDef != JobDefOf.Clean) continue;
                if (pawn.CurJob?.targetA.Thing != target) continue;
                if (pawn.equipment?.Primary?.def != JanitorDefOf.Janitor_Mop) continue;
                return true;
            }
            return false;
        }
    }
}
