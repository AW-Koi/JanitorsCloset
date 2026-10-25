using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Assignments;

/// <summary>Ordering an outdoors-only janitor to clean indoor filth fails with a reason the float menu can show.</summary>
public sealed class OutsideAssignmentGivesReason : HarnessScenario
{
    public override async Task Run()
    {
        // A janitor in a closed room, holding a push broom assigned to outdoor work.
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room()
            .Size(6, 6)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .WithDoor(Rot4.South)
            .Roofed()
            .Build();
        var janitor = Spawn.Colonist().WithName("Janitor").Inside(room).Spawn();
        var broom = Spawn.Thing(JanitorDefOf.Janitor_PushBroom).MadeOf(ThingDefOf.WoodLog).GivenTo(janitor).Spawn();
        broom.TryGetComp<CompCleaningAssignment>().assignment = CleaningAreaAssignment.Outdoors;

        // Dirt inside the room, aged past vanilla's 600-tick wait before it counts as work.
        var dirt = Spawn.Filth(ThingDefOf.Filth_Dirt, room.Center + IntVec3.East);
        Clock.Speed = TimeSpeed.Superfast;
        await Ticks(700);
        Clock.Speed = TimeSpeed.Paused;

        // A forced cleaning order on the dirt is refused, with the assignment as the reason.
        JobFailReason.Clear();
        var cleaning = (WorkGiver_Scanner)ScenarioDefOf.CleanFilth.Worker;
        var canClean = cleaning.HasJobOnThing(janitor, dirt, forced: true);
        Check(!canClean);
        Check(JobFailReason.Reason == "JanitorsCloset.CleaningAssignment.OutsideAssignment".Translate());
    }
}
