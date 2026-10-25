using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios;

/// <summary>A forced "Prioritize cleaning" on blood keeps the blood queued, even when the broom prefers nearby dirt.</summary>
[Mod("../mod", Build = "dotnet build Source/JanitorsCloset/JanitorsCloset.csproj --source https://api.nuget.org/v3/index.json", BuildIn = "..")]
public sealed class ForcedCleanKeepsTarget : HarnessScenario
{
    public override async Task Run()
    {
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room()
            .Size(8, 7)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .WithDoor(Rot4.South)
            .Roofed()
            .Build();
        var janitor = Spawn.Colonist()
            .WithName("Sweeper")
            .Inside(room)
            .Spawn();
        Spawn.Thing(JanitorDefOf.Janitor_PushBroom)
            .MadeOf(ThingDefOf.WoodLog)
            .GivenTo(janitor)
            .Spawn();
        var bloodCell = room.Center + IntVec3.East;
        var blood = Spawn.Filth(ThingDefOf.Filth_Blood, bloodCell);
        Spawn.Filth(ThingDefOf.Filth_Dirt, bloodCell + IntVec3.North);
        Spawn.Filth(ThingDefOf.Filth_Dirt, bloodCell + IntVec3.South);
        // Vanilla only queues filth that has sat for 600 ticks.
        Clock.Speed = TimeSpeed.Superfast;
        await Ticks(700);
        Clock.Speed = TimeSpeed.Paused;

        var cleaning = (WorkGiver_Scanner)ScenarioDefOf.CleanFilth.Worker;
        var job = cleaning.JobOnThing(janitor, blood, forced: true);

        Require(job != null);
        Check(job?.targetQueueA?.Any(target => target.Thing == blood) == true);
    }
}
