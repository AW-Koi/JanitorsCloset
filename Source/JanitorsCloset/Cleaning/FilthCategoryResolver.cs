using System;
using System.Collections.Generic;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Decides whether a given Filth def counts as Wet or Dry. The default policy infers
    // from the def's own cleaningSound: vanilla flags wet filth with Interact_CleanFilth_Fluid
    // and dry filth with Interact_CleanFilth_Dirt (or similar). That signal already covers
    // every vanilla and DLC filth without us having to enumerate them, and most filth-adding
    // mods set a cleaning sound too.
    //
    // A FilthCategoryExtension on a Filth def is always honoured first — that's the escape
    // hatch for cases where a modded filth's sound misleads or is missing.
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
            if (sound != null && sound.defName != null
                && sound.defName.IndexOf("Fluid", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return CleaningCategory.Wet;
            }

            // Default: Dry. Catches both the "Dirt"/loose sounds and modded filth that
            // doesn't set a cleaningSound at all — sweeping is the more conservative guess.
            return CleaningCategory.Dry;
        }
    }
}
