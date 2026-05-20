using System.Collections.Generic;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Attached to a weapon ThingDef to declare which filth categories this tool speeds up.
    // A tool that lists no categories never matches — equivalent to "no specialty bonus on
    // any filth," which is almost certainly a def error, so we log it during DefOf-init time.
    public class CleaningToolExtension : DefModExtension
    {
        public List<CleaningCategory> categories;

        // Optional motion config for the tool while its bearer is cleaning. Null means
        // no custom animation — the weapon stays in the default carry pose.
        public CleaningAnimProfile animProfile;

        // Optional SoundDef override for the cleaning sustainer. When set, Patch_CleaningToolSound
        // forces this sound regardless of filth — useful for tools that don't fit the dry/wet
        // sonic dichotomy (e.g. the Glittervacuum, which is a humming field device).
        public SoundDef customCleaningSound;

        // When true, a successful clean dematerialises every filth on the target cell *and*
        // its eight neighbours in one pass, not just the piece the toil was tracking. Gates
        // Patch_GlittervacuumDematerialise and surfaces a "Cleaning specialty" entry on the
        // info card. Named for the original stack-clearing behaviour; the 3x3 radius was
        // bundled in later as part of the same field-collapse effect.
        public bool clearsFilthStack;

        public bool Matches(CleaningCategory category)
        {
            return categories != null && categories.Contains(category);
        }
    }
}
