using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios;

/// <summary>A glittervacuum clears a cluster of filth in one pass, and the room counts as freshly cleaned.</summary>
[Mod("../mod", Build = "dotnet build Source/JanitorsCloset/JanitorsCloset.csproj --source https://api.nuget.org/v3/index.json", BuildIn = "..")]
public sealed class GlittervacuumStampsRoom : HarnessScenario
{
    public override async Task Run()
    {
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room()
            .Size(7, 7)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .WithDoor(Rot4.South)
            .Roofed()
            .Build();
        var janitor = Spawn.Colonist()
            .WithName("Vacuumer")
            .Inside(room)
            .Doing(WorkTypeDefOf.Cleaning)
            .Spawn();
        Spawn.Thing(JanitorDefOf.Janitor_Glittervacuum)
            .GivenTo(janitor)
            .Spawn();
        Colonists.RemoveAllExcept(janitor);
        foreach (var filth in room.Filth()) filth.Destroy();
        Spawn.Filth(ThingDefOf.Filth_Dirt, room.Center);
        Spawn.Filth(ThingDefOf.Filth_Dirt, room.Center + IntVec3.East);
        Spawn.Filth(ThingDefOf.Filth_Dirt, room.Center + IntVec3.North);

        Clock.Speed = TimeSpeed.Superfast;
        await Until(() => room.Filth().Count == 0, timeoutSeconds: 120);
        Clock.Speed = TimeSpeed.Paused;

        var freshRooms = World.Map.GetComponent<MapComponent_FreshlyCleanedRooms>();
        Check(freshRooms.IsRoomStamped(room.Center.GetRoom(World.Map)));
    }
}
