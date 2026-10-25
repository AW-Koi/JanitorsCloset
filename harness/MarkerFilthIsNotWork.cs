using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios;

/// <summary>A mop mark in the home area stays off the list of filth that idle cleaners scan, even after the home area is redrawn.</summary>
[Mod("../mod", Build = "dotnet build Source/JanitorsCloset/JanitorsCloset.csproj --source https://api.nuget.org/v3/index.json", BuildIn = "..")]
public sealed class MarkerFilthIsNotWork : HarnessScenario
{
    public override async Task Run()
    {
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room()
            .Size(6, 6)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .Roofed()
            .Build();
        var home = World.Map.areaManager.Home;
        foreach (var cell in room.Interior) home[cell] = true;
        var mark = Spawn.Filth(JanitorDefOf.Janitor_MopMark, room.Center);
        var dirt = Spawn.Filth(ThingDefOf.Filth_Dirt, room.Center + IntVec3.East);
        Require(mark != null && dirt != null);

        var homeFilth = World.Map.listerFilthInHomeArea.FilthInHomeArea;
        Check(!homeFilth.Contains(mark));
        Check(homeFilth.Contains(dirt));

        home[room.Center] = false;
        home[room.Center] = true;
        Clock.Speed = TimeSpeed.Normal;
        await Ticks(1);
        Check(!homeFilth.Contains(mark));
    }
}
