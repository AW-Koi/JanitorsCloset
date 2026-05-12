using Verse;

namespace JanitorsCloset.Cleaning
{
    // Attached to a Filth ThingDef to tag it as wet or dry. Untagged filth is treated
    // as "matches any tool" — third-party modded filth therefore never gets penalised,
    // it just doesn't receive a specialty bonus from our tools.
    public class FilthCategoryExtension : DefModExtension
    {
        public CleaningCategory category;
    }
}
