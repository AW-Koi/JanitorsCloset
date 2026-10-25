// ReSharper disable InconsistentNaming
namespace JanitorsCloset.Scenarios;

/// <summary>Defs the scenarios use that neither RimWorld's nor the mod's DefOf classes cover.</summary>
[DefOf]
public static class ScenarioDefOf
{
    public static ThingDef MeleeWeapon_Club;
    public static WorkGiverDef CleanFilth;

    [MayRequireBiotech]
    public static GeneDef ViolenceDisabled;

    [MayRequireAnomaly]
    public static ThingDef Filth_Fleshmass;
}
