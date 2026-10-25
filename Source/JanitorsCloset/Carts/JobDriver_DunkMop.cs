using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Carts
{
    /// <summary>Wets the mop in the cart's bucket, using one dunk of its water, then queues the clean.</summary>
    public class JobDriver_DunkMop : JobDriver
    {
        private const TargetIndex CartIndex = TargetIndex.A;
        private const int DunkTicks = 120;

        private Thing Cart => job.GetTarget(CartIndex).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Cart, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(CartIndex);
            yield return Toils_Goto.GotoThing(CartIndex, PathEndMode.Touch);
            yield return Toils_General.Wait(DunkTicks, CartIndex).WithProgressBarToilDelay(CartIndex);

            var wet = ToilMaker.MakeToil();
            wet.initAction = () =>
            {
                var cart = Cart.TryGetComp<CompJanitorCart>();
                var mop = pawn.equipment?.Primary?.TryGetComp<CompMopCharge>();
                if (cart == null || mop == null || !cart.Water.TryUse()) return;
                mop.Wetness.Refill();
                cart.MarkUsed();
                JanitorCarts.QueueClean(pawn, job.targetQueueB);
            };
            yield return wet;
        }
    }
}
