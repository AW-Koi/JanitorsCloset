using System.Collections.Generic;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // Kicks up a small dust puff at the end of each broom push stroke — the tick where the
    // broom reaches max forward extent in the CleaningAnimProfile phase model used by
    // Patch_DrawEquipmentAiming (cos(t) at its peak, i.e. t crossing a 2π boundary).
    //
    // Brooms only: dust fits a dry-bristle stroke. The mop, Glittervacuum and Hazmat
    // Sprayer have their own VFX and dust would be wrong for wet/wand tools.
    [HarmonyPatch]
    public static class Patch_BroomDustPuff
    {
        private const float GoldenRatioFraction = 0.6180339887f;
        private const float TwoPi = Mathf.PI * 2f;

        // Per-pawn last-seen stroke-cycle index. DriverTickInterval can sample on any
        // cadence, so "compare with the previous tick" would miss transitions when the
        // hook fires every N ticks rather than every tick. The dict isn't persisted —
        // a stale entry on first stroke after load costs at most one mistimed puff.
        private static readonly Dictionary<int, int> LastStrokeCycle = new Dictionary<int, int>();

        [HarmonyPatch(typeof(JobDriver), "DriverTickInterval")]
        [HarmonyPostfix]
        public static void Postfix(JobDriver __instance)
        {
            if (!(__instance is JobDriver_CleanFilth)) return;

            var pawn = __instance.pawn;
            var weaponDef = pawn?.equipment?.Primary?.def;
            if (weaponDef == null) return;
            if (weaponDef != JanitorDefOf.Janitor_StrawBroom && weaponDef != JanitorDefOf.Janitor_PushBroom) return;

            if (pawn.Map == null) return;
            if (pawn.pather != null && pawn.pather.Moving) return;

            var job = __instance.job;
            if (job == null || !job.targetA.IsValid) return;

            var profile = weaponDef.GetModExtension<CleaningToolExtension>()?.animProfile;
            if (profile == null) return;

            int ticks = Find.TickManager.TicksGame;
            float pawnPhase = (pawn.thingIDNumber * GoldenRatioFraction) % 1f * TwoPi;
            float t = ticks * profile.basePhaseRate
                + profile.speedModDepth * Mathf.Sin(ticks * profile.speedModRate)
                + pawnPhase;
            int cycle = Mathf.FloorToInt(t / TwoPi);

            int pawnId = pawn.thingIDNumber;
            int last;
            bool tracked = LastStrokeCycle.TryGetValue(pawnId, out last);
            LastStrokeCycle[pawnId] = cycle;
            if (!tracked || cycle == last) return;

            SpawnDustPuff(pawn, job.targetA.CenterVector3);
        }

        // Spawn the puff just past the target cell along the pawn→target line so the
        // dust reads as flicked forward by the bristles. Tiny jitter keeps successive
        // strokes from stacking identical puffs on the same point.
        private static void SpawnDustPuff(Pawn pawn, Vector3 targetCenter)
        {
            Vector3 dir = targetCenter - pawn.DrawPos;
            dir.y = 0f;
            float sqr = dir.sqrMagnitude;
            Vector3 unit = sqr > 0.0001f ? dir / Mathf.Sqrt(sqr) : Vector3.zero;

            Vector3 spawn = targetCenter + unit * 0.15f;
            spawn.x += Rand.Range(-0.10f, 0.10f);
            spawn.z += Rand.Range(-0.10f, 0.10f);

            FleckMaker.ThrowDustPuff(spawn, pawn.Map, Rand.Range(0.7f, 1.0f));
        }
    }
}
