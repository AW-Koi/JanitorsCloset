using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios;

/// <summary>A janitor mops the last blood out of a room: the room is stamped and the janitor gains the spotless thought.</summary>
[Mod("../mod", Build = "dotnet build Source/JanitorsCloset/JanitorsCloset.csproj --source https://api.nuget.org/v3/index.json", BuildIn = "..")]
public sealed class SpotlessMoment : HarnessScenario
{
    public override async Task Run()
    {
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room(World.Center, width: 8, height: 7, floor: TerrainDefOf.WoodPlankFloor, roof: true);
        var janitor = Spawn.Colonist("Janitor", inside: room, work: [WorkTypeDefOf.Cleaning]);
        janitor.Equip(JanitorDefOf.Janitor_Mop, stuff: ThingDefOf.WoodLog);
        Colonists.RemoveAllExcept(janitor);
        room.Scatter(ThingDefOf.Filth_Blood, 6, seed: 3);
        Camera.Follow(janitor, zoom: 11f);

        Clock.Speed = TimeSpeed.Superfast;
        await Until(() => Mess(room) <= 1, timeoutSeconds: 120);
        Clock.Speed = TimeSpeed.Normal;
        await Until(() => Mess(room) == 0, timeoutSeconds: 60);

        await Shot("spotless-0");
        await Ticks(20);
        await Shot("spotless-1");
        await Ticks(40);
        await Shot("spotless-2");

        var freshRooms = World.Map.GetComponent<MapComponent_FreshlyCleanedRooms>();
        Check(freshRooms.IsRoomStamped(room.Center.GetRoom(World.Map)));
        Check(janitor.HasThought(JanitorDefOf.Janitor_SpotlessWork));
    }

    private static int Mess(HarnessRoom room) => room.Filth().Count(f => !MarkerFilth.IsMarker(f));
}
