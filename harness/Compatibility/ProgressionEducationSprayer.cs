using HarmonyLib;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Compatibility;

/// <summary>With Progression: Education, the hazmat sprayer needs low-tech proficiency rather than firearms.</summary>
/// <remarks>PE's types aren't referenced at build time, so the check reads its extension by type name.</remarks>
[RequiresMod("ferny.ProgressionEducation")]
[Dlc("biotech")]
public sealed class ProgressionEducationSprayer : HarnessScenario
{
    private const string ProficiencyRequirementType = "ProgressionEducation.ItemProficiencyRequirement";

    public override Task Run()
    {
        // The sprayer carries PE's proficiency requirement.
        var requirement = JanitorDefOf.Janitor_HazmatSprayer.modExtensions?
            .SingleOrDefault(extension => extension.GetType().FullName == ProficiencyRequirementType);
        Require(requirement != null);

        // It asks for PE's low-tech proficiency.
        var proficiency = Traverse.Create(requirement).Field("requiredProficiency").GetValue<Def>();
        Check(proficiency == DefDatabase<TraitDef>.GetNamed("PE_LowTechProficiency"));
        return Task.CompletedTask;
    }
}
