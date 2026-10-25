using System.Collections.Generic;
using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    /// <summary>The sounds vanilla plays while cleaning: every filth's cleaning sound plus the generic one.</summary>
    public static class CleaningSustainers
    {
        private static HashSet<SoundDef> _sounds;

        public static bool Contains(SoundDef sound)
        {
            if (_sounds == null) _sounds = Collect();
            return _sounds.Contains(sound);
        }

        private static HashSet<SoundDef> Collect()
        {
            var sounds = new HashSet<SoundDef> { SoundDefOf.Interact_CleanFilth };
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                var sound = def.filth?.cleaningSound;
                if (sound != null) sounds.Add(sound);
            }
            return sounds;
        }
    }
}
