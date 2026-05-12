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

        // Vanilla SoundDef referenced for the mop cleaning-sound swap.
        public static SoundDef Interact_CleanFilth_Fluid;

        // Custom fleck spawned per particle in the Glittervacuum's dematerialise burst.
        public static FleckDef Janitor_DematerialiseShimmer;

        static JanitorDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JanitorDefOf));
        }
    }
}
