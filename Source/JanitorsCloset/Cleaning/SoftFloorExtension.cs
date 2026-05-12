using Verse;

namespace JanitorsCloset.Cleaning
{
    // Attach to a TerrainDef to force the soft/hard classification regardless of the
    // built-in heuristic. Useful when modded carpet/leathery/woven floors don't match
    // the defName patterns SoftFloorResolver uses by default.
    public class SoftFloorExtension : DefModExtension
    {
        public bool isSoft = true;
    }
}
