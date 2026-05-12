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
    // than blinking. Companion to Patch_SpawnDematerialiseEffect, which fires the
    // floating-away puffs when the filth is finally destroyed.
    [HarmonyPatch]
    public static class Patch_GlittervacuumCleaningGlow
    {
        // Pulse fleck total lifespan is ~2.1s (0.7 fade-in + 0.4 solid + 1.0 fade-out).
        // Spawning every 75 ticks (~1.25s) overlaps adjacent pulses so the glow is
        // continuous and slowly breathes rather than strobing.
        private const int PulseIntervalTicks = 75;

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
            // Per-pawn phase so multiple Glittervacuums don't pulse in lockstep.
            int phase = pawn.thingIDNumber & 0xFF;
            if ((ticks + phase) % PulseIntervalTicks != 0) return;

            // Spawn at the filth cell — that's where the wand head reaches to once the
            // aimAtTarget profile shifts the draw point. Slight scale jitter so the
            // breathing isn't perfectly uniform.
            Vector3 center = job.targetA.CenterVector3;
            FleckMaker.Static(center, pawn.Map, JanitorDefOf.Janitor_GlittervacuumPulse, Rand.Range(0.85f, 1.05f));
        }
    }
}
