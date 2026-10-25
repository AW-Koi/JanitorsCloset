using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.FreshRooms;

/// <summary>A room a janitor made spotless is still freshly cleaned after the game is saved and loaded.</summary>
public sealed class StampSurvivesReload : HarnessScenario
{
    public override async Task Run()
    {
        // A lone janitor with a mop in a room splashed with blood.
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room()
            .Size(7, 7)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .WithDoor(Rot4.South)
            .Roofed()
            .Build();
        var janitor = Spawn.Colonist().WithName("Janitor").Inside(room).Doing(WorkTypeDefOf.Cleaning).Spawn();
        Spawn.Thing(JanitorDefOf.Janitor_Mop).MadeOf(ThingDefOf.WoodLog).GivenTo(janitor).Spawn();
        Colonists.RemoveAllExcept(janitor);
        room.Scatter(ThingDefOf.Filth_Blood, 4, seed: 7);

        // The janitor mops it spotless, which stamps the room.
        Clock.Speed = TimeSpeed.Superfast;
        await Until(() => IsStamped(room), timeoutSeconds: 120);
        Clock.Speed = TimeSpeed.Paused;

        // After a save and load, room IDs are new but the stamp is still there.
        await Reload();
        Check(IsStamped(room));
    }

    private static bool IsStamped(HarnessRoom room) =>
        World.Map.GetComponent<MapComponent_FreshlyCleanedRooms>().IsRoomStamped(room.Center.GetRoom(World.Map));
}
