using JanitorsCloset.Defs;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Carts
{
    /// <summary>Once the cleaning is done, takes a dry cart, or one idle away from home for four hours, back to the sink.</summary>
    public class WorkGiver_ReturnCartHome : WorkGiver
    {
        public override Job NonScanJob(Pawn pawn)
        {
            var carts = pawn.Map.listerThings.ThingsOfDef(JanitorDefOf.Janitor_Cart);
            if (carts.Count == 0) return null;
            var sink = JanitorCarts.FindHome(pawn);
            if (sink == null) return null;

            for (int i = 0; i < carts.Count; i++)
            {
                var cart = carts[i];
                var comp = cart.TryGetComp<CompJanitorCart>();
                if (comp == null || !comp.AvailableTo(pawn) || cart.IsForbidden(pawn)) continue;
                bool atHome = JanitorCarts.IsHome(cart, sink);
                if (!comp.Water.Empty && !CartIdleRule.ShouldGoHome(comp.TicksIdle, atHome)) continue;
                if (!pawn.CanReserveAndReach(cart, PathEndMode.ClosestTouch, Danger.Some)) continue;
                var job = JanitorCarts.MakeReturnJob(pawn, cart, sink);
                if (job != null) return job;
            }
            return null;
        }
    }
}
