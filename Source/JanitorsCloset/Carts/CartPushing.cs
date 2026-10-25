using UnityEngine;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Carts
{
    /// <summary>Takes hold of a cart, draws it rolling ahead of the pawn and parks it; one per job that pushes a cart.</summary>
    public class CartPushing
    {
        private const float LeadDistance = 0.75f;
        private const float SideLeadDistance = 1f;
        private const float SideDrop = 0.12f;
        private const float SmoothTime = 0.25f;

        private Vector3 offset;
        private Vector3 velocity;

        public Toil TakeHold(TargetIndex cartIndex)
        {
            var toil = ToilMaker.MakeToil();
            toil.initAction = () =>
            {
                var pawn = toil.actor;
                var cart = pawn.jobs.curJob.GetTarget(cartIndex).Thing;
                cart.TryGetComp<CompJanitorCart>()?.Claim(pawn);
                if (pawn.carryTracker.TryStartCarry(cart, 1) == 0) pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                offset = Lead(pawn.Rotation);
                velocity = Vector3.zero;
            };
            return toil;
        }

        /// <summary>Sets the cart down on the cell, facing the way the pawn walked; returns the parked cart or null.</summary>
        public static Thing Park(Pawn pawn, IntVec3 cell)
        {
            var facing = pawn.Rotation;
            if (!pawn.carryTracker.TryDropCarriedThing(cell, ThingPlaceMode.Direct, out var parked)
                && !pawn.carryTracker.TryDropCarriedThing(cell, ThingPlaceMode.Near, out parked)) return null;
            if (parked == null) return null;
            parked.Rotation = facing;
            parked.TryGetComp<CompJanitorCart>()?.MarkUsed();
            return parked;
        }

        public bool ModifyDrawPos(Pawn pawn, ref Vector3 drawPos)
        {
            var cart = pawn.carryTracker.CarriedThing;
            if (cart == null || cart.TryGetComp<CompJanitorCart>() == null) return false;

            var target = Lead(pawn.Rotation);
            if (!Find.TickManager.Paused)
            {
                offset = Vector3.SmoothDamp(offset, target, ref velocity, SmoothTime, 10f, Find.TickManager.TickRateMultiplier / 60f);
            }
            var pawnPos = pawn.DrawPos;
            drawPos.x = pawnPos.x + offset.x;
            drawPos.z = pawnPos.z + offset.z;
            if (pawn.Rotation == Rot4.North) drawPos.y = pawnPos.y + PawnRenderUtility.AltitudeForLayer(-10f);
            cart.Rotation = pawn.Rotation;
            return true;
        }

        // Sideways, the cart sits further out and lower, so the handle meets the hands and the wheels the feet.
        private static Vector3 Lead(Rot4 facing)
        {
            if (facing.IsHorizontal) return facing.FacingCell.ToVector3() * SideLeadDistance + new Vector3(0f, 0f, -SideDrop);
            return facing.FacingCell.ToVector3() * LeadDistance;
        }
    }
}
