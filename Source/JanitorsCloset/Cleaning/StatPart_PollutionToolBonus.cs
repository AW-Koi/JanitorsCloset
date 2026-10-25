using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Adds a Toxic tool's cleaning-speed offset to general labour speed while its holder clears pollution.
    public class StatPart_PollutionToolBonus : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            val += Bonus(req);
        }

        public override string ExplanationPart(StatRequest req)
        {
            var bonus = Bonus(req);
            if (bonus <= 0f) return null;
            return "JanitorsCloset.CleaningTool.ToxicBonus".Translate(bonus.ToStringPercent());
        }

        private static float Bonus(StatRequest req)
        {
            if (!CleaningToolStats.TryGetTool(req, out var pawn, out var toolThing, out var tool)) return 0f;
            if (!tool.Matches(CleaningCategory.Toxic)) return 0f;
            if (!(CleaningToolStats.CurrentDriver(pawn) is JobDriver_ClearPollution)) return 0f;

            var bonus = CleaningToolStats.EquippedCleaningSpeedOffset(toolThing);
            if (bonus > 0f && CleaningToolStats.LogEnabled)
            {
                Log.Message($"[JC stat] POLLUTION BONUS pawn='{pawn.LabelShort}' tool='{toolThing.def.defName}' +{bonus.ToStringPercent()}");
            }
            return bonus;
        }
    }
}
