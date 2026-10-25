using JanitorsCloset.Defs;
using RimWorld;
using Verse.AI;

namespace JanitorsCloset.Cleaning.ToolEffects
{
    // An area-clearing tool cleans a stacked filth in one pass, so its work is computed for thickness 1.
    public static class GlittervacuumLayerCollapse
    {
        public static void Apply(CleaningToilSession session)
        {
            if (!(session.Driver is JobDriver_CleanFilth)) return;
            var filth = session.Driver.job?.GetTarget(TargetIndex.A).Thing as Filth;
            if (filth == null || filth.Destroyed) return;
            if (filth.def == JanitorDefOf.Janitor_MopMark) return;
            if (filth.thickness > 1) filth.thickness = 1;
        }
    }
}
