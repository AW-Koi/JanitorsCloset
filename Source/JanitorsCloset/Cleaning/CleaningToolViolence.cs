using Verse;

namespace JanitorsCloset.Cleaning
{
    // Treats cleaning tools as tools, not weapons, wherever vanilla bars pawns incapable of violence.
    public static class CleaningToolViolence
    {
        public static bool IsViolentWeapon(ThingDef def)
        {
            return def.IsWeapon && !def.HasModExtension<CleaningToolExtension>();
        }

        public static ThingWithComps ViolentPrimary(Pawn_EquipmentTracker equipment)
        {
            var primary = equipment.Primary;
            return primary != null && primary.def.HasModExtension<CleaningToolExtension>() ? null : primary;
        }
    }
}
