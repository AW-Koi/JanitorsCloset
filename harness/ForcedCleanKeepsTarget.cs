using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios;

/// <summary>A forced "Prioritize cleaning" on blood keeps the blood queued, even when the broom prefers nearby dirt.</summary>
[Mod("../mod", Build = "dotnet build Source/JanitorsCloset/JanitorsCloset.csproj --source https://api.nuget.org/v3/index.json", BuildIn = "..")]
public sealed class ForcedCleanKeepsTarget : HarnessScenario
{
    public override async Task Run()
    {
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room(World.Center, width: 8, height: 7, floor: TerrainDefOf.WoodPlankFloor, roof: true);
        var janitor = Spawn.Colonist("Sweeper", inside: room);
        janitor.Equip(JanitorDefOf.Janitor_PushBroom, stuff: ThingDefOf.WoodLog);
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
