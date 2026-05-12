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

        public bool Matches(CleaningCategory category)
        {
            return categories != null && categories.Contains(category);
        }
    }
}
