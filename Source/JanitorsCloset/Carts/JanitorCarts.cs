using System.Collections.Generic;
using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Carts
{
    /// <summary>Finds a janitor's cart and home sink, and turns a clean job into the cart errand that comes first.</summary>
    public static class JanitorCarts
    {
        private const float HomeRadius = 3f;

        public static bool TryMakeErrand(Pawn pawn, Job cleanJob, out Job errand)
        {
            errand = null;
            var targets = cleanJob.targetQueueA;
            if (targets == null || targets.Count == 0) return false;
            var tool = pawn.equipment?.Primary?.def.GetModExtension<CleaningToolExtension>();
            if (tool?.refillsAtCart != true) return false;

            var cart = FindCartFor(pawn);
            var cartComp = cart?.TryGetComp<CompJanitorCart>();
            if (cartComp == null) return false;

            var anchor = targets[0].Cell;
            var sink = FindHome(pawn);
            var situation = new CartSituation
            {
                MessSuitsTool = SuitsTool(targets, tool),
                MopSpent = MopSpent(pawn),
                CartDry = cartComp.Water.Empty,
                CartFarFromMess = CartPushRule.NeedsPush(cart.GetRoom() == anchor.GetRoom(pawn.Map), cart.Position.DistanceTo(anchor)),
                HasHome = sink != null,
            };

            switch (CartErrands.Next(situation))
            {
                case CartErrand.ReturnHome:
                    errand = MakeReturnJob(pawn, cart, sink);
                    break;
                case CartErrand.Push:
                    var parking = CartParking.Choose(CartParkingSurvey.Around(pawn, cart, anchor, Centre(targets)));
                    if (parking == null) return false;
                    errand = JobMaker.MakeJob(JanitorDefOf.Janitor_PushCart, cart, parking.Value);
                    errand.count = 1;
                    errand.targetQueueB = new List<LocalTargetInfo>(targets);
                    break;
                case CartErrand.Dunk:
                    errand = MakeDunkJob(cart, targets);
                    break;
            }
            return errand != null;
        }

        /// <summary>A job wheeling the cart beside the sink to refill it, or null when there's no free cell there.</summary>
        public static Job MakeReturnJob(Pawn pawn, Thing cart, Thing sink)
        {
            var parking = IsHome(cart, sink)
                ? cart.Position
                : CartParking.Choose(CartParkingSurvey.Around(pawn, cart, sink.Position, sink.Position));
            if (parking == null) return null;
            var job = JobMaker.MakeJob(JanitorDefOf.Janitor_ReturnCart, cart, sink, parking.Value);
            job.count = 1;
            return job;
        }

        public static void QueueAfterParking(Pawn pawn, Thing cart, List<LocalTargetInfo> targets)
        {
            var cartComp = cart.TryGetComp<CompJanitorCart>();
            if (MopSpent(pawn) && cartComp != null && !cartComp.Water.Empty)
            {
                pawn.jobs.jobQueue.EnqueueFirst(MakeDunkJob(cart, targets), JobTag.MiscWork);
                return;
            }
            QueueClean(pawn, targets);
        }

        public static void QueueClean(Pawn pawn, List<LocalTargetInfo> targets)
        {
            if (targets == null || targets.Count == 0) return;
            var clean = JobMaker.MakeJob(JobDefOf.Clean);
            for (int i = 0; i < targets.Count; i++) clean.AddQueuedTarget(TargetIndex.A, targets[i]);
            pawn.jobs.jobQueue.EnqueueFirst(clean, JobTag.MiscWork);
        }

        public static Thing FindCartFor(Pawn pawn)
        {
            var carts = pawn.Map.listerThings.ThingsOfDef(JanitorDefOf.Janitor_Cart);
            Thing best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < carts.Count; i++)
            {
                var cart = carts[i];
                var comp = cart.TryGetComp<CompJanitorCart>();
                if (comp == null || !comp.AvailableTo(pawn) || cart.IsForbidden(pawn)) continue;
                if (!pawn.CanReserveAndReach(cart, PathEndMode.ClosestTouch, Danger.Some)) continue;
                if (comp.Owner == pawn) return cart;
                float distance = cart.Position.DistanceToSquared(pawn.Position);
                if (distance < bestDistance)
                {
                    best = cart;
                    bestDistance = distance;
                }
            }
            return best;
        }

        /// <summary>The nearest utility sink the pawn can reach and reserve.</summary>
        public static Thing FindHome(Pawn pawn)
        {
            var sinks = pawn.Map.listerThings.ThingsOfDef(JanitorDefOf.Janitor_UtilitySink);
            Thing best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < sinks.Count; i++)
            {
                var sink = sinks[i];
                if (sink.IsForbidden(pawn) || !pawn.CanReserveAndReach(sink, PathEndMode.Touch, Danger.Some)) continue;
                float distance = sink.Position.DistanceToSquared(pawn.Position);
                if (distance < bestDistance)
                {
                    best = sink;
                    bestDistance = distance;
                }
            }
            return best;
        }

        public static bool IsHome(Thing cart, Thing sink)
        {
            return cart.GetRoom() == sink.GetRoom() && cart.Position.DistanceTo(sink.Position) <= HomeRadius;
        }

        private static bool SuitsTool(List<LocalTargetInfo> targets, CleaningToolExtension tool)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (!(targets[i].Thing is Filth filth)) continue;
                var category = FilthCategoryResolver.Resolve(filth.def);
                if (category != null && tool.Matches(category.Value)) return true;
            }
            return false;
        }

        private static bool MopSpent(Pawn pawn)
        {
            return pawn.equipment?.Primary?.TryGetComp<CompMopCharge>()?.Wetness.Empty == true;
        }

        private static Job MakeDunkJob(Thing cart, List<LocalTargetInfo> targets)
        {
            var job = JobMaker.MakeJob(JanitorDefOf.Janitor_DunkMop, cart);
            job.targetQueueB = new List<LocalTargetInfo>(targets);
            return job;
        }

        private static IntVec3 Centre(List<LocalTargetInfo> targets)
        {
            int x = 0, z = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                x += targets[i].Cell.x;
                z += targets[i].Cell.z;
            }
            return new IntVec3(x / targets.Count, 0, z / targets.Count);
        }
    }
}
