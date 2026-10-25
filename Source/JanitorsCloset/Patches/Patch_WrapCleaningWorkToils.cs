using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JetBrains.Annotations;
using RimWorld;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // Attaches a CleaningToilSession to the work toil of filth, pollution and snow jobs when the pawn holds a tool.
    [HarmonyPatch]
    [UsedImplicitly]
    public static class Patch_WrapCleaningWorkToils
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(JobDriver_CleanFilth), "MakeNewToils");
            yield return AccessTools.Method(typeof(JobDriver_ClearPollution), "MakeNewToils");
            yield return AccessTools.Method(typeof(JobDriver_ClearSnowAndSand), "MakeNewToils");
        }

        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver __instance)
        {
            var session = CleaningToilSession.TryStart(__instance);
            foreach (var toil in __result)
            {
                if (session != null && CleaningToilSession.IsWorkToil(toil)) session.Attach(toil);
                yield return toil;
            }
        }
    }
}
