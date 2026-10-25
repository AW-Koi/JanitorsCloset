using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // An area-clearing tool cleans each tile at half speed; its edge comes from clearing nine tiles at once.
    public class StatPart_AreaCleanerSpeed : StatPart
    {
        private const float PerTileSpeedPenalty = 0.5f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (IsAreaCleanerCleaning(req)) val -= PerTileSpeedPenalty;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!CleaningToolStats.TryGetTool(req, out _, out _, out var tool) || !tool.clearsFilthStack) return null;
            var key = IsAreaCleanerCleaning(req)
                ? "JanitorsCloset.CleaningTool.AreaCleanerPerTile"
                : "JanitorsCloset.CleaningTool.AreaCleanerPerTileWhenCleaning";
            return key.Translate() + ": -" + PerTileSpeedPenalty.ToStringPercent();
        }

        private static bool IsAreaCleanerCleaning(StatRequest req)
        {
            if (!CleaningToolStats.TryGetTool(req, out var pawn, out _, out var tool) || !tool.clearsFilthStack) return false;
            return CleaningToolStats.CurrentDriver(pawn) is JobDriver_CleanFilth;
        }
    }
}
