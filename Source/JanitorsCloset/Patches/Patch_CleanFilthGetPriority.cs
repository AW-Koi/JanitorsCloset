using HarmonyLib;
using JanitorsCloset.Cleaning;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    // Ranks filth for a pawn with a preferring tool: matching filth up to ~40 tiles further away beats a closer mismatch.
    [HarmonyPatch(typeof(WorkGiver_Scanner), nameof(WorkGiver_Scanner.GetPriority), typeof(Pawn), typeof(TargetInfo))]
    [UsedImplicitly]
    public static class Patch_CleanFilthGetPriority
    {
        private const float CategoryBonus = 1600f;

        public static void Postfix(WorkGiver_Scanner __instance, Pawn pawn, TargetInfo t, ref float __result)
        {
            if (!(__instance is WorkGiver_CleanFilth)) return;
            if (pawn == null || !t.IsValid) return;

            var preferred = PawnToolPreference.TryResolvePreferredCategory(pawn);
            if (preferred == null) return;
            CleanFilthBiasDiagnostics.OnceForScan(pawn, preferred);

            float distSq = (t.Cell - pawn.Position).LengthHorizontalSquared;
            var filthCat = FilthCategoryResolver.Resolve(t.Thing?.def);
            __result = (filthCat == preferred ? CategoryBonus : 0f) - distSq;
        }
    }
}
