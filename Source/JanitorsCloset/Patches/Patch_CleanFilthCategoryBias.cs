using HarmonyLib;
using JanitorsCloset.Cleaning;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // Bias the initial filth pick toward the pawn's tool category. WorkGiver_CleanFilth
    // doesn't override Prioritized or GetPriority, so JobGiver_Work runs it as a plain
    // closest-reachable scan capped at 4 regions. We force Prioritized = true (switching
    // the dispatcher to GenClosest.ClosestThing_Global_Reachable with our scoring
    // function) and provide a score that prefers matching-category filth within ~15
    // tiles of distance equivalence.
    //
    // Companion to Patch_CleanFilthQueueCategoryOrder, which biases the in-trip queue
    // after the initial pick has been made.

    // Prioritized and GetPriority are declared on WorkGiver_Scanner; WorkGiver_CleanFilth
    // inherits them without override, so we must patch the base and gate by instance type.

    [HarmonyPatch(typeof(WorkGiver_Scanner), nameof(WorkGiver_Scanner.Prioritized), MethodType.Getter)]
    [UsedImplicitly]
    public static class Patch_CleanFilthPrioritized
    {
        public static void Postfix(WorkGiver_Scanner __instance, ref bool __result)
        {
            if (__instance is WorkGiver_CleanFilth) __result = true;
        }
    }

    [HarmonyPatch(typeof(WorkGiver_Scanner), nameof(WorkGiver_Scanner.GetPriority),
        new[] { typeof(Pawn), typeof(TargetInfo) })]
    [UsedImplicitly]
    public static class Patch_CleanFilthGetPriority
    {
        // 15 tiles squared. A matching-category filth up to ~15 tiles further away beats
        // a closer mismatched filth; beyond that, distance wins. Calibrated so pawns
        // don't trek across the map but reliably specialise within a typical base.
        private const float CategoryBonus = 225f;

        public static void Postfix(WorkGiver_Scanner __instance, Pawn pawn, TargetInfo t, ref float __result)
        {
            if (!(__instance is WorkGiver_CleanFilth)) return;
            if (pawn == null || !t.IsValid) return;

            float distSq = (t.Cell - pawn.Position).LengthHorizontalSquared;
            var preferred = PawnToolPreference.TryResolvePreferredCategory(pawn);
            if (preferred == null)
            {
                // No preference -> distance-only ranking, equivalent to vanilla
                // closest-first selection now that Prioritized is forced on.
                __result = -distSq;
                return;
            }

            var filthCat = FilthCategoryResolver.Resolve(t.Thing?.def);
            float bonus = filthCat == preferred ? CategoryBonus : 0f;
            __result = bonus - distSq;
        }
    }
}
