using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Cleaning;

/// <summary>A mop mark in the home area stays off the list of filth that idle cleaners scan, even after the home area is redrawn.</summary>
public sealed class MarkerFilthIsNotWork : HarnessScenario
{
    public override async Task Run()
    {
        // A floored room inside the home area.
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room()
            .Size(6, 6)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .Roofed()
            .Build();
        var home = World.Map.areaManager.Home;
        foreach (var cell in room.Interior) home[cell] = true;

        // A mop mark and some dirt: only the dirt joins the home-filth list.
        var mark = Spawn.Filth(JanitorDefOf.Janitor_MopMark, room.Center);
        var dirt = Spawn.Filth(ThingDefOf.Filth_Dirt, room.Center + IntVec3.East);
        Require(mark != null && dirt != null);

        var homeFilth = World.Map.listerFilthInHomeArea.FilthInHomeArea;
        Check(!homeFilth.Contains(mark));
        Check(homeFilth.Contains(dirt));


        // Redrawing the home area over the mark still leaves it off the list.
        home[room.Center] = false;
        home[room.Center] = true;
        Clock.Speed = TimeSpeed.Normal;
        await Ticks(1);
        Check(!homeFilth.Contains(mark));
    }
}
