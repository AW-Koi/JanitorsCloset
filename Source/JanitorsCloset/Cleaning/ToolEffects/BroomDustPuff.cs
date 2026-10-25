using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Cleaning.ToolEffects
{
    // A dust puff at the end of each broom push stroke, in phase with the swing animation.
    public static class BroomDustPuff
    {
        private const float GoldenRatioFraction = 0.6180339887f;
        private const float TwoPi = Mathf.PI * 2f;

        public static void Tick(CleaningToilSession session, Job job)
        {
            var profile = session.Tool.animProfile;
            if (profile == null) return;

            var pawn = session.Pawn;
            int ticks = Find.TickManager.TicksGame;
            float pawnPhase = (pawn.thingIDNumber * GoldenRatioFraction) % 1f * TwoPi;
            float t = ticks * profile.basePhaseRate
                + profile.speedModDepth * Mathf.Sin(ticks * profile.speedModRate)
                + pawnPhase;
            int cycle = Mathf.FloorToInt(t / TwoPi);

            int last = session.LastStrokeCycle;
            session.LastStrokeCycle = cycle;
            if (last == int.MinValue || cycle == last) return;

            SpawnDustPuff(pawn, job.targetA.CenterVector3);
        }

        // Flick the puff just past the target along the pawn-to-target line.
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
