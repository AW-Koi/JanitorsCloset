using RimWorld;
using Verse;

namespace JanitorsCloset.Defs
{
    [DefOf]
    public static class JanitorDefOf
    {
        public static ThingDef Janitor_Mop;
        public static ThingDef Janitor_MopMark;

        // Vanilla SoundDef referenced for the mop cleaning-sound swap.
        public static SoundDef Interact_CleanFilth_Fluid;

        static JanitorDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JanitorDefOf));
        }
    }
}
