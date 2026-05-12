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

        // Vanilla SoundDefs referenced for the per-tool cleaning-sound swap.
        public static SoundDef Interact_CleanFilth_Fluid;
        public static SoundDef Interact_CleanFilth_Dirt;

        static JanitorDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JanitorDefOf));
        }
    }
}
