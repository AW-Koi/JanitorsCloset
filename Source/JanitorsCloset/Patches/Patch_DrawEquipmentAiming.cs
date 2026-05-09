using System;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Defs;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
// ReSharper disable InconsistentNaming

namespace JanitorsCloset.Patches
{
    [HarmonyPatch]
    [UsedImplicitly]
    public static class Patch_DrawEquipmentAiming
    {
        private const float BasePhaseRate  = 0.025f;
        private const float SpeedModRate   = 0.025f;         // how often stroke speed itself varies
        private const float SpeedModDepth  = 3f;            // how much it varies (radians of phase wobble)
        private const float WobbleDegrees  = 20f;
        private const float SlideTiles     = 0.125f;
        private const float MopReachFactor = 0.3f;         // fraction of the pawn->target vector to push drawLoc by
        private const float MoppingRotationOffset = 30f;    // offset to rotate the mop while in use
        private const float MoppingVerticalOffset = 0.3f;   // offset to shift the mop vertically while in use

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

            // Push drawLoc toward the cell being cleaned so the mop reaches the actual filth,
            // not the pawn's feet. Target can be any of the 9 cells around the pawn (or the cell
            // they're standing on, in which case the offset is zero and the mop stays centered).
            var job = pawn.CurJob;
            if (job != null && job.targetA.IsValid)
            {
                Vector3 toTarget = job.targetA.CenterVector3 - pawn.Position.ToVector3Shifted();
                drawLoc.x += toTarget.x * MopReachFactor;
                drawLoc.z += toTarget.z * MopReachFactor;
            }

            // Phase-modulated wobble: the stroke angle is sin(baseT + depth*sin(modT)). The
            // instantaneous speed (derivative) varies on a slow sine, so strokes feel organic
            // — push, ease, pull, ease — instead of constant metronomic sweep.
            int ticks = Find.TickManager.TicksGame;
            float t = ticks * BasePhaseRate + SpeedModDepth * Mathf.Sin(ticks * SpeedModRate);
            float wobble = Mathf.Sin(t) * WobbleDegrees;
            float slide  = Mathf.Cos(t) * SlideTiles;

            float postWobbleRad = (aimAngle + wobble) * Mathf.Deg2Rad;
            drawLoc.x += Mathf.Cos(postWobbleRad) * slide;
            drawLoc.z += Mathf.Sin(postWobbleRad) * slide + MoppingVerticalOffset;
            aimAngle  += wobble + MoppingRotationOffset;
        }
    }
}
