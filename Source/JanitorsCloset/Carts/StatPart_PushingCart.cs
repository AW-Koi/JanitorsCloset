using JanitorsCloset.Defs;
using RimWorld;
using Verse;

namespace JanitorsCloset.Carts
{
    /// <summary>Slows a pawn while they push a janitor's cart.</summary>
    public class StatPart_PushingCart : StatPart
    {
        public const float SpeedFactor = 0.85f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (IsPushing(req)) val *= SpeedFactor;
        }

        public override string ExplanationPart(StatRequest req)
        {
            return IsPushing(req) ? "JanitorsCloset.Cart.PushingSpeed".Translate(SpeedFactor.ToStringPercent()).Resolve() : null;
        }

        private static bool IsPushing(StatRequest req)
        {
            return req.Thing is Pawn pawn && pawn.carryTracker?.CarriedThing?.def == JanitorDefOf.Janitor_Cart;
        }
    }
}
