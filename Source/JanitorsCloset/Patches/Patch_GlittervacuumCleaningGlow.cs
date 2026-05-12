using HarmonyLib;
using JanitorsCloset.Defs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // While a Glittervacuum-wielding pawn is on a cleaning job, throw a periodic
    // lightning-glow fleck (plus an occasional microspark) at the cell being cleaned
    // so the dematerialise field reads as an active, pulsing light source rather than
    // a silent animation. Companion to Patch_SpawnDematerialiseEffect, which fires the
    // one-shot burst when the filth is finally destroyed.
    [HarmonyPatch]
    public static class Patch_GlittervacuumCleaningGlow
    {
        // Glow re-thrown every N ticks. ThrowLightningGlow's own fleck lifespan is short,
        // so the cadence is what produces the flicker — too low and it strobes, too high
        // and it gaps out.
        private const int GlowIntervalTicks = 12;
        private const int SparkIntervalTicks = 30;

        [HarmonyPatch(typeof(JobDriver), "DriverTickInterval")]
        [HarmonyPostfix]
        public static void Postfix(JobDriver __instance)
        {
            if (!(__instance is JobDriver_CleanFilth)) return;

            var pawn = __instance.pawn;
            if (pawn?.equipment?.Primary?.def != JanitorDefOf.Janitor_Glittervacuum) return;
            if (pawn.Map == null) return;
            if (pawn.pather != null && pawn.pather.Moving) return;

            var job = __instance.job;
            if (job == null || !job.targetA.IsValid) return;

            int ticks = Find.TickManager.TicksGame;
            // Per-pawn offset so multiple Glittervacuums don't pulse in lockstep.
            int phase = pawn.thingIDNumber & 0xFF;

            Vector3 center = job.targetA.CenterVector3;

            if ((ticks + phase) % GlowIntervalTicks == 0)
            {
                // Size varies slightly each pulse for an unsettled "field's working on it" feel.
                FleckMaker.ThrowLightningGlow(center, pawn.Map, Rand.Range(0.9f, 1.3f));
            }

            if ((ticks + phase) % SparkIntervalTicks == 0)
            {
                var offset = new Vector3(Rand.Range(-0.2f, 0.2f), 0f, Rand.Range(-0.2f, 0.2f));
                FleckMaker.ThrowMicroSparks(center + offset, pawn.Map);
            }
        }
    }
}
