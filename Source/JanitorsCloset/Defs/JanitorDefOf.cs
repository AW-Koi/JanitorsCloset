using RimWorld;
using Verse;

namespace JanitorsCloset.Defs
{
    [DefOf]
    public static class JanitorDefOf
    {
        public static ThingDef Janitor_Mop;
        public static ThingDef Janitor_MopMark;
        public static ThingDef Janitor_Glittervacuum;

        // Custom flecks for the Glittervacuum's during-cleaning glow.
        public static FleckDef Janitor_GlittervacuumPulse;

        // Vanilla SoundDefs referenced for the per-tool cleaning-sound swap.
        public static SoundDef Interact_CleanFilth_Fluid;
        public static SoundDef Interact_CleanFilth_Dirt;

        // Mod SoundDef for the Glittervacuum cleaning sustainer.
        public static SoundDef Janitor_Glittervacuum_Cleaning;

        static JanitorDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JanitorDefOf));
        }
    }
}
