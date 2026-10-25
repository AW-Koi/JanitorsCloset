using HarmonyLib;
using JanitorsCloset.Carts;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // Puts a janitor's cart errand (fetch water, wheel the cart over, wet the mop) before the clean; forced orders clean straight away.
    [HarmonyPatch(typeof(WorkGiver_CleanFilth), nameof(WorkGiver_CleanFilth.JobOnThing))]
    [UsedImplicitly]
    public static class Patch_CartErrandBeforeCleaning
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Pawn pawn, bool forced, ref Job __result)
        {
            if (forced || __result?.def != JobDefOf.Clean) return;
            if (JanitorCarts.TryMakeErrand(pawn, __result, out var errand)) __result = errand;
        }
    }
}
