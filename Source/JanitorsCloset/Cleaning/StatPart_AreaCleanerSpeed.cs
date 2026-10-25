using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // An area-clearing tool cleans each tile at half speed, its own bonus cancelled; its edge comes from clearing nine tiles at once.
    public class StatPart_AreaCleanerSpeed : StatPart
    {
        private const float PerTileSpeedPenalty = 0.5f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (IsAreaCleanerCleaning(req, out var toolThing)) val -= Penalty(toolThing);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!CleaningToolStats.TryGetTool(req, out _, out var toolThing, out var tool) || !tool.clearsFilthStack) return null;
            var key = IsAreaCleanerCleaning(req, out _)
                ? "JanitorsCloset.CleaningTool.AreaCleanerPerTile"
                : "JanitorsCloset.CleaningTool.AreaCleanerPerTileWhenCleaning";
            return key.Translate(Penalty(toolThing).ToStringPercent());
        }

        private static float Penalty(Thing toolThing)
        {
            return CleaningToolStats.EquippedCleaningSpeedOffset(toolThing) + PerTileSpeedPenalty;
        }

        private static bool IsAreaCleanerCleaning(StatRequest req, out Thing toolThing)
        {
            if (!CleaningToolStats.TryGetTool(req, out var pawn, out toolThing, out var tool) || !tool.clearsFilthStack) return false;
            return CleaningToolStats.CurrentDriver(pawn) is JobDriver_CleanFilth;
        }
    }
}
