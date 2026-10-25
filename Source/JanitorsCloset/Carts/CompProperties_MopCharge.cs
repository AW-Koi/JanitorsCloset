using Verse;

namespace JanitorsCloset.Carts
{
    public class CompProperties_MopCharge : CompProperties
    {
        public int charges = 8;

        public CompProperties_MopCharge()
        {
            compClass = typeof(CompMopCharge);
        }
    }
}
