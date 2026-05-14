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
    }
}
