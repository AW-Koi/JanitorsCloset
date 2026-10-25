using JanitorsCloset.Defs;
using RimWorld;

namespace JanitorsCloset.Cleaning.ToolEffects
{
    // Routes each work-toil tick to the equipped tool's visual effects.
    public static class CleaningToolEffects
    {
        public static void Tick(CleaningToilSession session, int delta)
        {
            var driver = session.Driver;
            var pawn = driver?.pawn;
            if (pawn?.Map == null || pawn.jobs?.curDriver != driver) return;
            var job = driver.job;
            if (job == null || !job.targetA.IsValid) return;

            var tool = session.ToolDef;
            bool clearingFilth = driver is JobDriver_CleanFilth;
            bool clearingPollution = driver is JobDriver_ClearPollution;

            if (tool == JanitorDefOf.Janitor_StrawBroom || tool == JanitorDefOf.Janitor_PushBroom)
            {
                if (clearingFilth) BroomDustPuff.Tick(session, job);
            }
            else if (tool == JanitorDefOf.Janitor_Glittervacuum)
            {
                if (clearingFilth || clearingPollution) GlittervacuumGlow.Tick(session, job, delta);
            }
            else if (tool != null && tool == JanitorDefOf.Janitor_HazmatSprayer)
            {
                if (!clearingFilth && !clearingPollution) return;
                HazmatSprayPuff.Tick(session, job, delta);
                HazmatFoamDeposit.Tick(session, job, delta);
            }
        }
    }
}
