// ReSharper disable InconsistentNaming
namespace JanitorsCloset.Scenarios;

/// <summary>Defs the scenarios use that neither RimWorld's nor the mod's DefOf classes cover.</summary>
[DefOf]
public static class ScenarioDefOf
{
    public static ThingDef MeleeWeapon_Club;
    public static WorkGiverDef CleanFilth;
    public static ThingCategoryDef WeaponsMelee;
    public static ThingDef CraftingSpot;
    public static ResearchProjectDef Housekeeping;
    public static RecipeDef Make_Janitor_StrawBroomFromWood;
    public static InspirationDef Janitor_CleaningFrenzy;

    [MayRequireBiotech]
    public static GeneDef ViolenceDisabled;

    [MayRequireAnomaly]
    public static ThingDef Filth_Fleshmass;
}
