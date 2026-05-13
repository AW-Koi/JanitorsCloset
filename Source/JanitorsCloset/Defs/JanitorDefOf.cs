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
        public static ThingDef Janitor_HazmatSprayer;
        public static ThingDef Janitor_HazmatFoam;

        // Custom flecks for the Glittervacuum's during-cleaning glow.
        public static FleckDef Janitor_GlittervacuumPulse;

        // Hazmat Sprayer chemical-mist puffs spawned at the wand tip during cleaning.
        public static FleckDef Janitor_HazmatChemSpray;

        // Vanilla SoundDefs referenced for the per-tool cleaning-sound swap.
        public static SoundDef Interact_CleanFilth_Fluid;
        public static SoundDef Interact_CleanFilth_Dirt;

        static JanitorDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JanitorDefOf));
        }
    }
}
