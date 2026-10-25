using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Removes a tool's cleaning-speed bonus while its holder cleans filth outside the tool's categories.
    public class StatPart_CleaningToolMatch : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            var penalty = OffsetToSuppress(req, out _);
            if (penalty > 0f) val -= penalty;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!CleaningToolStats.TryGetTool(req, out _, out var toolDef, out var tool)) return null;

            var penalty = OffsetToSuppress(req, out bool cleaning);
            if (penalty > 0f)
            {
                return "JanitorsCloset.CleaningTool.WrongFilthCategory".Translate(penalty.ToStringPercent());
            }
            if (cleaning || CleaningToolStats.EquippedCleaningSpeedOffset(toolDef) <= 0f) return null;
            if (tool.Matches(CleaningCategory.Dry) && tool.Matches(CleaningCategory.Wet)) return null;
            return "JanitorsCloset.CleaningTool.BonusAppliesOnlyTo".Translate(CleaningToolStats.SpecialtyList(tool));
        }

        // The offset to remove, and whether the pawn is cleaning filth right now.
        private static float OffsetToSuppress(StatRequest req, out bool cleaning)
        {
            cleaning = false;
            if (!CleaningToolStats.TryGetTool(req, out var pawn, out var toolDef, out var tool)) return 0f;
            if (!(CleaningToolStats.CurrentDriver(pawn) is JobDriver_CleanFilth driver)) return 0f;

            if (!(driver.job?.targetA.Thing is Filth filth)) return 0f;
            var category = FilthCategoryResolver.Resolve(filth.def);
            if (category == null) return 0f;
            cleaning = true;
            if (tool.Matches(category.Value)) return 0f;

            var penalty = CleaningToolStats.EquippedCleaningSpeedOffset(toolDef);
            if (CleaningToolStats.LogEnabled)
            {
                Log.Message($"[JC stat] MISMATCH pawn='{pawn.LabelShort}' tool='{toolDef.defName}' filth='{filth.def.defName}' cat={category.Value} -> suppress {penalty.ToStringPercent()}");
            }
            return penalty;
        }
    }
}
