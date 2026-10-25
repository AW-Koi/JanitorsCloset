using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.FreshRooms;

/// <summary>A glittervacuum clears a cluster of filth in one pass, and the room counts as freshly cleaned.</summary>
public sealed class GlittervacuumStampsRoom : HarnessScenario
{
    public override async Task Run()
    {
        // A lone janitor with a glittervacuum in a clean room.
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

        // Three dirt patches inside one 3x3 field.
        Spawn.Filth(ThingDefOf.Filth_Dirt, room.Center);
        Spawn.Filth(ThingDefOf.Filth_Dirt, room.Center + IntVec3.East);
        Spawn.Filth(ThingDefOf.Filth_Dirt, room.Center + IntVec3.North);


        // Once the vacuum clears them, the room is stamped fresh.
        Clock.Speed = TimeSpeed.Superfast;
        await Until(() => room.Filth().Count == 0, timeoutSeconds: 120);
        Clock.Speed = TimeSpeed.Paused;

        var freshRooms = World.Map.GetComponent<MapComponent_FreshlyCleanedRooms>();
        Check(freshRooms.IsRoomStamped(room.Center.GetRoom(World.Map)));
    }
}
