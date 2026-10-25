using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Tools;

/// <summary>Cleaning tools sit in their own stockpile category under weapons, apart from melee weapons.</summary>
public sealed class CleaningToolsHaveOwnCategory : HarnessScenario
{
    public override Task Run()
    {
        // Every tool is in the cleaning-tools category and nowhere else.
        var tools = new[] { JanitorDefOf.Janitor_StrawBroom, JanitorDefOf.Janitor_PushBroom, JanitorDefOf.Janitor_Mop, JanitorDefOf.Janitor_Glittervacuum };
        foreach (var tool in tools)
        {
            Check(tool.thingCategories.SequenceEqual(new[] { JanitorDefOf.Janitor_CleaningTools }), tool.defName);
        }

        // The category lives under weapons, so existing weapon stockpiles still take tools.
        Check(JanitorDefOf.Janitor_CleaningTools.parent == ThingCategoryDefOf.Weapons);
        Check(!ScenarioDefOf.WeaponsMelee.DescendantThingDefs.Contains(JanitorDefOf.Janitor_Mop));
        return Task.CompletedTask;
    }
}
