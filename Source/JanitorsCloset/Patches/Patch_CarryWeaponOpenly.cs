using HarmonyLib;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    [HarmonyPatch(typeof(PawnRenderUtility), "CarryWeaponOpenly")]
    public static class Patch_CarryWeaponOpenly
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (__result) return;
            if (pawn == null) return;
            if (pawn.CurJobDef != JobDefOf.Clean) return;
            if (pawn.equipment?.Primary?.def != JanitorDefOf.Janitor_Mop) return;

            __result = true;
        }
    }
}
