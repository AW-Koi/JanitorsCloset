using JanitorsCloset.Patches;
using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Silent per-tile slowdown applied while a pawn is actively running a cleaning toil with
    // an area-clearing tool (CleaningToolExtension.clearsFilthStack). The tool clears a 3x3
    // patch per toil — without this penalty its throughput on dense floors scales by up to 9x.
    // Reducing the per-tile rate keeps the area advantage on busy rooms while letting sparse
    // floors come out roughly break-even with a bare hand.
    //
    // Intentionally invisible: no ExplanationPart, no equippedStatOffsets entry on the weapon.
    // The slowdown is part of the field-collapse fantasy, not a knob the player tunes against.
    // The penalty only ever applies during a real cleaning job for the wielding pawn, so UI
    // inspections of the weapon still report its nominal speed.
    public class StatPart_AreaCleanerSilentSpeed : StatPart
    {
        private const float PerTileSpeedPenalty = 0.5f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!IsAreaCleanerActive(req)) return;
            val -= PerTileSpeedPenalty;
        }

        public override string ExplanationPart(StatRequest req)
        {
            return null;
        }

        private static bool IsAreaCleanerActive(StatRequest req)
        {
            if (!(req.Thing is Pawn pawn)) return false;
            var tool = pawn.equipment?.Primary;
            if (tool == null) return false;
            var ext = tool.def.GetModExtension<CleaningToolExtension>();
            if (ext == null || !ext.clearsFilthStack) return false;
            var driver = Patch_TrackCurrentJobDriver.Current as JobDriver_CleanFilth;
            return driver != null && driver.pawn == pawn;
        }
    }
}
