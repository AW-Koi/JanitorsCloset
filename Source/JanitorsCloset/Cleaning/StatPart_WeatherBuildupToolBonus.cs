using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Adds a WeatherBuildup tool's cleaning-speed offset to general labour speed while its holder clears snow or sand.
    public class StatPart_WeatherBuildupToolBonus : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            val += Bonus(req);
        }

        public override string ExplanationPart(StatRequest req)
        {
            var bonus = Bonus(req);
            if (bonus <= 0f) return null;
            return "JanitorsCloset.CleaningTool.WeatherBuildupBonus".Translate() + ": +" + bonus.ToStringPercent();
        }

        private static float Bonus(StatRequest req)
        {
            if (!CleaningToolStats.TryGetTool(req, out var pawn, out var toolDef, out var tool)) return 0f;
            if (!tool.Matches(CleaningCategory.WeatherBuildup)) return 0f;
            if (!(CleaningToolStats.CurrentDriver(pawn) is JobDriver_ClearSnowAndSand)) return 0f;

            var bonus = CleaningToolStats.EquippedCleaningSpeedOffset(toolDef);
            if (bonus > 0f && CleaningToolStats.LogEnabled)
            {
                Log.Message($"[JC stat] WEATHER BUILDUP BONUS pawn='{pawn.LabelShort}' tool='{toolDef.defName}' +{bonus.ToStringPercent()}");
            }
            return bonus;
        }
    }
}
