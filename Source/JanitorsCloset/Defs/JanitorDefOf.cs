using RimWorld;
using Verse;

namespace JanitorsCloset
{
    [DefOf]
    public static class JanitorDefOf
    {
        public static ThingDef Janitor_Mop;
        public static ThingDef Janitor_MopMark;

        static JanitorDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JanitorDefOf));
        }
    }
}
