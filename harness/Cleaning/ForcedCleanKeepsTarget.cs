using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Cleaning;

/// <summary>A forced "Prioritize cleaning" on blood keeps the blood queued, even when the broom prefers nearby dirt.</summary>
public sealed class ForcedCleanKeepsTarget : HarnessScenario
{
    public override async Task Run()
    {
        // A janitor with a push broom, which prefers dry filth.
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

        // Blood flanked by dirt, so the broom has nearer dry work to prefer.
        var bloodCell = room.Center + IntVec3.East;
        var blood = Spawn.Filth(ThingDefOf.Filth_Blood, bloodCell);
        Spawn.Filth(ThingDefOf.Filth_Dirt, bloodCell + IntVec3.North);
        Spawn.Filth(ThingDefOf.Filth_Dirt, bloodCell + IntVec3.South);
        // Age the filth past vanilla's 600-tick wait before it counts as work.
        Clock.Speed = TimeSpeed.Superfast;
        await Ticks(700);
        Clock.Speed = TimeSpeed.Paused;

        // A forced order on the blood keeps the blood in the job's queue.
        var cleaning = (WorkGiver_Scanner)ScenarioDefOf.CleanFilth.Worker;
        var job = cleaning.JobOnThing(janitor, blood, forced: true);

        Require(job != null);
        Check(job?.targetQueueA?.Any(target => target.Thing == blood) == true);
    }
}
