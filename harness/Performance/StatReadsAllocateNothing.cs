using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Performance;

/// <summary>Reading a cleaning janitor's work-speed stats, which vanilla does every work tick, allocates nothing.</summary>
public sealed class StatReadsAllocateNothing : HarnessScenario
{
    private const int Reads = 1000;

    public override async Task Run()
    {
        // A janitor with a push broom sweeping blood, so the mismatch penalty is live.
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room()
            .Size(6, 6)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .WithDoor(Rot4.South)
            .Roofed()
            .Build();
        var janitor = Spawn.Colonist().WithName("Sweeper").Inside(room).Doing(WorkTypeDefOf.Cleaning).Spawn();
        Spawn.Thing(JanitorDefOf.Janitor_PushBroom).MadeOf(ThingDefOf.WoodLog).GivenTo(janitor).Spawn();
        Colonists.RemoveAllExcept(janitor);
        Spawn.Filth(ThingDefOf.Filth_Blood, room.Center + IntVec3.East, thickness: 5);
        Clock.Speed = TimeSpeed.Normal;
        await Until(() => janitor.CurrentJob() == JobDefOf.Clean, timeoutSeconds: 60);
        Clock.Speed = TimeSpeed.Paused;

        // A thousand reads of each stat, after one warm-up read, allocate no bytes.
        var request = StatRequest.For(janitor);
        ReadStats(request, 1);
        var before = GC.GetAllocatedBytesForCurrentThread();
        ReadStats(request, Reads);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Note("bytes", allocated);
        Check(allocated == 0);
    }

    private static void ReadStats(StatRequest request, int times)
    {
        for (var i = 0; i < times; i++)
        {
            StatDefOf.CleaningSpeed.Worker.GetValue(request);
            StatDefOf.GeneralLaborSpeed.Worker.GetValue(request);
        }
    }
}
