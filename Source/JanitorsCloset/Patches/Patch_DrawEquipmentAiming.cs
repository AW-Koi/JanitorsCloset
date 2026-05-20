using System;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using JanitorMod = JanitorsCloset.JanitorsCloset;
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
            // Filth-cleaning, Biotech pollution-clearing, and weather-buildup-clearing all
            // drive the anim — a Hazmat Sprayer pawn waving a wand at a polluted tile needs
            // the same reach/wobble treatment as a mop pawn working a blood spatter or a
            // broom pawn sweeping a dusting.
            //
            // Buildup-clearing is detected by driver type (matches the rest of the
            // codebase) so a 1.6 def rename or Odyssey absence can't silently break the
            // anim — JobDriver_ClearSnowAndSand lives in Assembly-CSharp regardless of
            // DLC ownership. JobDefOf.ClearSnow was unreliable here.
            var jobDef = pawn.CurJobDef;
            var driver = pawn.jobs?.curDriver;
            bool isWeatherBuildupJob = driver is JobDriver_ClearSnowAndSand;
            // Log every state transition for an animProfile-bearing tool, so we can see
            // which driver/jobDef vanilla picks for sand clearing vs snow. We skip the
            // common "user is doing literally anything else" reject because it floods the
            // log with Wait_Combat etc. when janitors are drafted.
            if (jobDef != JobDefOf.Clean && jobDef != JobDefOf.ClearPollution && !isWeatherBuildupJob)
            {
                AnimDiagnostics.Log(pawn, eq.def, jobDef, driver, "skip:not-a-cleaning-job");
                return;
            }
            if (isWeatherBuildupJob && !ext.Matches(CleaningCategory.WeatherBuildup))
            {
                AnimDiagnostics.Log(pawn, eq.def, jobDef, driver, "reject:tool-not-weather-buildup");
                return;
            }
            if (pawn.pather != null && pawn.pather.Moving)
            {
                AnimDiagnostics.Log(pawn, eq.def, jobDef, driver, "reject:pawn-moving");
                return;
            }
            AnimDiagnostics.Log(pawn, eq.def, jobDef, driver, "apply");

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

        // Per-pawn dedup keyed on the full state (outcome + jobDef + driver type) so we
        // emit exactly one line per pawn per state change — no flooding when paused or
        // when multiple pawns redraw the same frame. Despawned pawns leak slowly; a
        // debug-only dict is the right tradeoff against a weak-ref dance.
        private static class AnimDiagnostics
        {
            [ThreadStatic] private static System.Collections.Generic.Dictionary<Pawn, string> _lastByPawn;

            public static void Log(Pawn pawn, Def toolDef, Def jobDef, Verse.AI.JobDriver driver, string outcome)
            {
                if (JanitorMod.Settings == null || !JanitorMod.Settings.DebugLogging) return;
                if (pawn == null) return;
                string key = outcome + "|" + (jobDef?.defName ?? "<null>") + "|" + (driver?.GetType().Name ?? "<null>");
                var map = _lastByPawn ?? (_lastByPawn = new System.Collections.Generic.Dictionary<Pawn, string>());
                if (map.TryGetValue(pawn, out var last) && last == key) return;
                map[pawn] = key;
                Verse.Log.Message(string.Format(
                    "[JC anim] pawn='{0}' tool='{1}' jobDef='{2}' driver='{3}' outcome={4}",
                    pawn.LabelShort,
                    toolDef?.defName ?? "<null>",
                    jobDef?.defName ?? "<null>",
                    driver?.GetType().Name ?? "<null>",
                    outcome));
            }
        }
    }
}
