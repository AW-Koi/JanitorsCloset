using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    [HarmonyPatch]
    public static class Patch_CarryWeaponOpenly
    {
        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(PawnRenderer), "CarryWeaponOpenly");
            if (method == null)
            {
                throw new InvalidOperationException(
                    "[Janitor's Closet] Could not find Verse.PawnRenderer.CarryWeaponOpenly. " +
                    "RimWorld may have renamed or moved the method — mop will not be visible during cleaning.");
            }
            return method;
        }

        private static readonly AccessTools.FieldRef<PawnRenderer, Pawn> PawnField =
            AccessTools.FieldRefAccess<PawnRenderer, Pawn>("pawn");

        public static void Postfix(PawnRenderer __instance, ref bool __result)
        {
            if (__result) return;
            if (__instance == null) return;

            var pawn = PawnField(__instance);
            if (pawn == null) return;
            if (pawn.CurJobDef != JobDefOf.Clean) return;

            var primary = pawn.equipment?.Primary;
            if (primary?.def != JanitorDefOf.Janitor_Mop) return;

            __result = true;
        }
    }
}
