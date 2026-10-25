using System.Collections.Generic;
using JanitorsCloset.Defs;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Wet or dry for a filth def: its FilthCategoryExtension if it has one, else wet when it cleans with the fluid sound.
    public static class FilthCategoryResolver
    {
        private static readonly Dictionary<ThingDef, CleaningCategory> Cache = new Dictionary<ThingDef, CleaningCategory>();

        public static CleaningCategory? Resolve(ThingDef filthDef)
        {
            if (filthDef == null) return null;
            if (Cache.TryGetValue(filthDef, out var cached)) return cached;
            var result = ResolveUncached(filthDef);
            Cache[filthDef] = result;
            return result;
        }

        private static CleaningCategory ResolveUncached(ThingDef filthDef)
        {
            var ext = filthDef.GetModExtension<FilthCategoryExtension>();
            if (ext != null) return ext.category;

            var sound = filthDef.filth?.cleaningSound;
            if (sound != null && sound == JanitorDefOf.Interact_CleanFilth_Fluid) return CleaningCategory.Wet;

            return CleaningCategory.Dry;
        }
    }
}
