using Verse;

namespace JanitorsCloset.Carts
{
    public class CompProperties_JanitorCart : CompProperties
    {
        public int dunks = 4;

        public CompProperties_JanitorCart()
        {
            compClass = typeof(CompJanitorCart);
        }
    }
}
