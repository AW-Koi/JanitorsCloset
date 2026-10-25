using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Tools;

/// <summary>With the camera far away, a hazmat sprayer still lays decontaminant foam while cleaning.</summary>
[Dlc("biotech")]
public sealed class SprayerOffscreen : HarnessScenario
{
    public override async Task Run()
    {
        // A lone cleaner with a hazmat sprayer in a dirty room.
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room()
            .Size(10, 8)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .WithDoor(Rot4.South)
            .Roofed()
            .Build();
        var sprayer = Spawn.Colonist()
            .WithName("Sprayer")
            .Inside(room)
            .Doing(WorkTypeDefOf.Cleaning)
            .Spawn();
        Spawn.Thing(JanitorDefOf.Janitor_HazmatSprayer)
            .GivenTo(sprayer)
            .Spawn();
        Colonists.RemoveAllExcept(sprayer);
        room.Scatter(ThingDefOf.Filth_Dirt, 16, seed: 5);


        // With the camera far away, the sprayer still leaves foam behind it.
        Camera.Look(new IntVec3(20, 0, 20), zoom: 60f);
        Clock.Speed = TimeSpeed.Normal;
        await Until(() => World.Things(JanitorDefOf.Janitor_HazmatFoam).Count >= 3, timeoutSeconds: 90);

        Note("foam", World.Things(JanitorDefOf.Janitor_HazmatFoam).Count);

        // A screenshot of the foam for the report.
        Camera.Follow(sprayer);
        await Shot("foam");
    }
}
