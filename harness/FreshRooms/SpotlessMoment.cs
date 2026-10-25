using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.FreshRooms;

/// <summary>A janitor mops the last blood out of a room: the room is stamped and the janitor gains the spotless thought.</summary>
public sealed class SpotlessMoment : HarnessScenario
{
    public override async Task Run()
    {
        // A lone janitor with a mop in a room splashed with blood.
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room()
            .Size(8, 7)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .WithDoor(Rot4.South)
            .Roofed()
            .Build();
        var janitor = Spawn.Colonist()
            .WithName("Janitor")
            .Inside(room)
            .Doing(WorkTypeDefOf.Cleaning)
            .Spawn();
        Spawn.Thing(JanitorDefOf.Janitor_Mop)
            .MadeOf(ThingDefOf.WoodLog)
            .GivenTo(janitor)
            .Spawn();
        Colonists.RemoveAllExcept(janitor);
        room.Scatter(ThingDefOf.Filth_Blood, 6, seed: 3);
        Camera.Follow(janitor, zoom: 11f);

        // Fast-forward to the last stain, then watch it go at normal speed.
        Clock.Speed = TimeSpeed.Superfast;
        await Until(() => Mess(room) <= 1, timeoutSeconds: 120);
        Clock.Speed = TimeSpeed.Normal;
        await Until(() => Mess(room) == 0, timeoutSeconds: 60);

        // Screenshots of the glint and shimmer for the report.
        await Shot("spotless-0");
        await Ticks(20);
        await Shot("spotless-1");
        await Ticks(40);
        await Shot("spotless-2");

        // The room is stamped fresh and the janitor feels proud.
        var freshRooms = World.Map.GetComponent<MapComponent_FreshlyCleanedRooms>();
        Check(freshRooms.IsRoomStamped(room.Center.GetRoom(World.Map)));
        Check(janitor.HasThought(JanitorDefOf.Janitor_SpotlessWork));
    }

    private static int Mess(HarnessRoom room) => room.Filth().Count(f => !MarkerFilth.IsMarker(f));
}
