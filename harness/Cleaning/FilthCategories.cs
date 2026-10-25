using JanitorsCloset.Cleaning;

namespace JanitorsCloset.Scenarios.Cleaning;

/// <summary>Vanilla filth sorts into wet and dry the way a janitor would reach for a mop or a broom.</summary>
public sealed class FilthCategories : HarnessScenario
{
    public override Task Run()
    {
        // Wet: anything that pools or smears.
        Check(FilthCategoryResolver.Resolve(ThingDefOf.Filth_Blood) == CleaningCategory.Wet);
        Check(FilthCategoryResolver.Resolve(ThingDefOf.Filth_Vomit) == CleaningCategory.Wet);

        // Dry: anything a broom pushes, including machine bits despite their fluid cleaning sound.
        Check(FilthCategoryResolver.Resolve(ThingDefOf.Filth_Dirt) == CleaningCategory.Dry);
        Check(FilthCategoryResolver.Resolve(ThingDefOf.Filth_MachineBits) == CleaningCategory.Dry);
        Check(FilthCategoryResolver.Resolve(ThingDefOf.Filth_Ash) == CleaningCategory.Dry);
        Check(FilthCategoryResolver.Resolve(ThingDefOf.Filth_RubbleRock) == CleaningCategory.Dry);
        return Task.CompletedTask;
    }
}
