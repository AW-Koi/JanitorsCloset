using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Cleaning
{
    // Shared lookups for the cleaning-tool stat parts.
    public static class CleaningToolStats
    {
        public static bool TryGetTool(StatRequest req, out Pawn pawn, out Thing toolThing, out CleaningToolExtension tool)
        {
            pawn = req.Thing as Pawn;
            toolThing = pawn?.equipment?.Primary;
            tool = toolThing?.def.GetModExtension<CleaningToolExtension>();
            return tool != null;
        }

        public static JobDriver CurrentDriver(Pawn pawn)
        {
            return pawn.jobs?.curDriver;
        }

        /// <summary>The tool's equipped cleaning-speed offset after quality, as vanilla applies it.</summary>
        public static float EquippedCleaningSpeedOffset(Thing tool)
        {
            return StatWorker.StatOffsetFromGear(tool, StatDefOf.CleaningSpeed);
        }

        public static bool LogEnabled => JanitorsCloset.Settings?.LogStats == true;

        // Info-card text only, so the list allocation is fine here.
        public static string SpecialtyList(CleaningToolExtension tool)
        {
            if (tool.categories == null) return string.Empty;
            var labels = new List<string>(tool.categories.Count);
            for (int i = 0; i < tool.categories.Count; i++)
            {
                labels.Add(("JanitorsCloset.CleaningTool.Specialty" + tool.categories[i]).Translate());
            }
            return labels.ToCommaList();
        }
    }
}
