using HarmonyLib;
using JanitorsCloset.Cleaning;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // Force vanilla to keep the equipped weapon visible while a pawn is doing a cleaning
    // job (Clean for filth, ClearPollution for Biotech pollution, ClearSnow for weather
    // buildup), for any tool that declares itself a cleaning tool via CleaningToolExtension.
    // Without this, vanilla's CarryWeaponOpenly returns false for undrafted work jobs and
    // DrawEquipmentAndApparelExtras skips DrawCarriedWeapon — the broom/mop/sprayer/
    // glittervacuum then vanishes during the path-to-target and while working, and our
    // DrawEquipmentAiming anim hook has nothing to act on.
    //
    // The weather-buildup gate uses driver type rather than JobDefOf.ClearSnow — the rest
    // of the codebase identifies that job that way, and it survives any future def rename.
    [HarmonyPatch(typeof(PawnRenderUtility), "CarryWeaponOpenly")]
    public static class Patch_CarryWeaponOpenly
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (__result) return;
            if (pawn == null) return;

            var primary = pawn.equipment?.Primary;
            if (primary == null) return;
            var ext = primary.def.GetModExtension<CleaningToolExtension>();
            if (ext == null) return;

            var jobDef = pawn.CurJobDef;
            if (jobDef == JobDefOf.Clean || jobDef == JobDefOf.ClearPollution)
            {
                __result = true;
                return;
            }
            if (pawn.jobs?.curDriver is JobDriver_ClearSnowAndSand
                && ext.Matches(CleaningCategory.WeatherBuildup))
            {
                __result = true;
            }
        }
    }
}
