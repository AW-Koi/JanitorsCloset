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
        var room = Build.Room(World.Center, width: 10, height: 8, floor: TerrainDefOf.WoodPlankFloor, roof: true);
        var sprayer = Spawn.Colonist("Sprayer", inside: room, work: [WorkTypeDefOf.Cleaning]);
        sprayer.Equip(JanitorDefOf.Janitor_HazmatSprayer);
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
