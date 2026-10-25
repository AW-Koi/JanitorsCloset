using JanitorsCloset.Cleaning;

namespace JanitorsCloset.Scenarios.Cleaning;

/// <summary>Fleshmass debris counts as wet filth for the mop, despite its dry cleaning sound.</summary>
[Dlc("anomaly")]
public sealed class FleshmassIsWet : HarnessScenario
{
    public override Task Run()
    {
        // Anomaly's fleshmass debris resolves to wet.
        Check(FilthCategoryResolver.Resolve(ScenarioDefOf.Filth_Fleshmass) == CleaningCategory.Wet);
        return Task.CompletedTask;
    }
}
