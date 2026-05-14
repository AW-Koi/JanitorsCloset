using RimWorld;
using Verse;

namespace JanitorsCloset.Defs
{
    [DefOf]
    public static class JanitorDefOf
    {
        public static ThingDef Janitor_StrawBroom;
        public static ThingDef Janitor_PushBroom;
        public static ThingDef Janitor_Mop;
        public static ThingDef Janitor_MopMark;
        public static ThingDef Janitor_Glittervacuum;

        // Hazmat Sprayer ThingDef and its marker filth — defined under 1.6/Biotech/ and
        // only loaded when Biotech is active. [MayRequire] tells DefOfHelper to leave the
        // field null rather than log a missing-def warning when the DLC isn't present.
        // Every consumer of these fields null-checks before use.
        [MayRequire("Ludeon.RimWorld.Biotech")]
        public static ThingDef Janitor_HazmatSprayer;
        [MayRequire("Ludeon.RimWorld.Biotech")]
        public static ThingDef Janitor_HazmatFoam;

        // Custom flecks for the Glittervacuum's during-cleaning glow.
        public static FleckDef Janitor_GlittervacuumPulse;

        // Hazmat Sprayer chemical-mist puffs spawned at the wand tip during cleaning.
        [MayRequire("Ludeon.RimWorld.Biotech")]
        public static FleckDef Janitor_HazmatChemSpray;

        // Vanilla SoundDefs referenced for the per-tool cleaning-sound swap.
        public static SoundDef Interact_CleanFilth_Fluid;
        public static SoundDef Interact_CleanFilth_Dirt;

        // Memory thought granted to colonists in a room whose last filth was just cleaned.
        public static ThoughtDef Janitor_FreshlyCleanedRoom;

        // Memory thoughts for bystanders/perpetrators of janitor side effects.
        public static ThoughtDef Janitor_SplashedByMop;
        public static ThoughtDef Janitor_TrackedFilthIntoCleanRoom;

        // Hazmat Sprayer overspray.
        [MayRequire("Ludeon.RimWorld.Biotech")]
        public static ThoughtDef Janitor_DousedInDeconFoam;

        static JanitorDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JanitorDefOf));
        }
    }
}
