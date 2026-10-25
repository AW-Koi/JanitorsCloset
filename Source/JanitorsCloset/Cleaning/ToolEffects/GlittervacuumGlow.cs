using JanitorsCloset.Defs;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Cleaning.ToolEffects
{
    // The slow cyan breathing glow under the glittervacuum head; first pulse fires on the job's first tick.
    public static class GlittervacuumGlow
    {
        // Pulse fleck lives ~47 ticks; spawning every 40 keeps the glow continuous.
        private const int PulseIntervalTicks = 40;

        public static void Tick(CleaningToilSession session, Job job, int delta)
        {
            if (!session.GlowInterval.Elapse(delta, PulseIntervalTicks)) return;
            FleckMaker.Static(job.targetA.CenterVector3, session.Pawn.Map,
                JanitorDefOf.Janitor_GlittervacuumPulse, Rand.Range(4.8f, 5.4f));
        }
    }
}
