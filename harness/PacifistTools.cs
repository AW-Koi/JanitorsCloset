namespace JanitorsCloset.Scenarios;

/// <summary>A colonist incapable of violence can equip a mop from the right-click menu but still can't equip a club.</summary>
[Mod("../mod", Build = "dotnet build Source/JanitorsCloset/JanitorsCloset.csproj --source https://api.nuget.org/v3/index.json", BuildIn = "..")]
public sealed class PacifistTools : HarnessScenario
{
    public override async Task Run()
    {
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room(World.Center, width: 8, height: 7, floor: "WoodPlankFloor", roof: true);
        var pacifist = Spawn.Colonist("Pacifist", inside: room, work: [WorkTypeDefOf.Cleaning], incapableOf: WorkTags.Violent);
        var mopCell = room.Center + IntVec3.East;
        var clubCell = room.Center + IntVec3.West * 2;
        Spawn.Thing("Janitor_Mop", mopCell, stuff: "WoodLog");
        Spawn.Thing("MeleeWeapon_Club", clubCell, stuff: "WoodLog");

        Require(pacifist.WorkTagIsDisabled(WorkTags.Violent));
        Check(pacifist.FloatMenuAt(mopCell).Any(o => !o.Disabled && o.Label.StartsWith("Equip")));
        Check(!pacifist.FloatMenuAt(clubCell).Any(o => !o.Disabled && o.Label.StartsWith("Equip")));

        Camera.Frame(room);
        await Shot("room");
    }
}
