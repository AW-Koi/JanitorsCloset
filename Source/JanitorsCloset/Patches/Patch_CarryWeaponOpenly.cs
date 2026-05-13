using HarmonyLib;
using JanitorsCloset.Cleaning;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    // Force vanilla to keep the equipped weapon visible while a pawn is doing a cleaning
    // job (either Clean for filth or ClearPollution for Biotech pollution work), for any
    // tool that declares itself a cleaning tool via CleaningToolExtension. Without this,
    // RimWorld treats cleaning as "working" and hides the weapon, so the broom/mop/sprayer/
    // glittervacuum vanishes during the path-to-target and while working.
    [HarmonyPatch(typeof(PawnRenderUtility), "CarryWeaponOpenly")]
    public static class Patch_CarryWeaponOpenly
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (__result) return;
            if (pawn == null) return;
            var jobDef = pawn.CurJobDef;
            if (jobDef != JobDefOf.Clean && jobDef != JobDefOf.ClearPollution) return;

            var primary = pawn.equipment?.Primary;
            if (primary == null) return;
            if (primary.def.GetModExtension<CleaningToolExtension>() == null) return;

            __result = true;
        }
    }
}
