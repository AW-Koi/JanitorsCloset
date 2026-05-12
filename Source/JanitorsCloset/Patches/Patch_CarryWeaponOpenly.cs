using HarmonyLib;
using JanitorsCloset.Cleaning;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    // Force vanilla to keep the equipped weapon visible while a pawn is doing a Clean job,
    // for any tool that declares itself a cleaning tool via CleaningToolExtension. Without
    // this, RimWorld treats cleaning as "working" and hides the weapon, so the broom/mop/
    // glittervacuum vanishes during the path-to-filth and while scrubbing.
    [HarmonyPatch(typeof(PawnRenderUtility), "CarryWeaponOpenly")]
    public static class Patch_CarryWeaponOpenly
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (__result) return;
            if (pawn == null) return;
            if (pawn.CurJobDef != JobDefOf.Clean) return;

            var primary = pawn.equipment?.Primary;
            if (primary == null) return;
            if (primary.def.GetModExtension<CleaningToolExtension>() == null) return;

            __result = true;
        }
    }
}
