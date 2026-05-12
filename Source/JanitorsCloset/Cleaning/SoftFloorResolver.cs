using System;
using System.Collections.Generic;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Classifies a TerrainDef as soft (carpet/wool/leathery/woven) or hard (stone, wood,
    // metal, tile, concrete). Soft floors absorb the mop's water rather than holding a
    // visible damp patch, so the mop-mark spawn skips them.
    //
    // RimWorld has no built-in soft-floor flag, so we infer from defName patterns and
    // honour an explicit SoftFloorExtension override for modded floors the heuristic
    // misses. Results are cached per TerrainDef.
    public static class SoftFloorResolver
    {
        private static readonly Dictionary<TerrainDef, bool> Cache = new Dictionary<TerrainDef, bool>();

        private static readonly string[] SoftMarkers =
        {
            "Carpet",
            "Wool",
            "Leathery",
            "Leather",
            "Woven",
        };

        public static bool IsSoftFloor(TerrainDef terrain)
        {
            if (terrain == null) return false;
            if (Cache.TryGetValue(terrain, out var cached)) return cached;
            var result = ResolveUncached(terrain);
            Cache[terrain] = result;
            return result;
        }

        private static bool ResolveUncached(TerrainDef terrain)
        {
            var ext = terrain.GetModExtension<SoftFloorExtension>();
            if (ext != null) return ext.isSoft;

            var defName = terrain.defName;
            if (defName == null) return false;
            for (int i = 0; i < SoftMarkers.Length; i++)
            {
                if (defName.IndexOf(SoftMarkers[i], StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }
    }
}
