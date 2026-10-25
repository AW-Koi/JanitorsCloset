using JanitorsCloset.Cleaning;
using RimWorld;
using Verse;

namespace JanitorsCloset.Carts
{
    /// <summary>Removes a dry mop's bonus on the filth it would otherwise speed up, until it's wetted at the cart.</summary>
    public class StatPart_DryMop : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            val -= Penalty(req);
        }

        public override string ExplanationPart(StatRequest req)
        {
            var penalty = Penalty(req);
            return penalty > 0f ? "JanitorsCloset.Mop.Dry".Translate(penalty.ToStringPercent()).Resolve() : null;
        }

        private static float Penalty(StatRequest req)
        {
            if (!CleaningToolStats.TryGetTool(req, out var pawn, out var toolThing, out var tool)) return 0f;
            if (toolThing.TryGetComp<CompMopCharge>()?.Wetness.Empty != true) return 0f;
            if (!(CleaningToolStats.CurrentDriver(pawn) is JobDriver_CleanFilth driver)) return 0f;
            if (!(driver.job?.targetA.Thing is Filth filth)) return 0f;
            var category = FilthCategoryResolver.Resolve(filth.def);
            if (category == null || !tool.Matches(category.Value)) return 0f;
            return CleaningToolStats.EquippedCleaningSpeedOffset(toolThing);
        }
    }
}
