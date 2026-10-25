using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Cleaning
{
    // Shared lookups for the cleaning-tool stat parts.
    public static class CleaningToolStats
    {
        public static bool TryGetTool(StatRequest req, out Pawn pawn, out ThingDef toolDef, out CleaningToolExtension tool)
        {
            pawn = req.Thing as Pawn;
            toolDef = pawn?.equipment?.Primary?.def;
            tool = toolDef?.GetModExtension<CleaningToolExtension>();
            return tool != null;
        }

        public static JobDriver CurrentDriver(Pawn pawn)
        {
            return pawn.jobs?.curDriver;
        }

        public static float EquippedCleaningSpeedOffset(ThingDef toolDef)
        {
            List<StatModifier> offsets = toolDef.equippedStatOffsets;
            if (offsets == null) return 0f;
            for (int i = 0; i < offsets.Count; i++)
            {
                if (offsets[i].stat == StatDefOf.CleaningSpeed) return offsets[i].value;
            }
            return 0f;
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
