using System;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
// ReSharper disable InconsistentNaming

namespace JanitorsCloset.Patches
{
    // Animates the equipped weapon sprite while the bearer is on a cleaning job, per the
    // CleaningAnimProfile attached to its CleaningToolExtension. A tool without a profile
    // (straw broom, Glittervacuum) just stays in the default carry pose.
    //
    // See CleaningAnimProfile for the per-parameter explanation of the stroke model.
    [HarmonyPatch]
    [UsedImplicitly]
    public static class Patch_DrawEquipmentAiming
    {
        private const float GoldenRatioFraction = 0.6180339887f;

        public static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(PawnRenderUtility), "DrawEquipmentAiming");
            if (method == null)
            {
                throw new InvalidOperationException(
                    "[Janitor's Closet] Could not find Verse.PawnRenderUtility.DrawEquipmentAiming. " +
                    "RimWorld may have renamed or moved the method — cleaning tool animations will not work.");
            }
            return method;
        }

        public static void Prefix(Thing eq, ref Vector3 drawLoc, ref float aimAngle)
        {
            if (eq == null) return;
            var ext = eq.def.GetModExtension<CleaningToolExtension>();
            var profile = ext?.animProfile;
            if (profile == null) return;

            if (!(eq.ParentHolder is Pawn_EquipmentTracker tracker)) return;
            var pawn = tracker.pawn;
            if (pawn == null) return;
            var jobDef = pawn.CurJobDef;
            // Filth-cleaning, Biotech pollution-clearing, and weather-buildup-clearing all
            // drive the anim — a Hazmat Sprayer pawn waving a wand at a polluted tile needs
            // the same reach/wobble treatment as a mop pawn working a blood spatter or a
            // broom pawn sweeping a dusting.
            //
            // For weather-buildup work we additionally gate on the tool's depth cap: if a
            // broom is standing on Thick buildup it gets no labor-speed bonus, so it should
            // also stay in the default carry pose. Anim presence == tool advantage active.
            bool isWeatherBuildupJob = jobDef == JobDefOf.ClearSnow;
            if (jobDef != JobDefOf.Clean && jobDef != JobDefOf.ClearPollution && !isWeatherBuildupJob) return;
            if (isWeatherBuildupJob)
            {
                var buildupJob = pawn.CurJob;
                if (buildupJob == null || !buildupJob.targetA.IsValid) return;
                if (!StatPart_WeatherBuildupToolBonus.ToolEligibleAt(ext, pawn.Map, buildupJob.targetA.Cell)) return;
            }
            if (pawn.pather != null && pawn.pather.Moving) return;

            // Push drawLoc toward the cell being cleaned so the tool reaches the actual filth,
            // not the pawn's feet. Target can be any of the 9 cells around the pawn (or the cell
            // they're standing on, in which case the offset is zero and the tool stays centered).
            var job = pawn.CurJob;
            Vector3 toTarget = Vector3.zero;
            bool hasDirectionalTarget = false;
            if (job != null && job.targetA.IsValid)
            {
                toTarget = job.targetA.CenterVector3 - pawn.Position.ToVector3Shifted();
                drawLoc.x += toTarget.x * profile.reachFactor;
                drawLoc.z += toTarget.z * profile.reachFactor;
                // Cleaning the cell underfoot leaves toTarget effectively zero — no direction
                // to aim at, so wand-style tools fall back to the default carry angle.
                hasDirectionalTarget = toTarget.x * toTarget.x + toTarget.z * toTarget.z > 0.0001f;
            }

            // Wand-style override: replace the default facing-derived carry angle with the
            // pawn→filth direction so the tool actually points at the cell it's cleaning.
            // equippedAngleOffset (on the ThingDef) is applied inside DrawEquipmentAiming on top.
            // RimWorld's aimAngle is compass-style: 0°=N, 90°=E, 180°=S, 270°=W — i.e.
            // Atan2(x, z), NOT the standard math Atan2(z, x).
            if (profile.aimAtTarget && hasDirectionalTarget)
            {
                aimAngle = toTarget.AngleFlat();
            }

            // Phase-modulated wobble: the stroke angle is sin(baseT + depth*sin(modT)). The
            // instantaneous speed (derivative) varies on a slow sine, so strokes feel organic
            // — push, ease, pull, ease — instead of constant metronomic sweep.
            // Per-pawn phase offset desyncs multiple janitors working at once.
            int ticks = Find.TickManager.TicksGame;
            float pawnPhase = (pawn.thingIDNumber * GoldenRatioFraction) % 1f * Mathf.PI * 2f;
            float t = ticks * profile.basePhaseRate + profile.speedModDepth * Mathf.Sin(ticks * profile.speedModRate) + pawnPhase;
            float wobble = Mathf.Sin(t) * profile.wobbleDegrees;
            float slide  = Mathf.Cos(t) * profile.slideTiles;

            float postWobbleRad = (aimAngle + wobble) * Mathf.Deg2Rad;
            drawLoc.x += Mathf.Cos(postWobbleRad) * slide;
            drawLoc.z += Mathf.Sin(postWobbleRad) * slide + profile.verticalOffset;
            aimAngle  += wobble + profile.rotationOffset;
        }
    }
}
