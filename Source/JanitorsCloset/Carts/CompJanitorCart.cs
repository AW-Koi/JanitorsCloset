using Verse;

namespace JanitorsCloset.Carts
{
    /// <summary>The cart's owner, the water left in its bucket and when a janitor last used it.</summary>
    public class CompJanitorCart : ThingComp
    {
        private Pawn owner;
        private Charges water;
        private int lastUsedTick;

        public CompProperties_JanitorCart Props => (CompProperties_JanitorCart)props;

        public Pawn Owner
        {
            get
            {
                if (owner != null && (owner.Dead || owner.Destroyed)) owner = null;
                return owner;
            }
        }

        public Charges Water => water ??= new Charges(Props.dunks);

        public int TicksIdle => Find.TickManager.TicksGame - lastUsedTick;

        public bool AvailableTo(Pawn pawn) => Owner == null || Owner == pawn;

        public void Claim(Pawn pawn) => owner = pawn;

        public void MarkUsed() => lastUsedTick = Find.TickManager.TicksGame;

        public override string CompInspectStringExtra()
        {
            var waterLine = "JanitorsCloset.Cart.Water".Translate(Water.Remaining, Water.Capacity).Resolve();
            return Owner == null ? waterLine : waterLine + "\n" + "JanitorsCloset.Cart.Owner".Translate(Owner.LabelShort).Resolve();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref owner, "owner");
            Scribe_Deep.Look(ref water, "water");
            Scribe_Values.Look(ref lastUsedTick, "lastUsedTick");
        }
    }
}
