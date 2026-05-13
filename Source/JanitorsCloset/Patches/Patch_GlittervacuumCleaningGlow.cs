using HarmonyLib;
using JanitorsCloset.Defs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // While a Glittervacuum-wielding pawn is on a cleaning job, spawn the slow cyan
    // Janitor_GlittervacuumPulse fleck under the wand head. The fleck's own fade-in /
    // solid / fade-out curve does the breathing; the spawn cadence just keeps a fresh
    // one starting before the previous fades out so the glow reads as continuous rather
    // than blinking. Companion to Patch_GlittervacuumDematerialise, which fires the
    // floating-away puffs and clears any co-located filth when the cell finishes.
    [HarmonyPatch]
    public static class Patch_GlittervacuumCleaningGlow
    {
        // Pulse fleck total lifespan is ~0.95s (0.25 fade-in + 0.3 solid + 0.4 fade-out).
        // Spawning every 45 ticks (~0.75s) overlaps adjacent pulses by ~0.2s so the glow
        // stays continuous without strobing while still appearing promptly when cleaning
        // starts and clearing quickly when a tile finishes.
        private const int PulseIntervalTicks = 45;

        [HarmonyPatch(typeof(JobDriver), "DriverTickInterval")]
        [HarmonyPostfix]
        public static void Postfix(JobDriver __instance)
        {
            // Fire on both filth cleaning and Biotech pollution clearing — the
            // Glittervacuum is an omni-tool (Dry + Wet + Toxic categories), so its
            // glow should follow it into pollution work the same way it follows it
            // onto a blood spatter or a dirt pile.
            if (!(__instance is JobDriver_CleanFilth) && !(__instance is JobDriver_ClearPollution)) return;

            var pawn = __instance.pawn;
            if (pawn?.equipment?.Primary?.def != JanitorDefOf.Janitor_Glittervacuum) return;
            if (pawn.Map == null) return;
            if (pawn.pather != null && pawn.pather.Moving) return;

            var job = __instance.job;
            if (job == null || !job.targetA.IsValid) return;

            int ticks = Find.TickManager.TicksGame;
            // Per-pawn phase so multiple Glittervacuums don't pulse in lockstep.
            int phase = pawn.thingIDNumber & 0xFF;
            if ((ticks + phase) % PulseIntervalTicks != 0) return;

            // Spawn at the filth cell — that's where the wand head reaches to once the
            // aimAtTarget profile shifts the draw point. Scale ~5 so the LightningGlow
            // texture's soft falloff covers roughly a 5x5 area, reading as ambient
            // lighting around the wand rather than a tight pinpoint mote. Slight jitter
            // so the breathing isn't perfectly uniform.
            Vector3 center = job.targetA.CenterVector3;
            FleckMaker.Static(center, pawn.Map, JanitorDefOf.Janitor_GlittervacuumPulse, Rand.Range(4.8f, 5.4f));
        }
    }
}
