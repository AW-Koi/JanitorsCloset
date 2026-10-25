using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Carts
{
    /// <summary>Wheels the cart to a parking cell near the mess, then queues wetting the mop or the clean itself.</summary>
    public class JobDriver_PushCart : JobDriver
    {
        private const TargetIndex CartIndex = TargetIndex.A;
        private const TargetIndex ParkingIndex = TargetIndex.B;

        private readonly CartPushing pushing = new CartPushing();

        private Thing Cart => job.GetTarget(CartIndex).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Cart, job, 1, -1, null, errorOnFailed)
                && pawn.Reserve(job.GetTarget(ParkingIndex), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(CartIndex);
            yield return Toils_Goto.GotoThing(CartIndex, PathEndMode.ClosestTouch).FailOnForbidden(CartIndex);
            yield return pushing.TakeHold(CartIndex);
            yield return Toils_Goto.GotoCell(ParkingIndex, PathEndMode.OnCell);

            var park = ToilMaker.MakeToil();
            park.initAction = () =>
            {
                var parked = CartPushing.Park(pawn, job.GetTarget(ParkingIndex).Cell);
                if (parked != null) JanitorCarts.QueueAfterParking(pawn, parked, job.targetQueueB);
            };
            yield return park;
        }

        public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool flip)
        {
            return pushing.ModifyDrawPos(pawn, ref drawPos);
        }
    }
}
