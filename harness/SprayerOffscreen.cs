using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios;

/// <summary>With the camera far away, a hazmat sprayer still lays decontaminant foam while cleaning.</summary>
[Mod("../mod", Build = "dotnet build Source/JanitorsCloset/JanitorsCloset.csproj --source https://api.nuget.org/v3/index.json", BuildIn = "..")]
[Dlc("biotech")]
public sealed class SprayerOffscreen : HarnessScenario
{
    public override async Task Run()
    {
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

        Camera.Look(new IntVec3(20, 0, 20), zoom: 60f);
        Clock.Speed = TimeSpeed.Normal;
        await Until(() => World.Things(JanitorDefOf.Janitor_HazmatFoam).Count >= 3, timeoutSeconds: 90);

        Note("foam", World.Things(JanitorDefOf.Janitor_HazmatFoam).Count);
        Camera.Follow(sprayer);
        await Shot("foam");
    }
}
