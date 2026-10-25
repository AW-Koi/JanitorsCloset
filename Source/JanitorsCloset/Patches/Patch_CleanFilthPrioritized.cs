using HarmonyLib;
using JanitorsCloset.Cleaning;
using JetBrains.Annotations;
using RimWorld;

namespace JanitorsCloset.Patches
{
    // Scores every home filth only for a pawn whose tool prefers a category; everyone else keeps vanilla's nearest search.
    [HarmonyPatch(typeof(WorkGiver_Scanner), nameof(WorkGiver_Scanner.Prioritized), MethodType.Getter)]
    [UsedImplicitly]
    public static class Patch_CleanFilthPrioritized
    {
        public static void Postfix(WorkGiver_Scanner __instance, ref bool __result)
        {
            if (!(__instance is WorkGiver_CleanFilth)) return;
            if (PawnToolPreference.TryResolvePreferredCategory(Patch_TrackWorkScanPawn.ScanningPawn) != null) __result = true;
        }
    }
}
