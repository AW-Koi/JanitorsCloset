using JanitorsCloset.Cleaning;

namespace JanitorsCloset.Scenarios;

/// <summary>Vanilla filth sorts into wet and dry the way a janitor would reach for a mop or a broom.</summary>
[Mod("../mod", Build = "dotnet build Source/JanitorsCloset/JanitorsCloset.csproj --source https://api.nuget.org/v3/index.json", BuildIn = "..")]
public sealed class FilthCategories : HarnessScenario
{
    public override Task Run()
    {
        Check(FilthCategoryResolver.Resolve(ThingDefOf.Filth_Blood) == CleaningCategory.Wet);
        Check(FilthCategoryResolver.Resolve(ThingDefOf.Filth_Vomit) == CleaningCategory.Wet);
        Check(FilthCategoryResolver.Resolve(ThingDefOf.Filth_Dirt) == CleaningCategory.Dry);
        Check(FilthCategoryResolver.Resolve(ThingDefOf.Filth_Ash) == CleaningCategory.Dry);
        Check(FilthCategoryResolver.Resolve(ThingDefOf.Filth_RubbleRock) == CleaningCategory.Dry);
        return Task.CompletedTask;
    }
}
