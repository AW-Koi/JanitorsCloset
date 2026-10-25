using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Assignments;

/// <summary>A mop lying on the floor tells the player its cleaning assignment when inspected.</summary>
public sealed class ToolInspectShowsAssignment : HarnessScenario
{
    public override Task Run()
    {
        // A mop on the ground, assigned to indoor work in the home area.
        Clock.Speed = TimeSpeed.Paused;
        var home = World.Map.areaManager.Home;
        var mop = Spawn.Thing(JanitorDefOf.Janitor_Mop).MadeOf(ThingDefOf.WoodLog).At(World.Center).Spawn();
        var assignment = mop.TryGetComp<CompCleaningAssignment>();
        assignment.assignment = CleaningAreaAssignment.Indoors;
        assignment.AssignArea(home);

        // Its inspect pane names both restrictions.
        var expected = "JanitorsCloset.CleaningAssignment.Inspect".Translate(
            "JanitorsCloset.CleaningAssignment.Inspect.Indoors".Translate(), home.Label);
        Check(mop.GetInspectString().Contains(expected));
        return Task.CompletedTask;
    }
}
