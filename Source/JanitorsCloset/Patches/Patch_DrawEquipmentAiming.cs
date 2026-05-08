using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace JanitorsCloset.Patches
{
    [HarmonyPatch]
    public static class Patch_DrawEquipmentAiming
    {
        private const float PhaseRate     = 0.42f;
        private const float WobbleDegrees = 18f;
        private const float SlideTiles    = 0.12f;

        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(PawnRenderUtility), "DrawEquipmentAiming");
            if (method == null)
            {
                throw new InvalidOperationException(
                    "[Janitor's Closet] Could not find Verse.PawnRenderUtility.DrawEquipmentAiming. " +
                    "RimWorld may have renamed or moved the method — mop cleaning animation will not work.");
            }
            return method;
        }

        public static void Prefix(Thing eq, ref Vector3 drawLoc, ref float aimAngle)
        {
            if (eq?.def != JanitorDefOf.Janitor_Mop) return;
            if (!(eq.ParentHolder is Pawn_EquipmentTracker tracker)) return;

            var pawn = tracker.pawn;
            if (pawn == null) return;
            if (pawn.CurJobDef != JobDefOf.Clean) return;
            if (pawn.pather != null && pawn.pather.Moving) return;

            float t = Find.TickManager.TicksGame * PhaseRate;
            float wobble = Mathf.Sin(t) * WobbleDegrees;
            float slide  = Mathf.Cos(t) * SlideTiles;

            float postWobbleRad = (aimAngle + wobble) * Mathf.Deg2Rad;
            drawLoc.x += Mathf.Cos(postWobbleRad) * slide;
            drawLoc.z += Mathf.Sin(postWobbleRad) * slide;
            aimAngle  += wobble;
        }
    }
}
