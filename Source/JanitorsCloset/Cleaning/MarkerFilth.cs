using System.Collections.Generic;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Single source of truth for "is this filth def a janitor-tool marker?"
    // Every consumer (stamping, room-cleanliness scan, cleaning work queue, mood
    // triggers) calls IsMarker rather than enumerating defs by hand.
    public static class MarkerFilth
    {
        public static bool IsMarker(ThingDef def)
        {
            return def != null && def.HasModExtension<JanitorMarkerFilthExtension>();
        }

        public static bool IsMarker(Thing t)
        {
            return t != null && IsMarker(t.def);
        }

        /// <summary>Removes marker filth from the entries appended to the list after the given count.</summary>
        public static void RemoveMarkersAddedSince(List<Thing> things, int countBefore)
        {
            for (int i = things.Count - 1; i >= countBefore; i--)
            {
                if (IsMarker(things[i])) things.RemoveAt(i);
            }
        }
    }
}
