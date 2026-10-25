namespace JanitorsCloset.Scenarios;

/// <summary>A forced "Prioritize cleaning" on blood keeps the blood queued, even when the broom prefers nearby dirt.</summary>
[Mod("../mod", Build = "dotnet build Source/JanitorsCloset/JanitorsCloset.csproj --source https://api.nuget.org/v3/index.json", BuildIn = "..")]
public sealed class ForcedCleanKeepsTarget : HarnessScenario
{
    public override async Task Run()
    {
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room(World.Center, width: 8, height: 7, floor: "WoodPlankFloor", roof: true);
        var janitor = Spawn.Colonist("Sweeper", inside: room);
        janitor.Equip("Janitor_PushBroom", stuff: "WoodLog");
        var bloodCell = room.Center + IntVec3.East;
        Spawn.Filth("Filth_Blood", bloodCell);
        Spawn.Filth("Filth_Dirt", bloodCell + IntVec3.North);
        Spawn.Filth("Filth_Dirt", bloodCell + IntVec3.South);
        // Vanilla only queues filth that has sat for 600 ticks.
        Clock.Speed = TimeSpeed.Superfast;
        await Ticks(700);
        Clock.Speed = TimeSpeed.Paused;

        var blood = World.Filth(room.Interior, "Filth_Blood").First();
        var cleaning = (WorkGiver_Scanner)DefDatabase<WorkGiverDef>.GetNamed("CleanFilth").Worker;
        var job = cleaning.JobOnThing(janitor, blood, forced: true);

        Require(job != null);
        Check(job?.targetQueueA?.Any(target => target.Thing == blood) == true);
    }
}
