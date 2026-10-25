using Verse;

namespace JanitorsCloset.Carts
{
    /// <summary>How many more wet messes the mop cleans at full speed before it needs wetting again.</summary>
    public class CompMopCharge : ThingComp
    {
        private Charges wetness;

        public CompProperties_MopCharge Props => (CompProperties_MopCharge)props;

        public Charges Wetness => wetness ??= new Charges(Props.charges);

        public override string CompInspectStringExtra()
        {
            return "JanitorsCloset.Mop.Wetness".Translate(Wetness.Remaining, Wetness.Capacity).Resolve();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref wetness, "wetness");
        }
    }
}
