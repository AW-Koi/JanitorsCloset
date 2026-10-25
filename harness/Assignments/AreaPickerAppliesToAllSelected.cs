using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Assignments;

/// <summary>With two janitors selected, picking a cleaning area from their shared gizmo restricts both tools.</summary>
public sealed class AreaPickerAppliesToAllSelected : HarnessScenario
{
    public override Task Run()
    {
        // Two colonists side by side, one with a mop and one with a push broom.
        Clock.Speed = TimeSpeed.Paused;
        var mopper = Spawn.Colonist().WithName("Mopper").At(World.Center).Spawn();
        var sweeper = Spawn.Colonist().WithName("Sweeper").At(World.Center + IntVec3.East).Spawn();
        var mop = Spawn.Thing(JanitorDefOf.Janitor_Mop).MadeOf(ThingDefOf.WoodLog).GivenTo(mopper).Spawn();
        var broom = Spawn.Thing(JanitorDefOf.Janitor_PushBroom).MadeOf(ThingDefOf.WoodLog).GivenTo(sweeper).Spawn();
        var home = World.Map.areaManager.Home;

        // Their area gizmos merge into one button, as when both are selected.
        var areaGizmos = mopper.equipment.GetGizmos()
            .Concat(sweeper.equipment.GetGizmos())
            .OfType<CleaningAreaCommand>()
            .ToList<Gizmo>();
        Require(areaGizmos.Count == 2);
        Check(areaGizmos[0].GroupsWith(areaGizmos[1]));

        // Picking the home area from the merged button restricts both tools.
        var options = ((CleaningAreaCommand)areaGizmos[0]).AreaOptions(areaGizmos);
        options.Single(option => option.Label == home.Label).action();
        Check(mop.TryGetComp<CompCleaningAssignment>().ResolveArea(World.Map) == home);
        Check(broom.TryGetComp<CompCleaningAssignment>().ResolveArea(World.Map) == home);
        return Task.CompletedTask;
    }
}
