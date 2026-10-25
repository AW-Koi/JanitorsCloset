using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Carts
{
    /// <summary>Wheels the cart home beside the utility sink, then empties and refills its bucket and wets the mop.</summary>
    public class JobDriver_ReturnCart : JobDriver
    {
        private const TargetIndex CartIndex = TargetIndex.A;
        private const TargetIndex SinkIndex = TargetIndex.B;
        private const TargetIndex ParkingIndex = TargetIndex.C;
        private const int RefillTicks = 300;

        private readonly CartPushing pushing = new CartPushing();

        private Thing Cart => job.GetTarget(CartIndex).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Cart, job, 1, -1, null, errorOnFailed)
                && pawn.Reserve(job.GetTarget(SinkIndex), job, 1, -1, null, errorOnFailed)
                && pawn.Reserve(job.GetTarget(ParkingIndex), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(CartIndex);
            this.FailOnDespawnedOrNull(SinkIndex);
            yield return Toils_Goto.GotoThing(CartIndex, PathEndMode.ClosestTouch).FailOnForbidden(CartIndex);
            yield return pushing.TakeHold(CartIndex);
            yield return Toils_Goto.GotoCell(ParkingIndex, PathEndMode.OnCell);

            var park = ToilMaker.MakeToil();
            park.initAction = () =>
            {
                if (CartPushing.Park(pawn, job.GetTarget(ParkingIndex).Cell) == null) EndJobWith(JobCondition.Incompletable);
            };
            yield return park;

            yield return Toils_Goto.GotoThing(SinkIndex, PathEndMode.Touch);
            yield return Toils_General.Wait(RefillTicks, SinkIndex).WithProgressBarToilDelay(SinkIndex);

            var refill = ToilMaker.MakeToil();
            refill.initAction = () =>
            {
                var cart = Cart.TryGetComp<CompJanitorCart>();
                cart?.Water.Refill();
                cart?.MarkUsed();
                pawn.equipment?.Primary?.TryGetComp<CompMopCharge>()?.Wetness.Refill();
            };
            yield return refill;
        }

        public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool flip)
        {
            return pushing.ModifyDrawPos(pawn, ref drawPos);
        }
    }
}
