using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios;

/// <summary>A colonist incapable of violence can equip a mop from the right-click menu but still can't equip a club.</summary>
[Mod("../mod", Build = "dotnet build Source/JanitorsCloset/JanitorsCloset.csproj --source https://api.nuget.org/v3/index.json", BuildIn = "..")]
public sealed class PacifistTools : HarnessScenario
{
    public override async Task Run()
    {
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room(World.Center, width: 8, height: 7, floor: TerrainDefOf.WoodPlankFloor, roof: true);
        var pacifist = Spawn.Colonist("Pacifist", inside: room, work: [WorkTypeDefOf.Cleaning], incapableOf: WorkTags.Violent);
        var mopCell = room.Center + IntVec3.East;
        var clubCell = room.Center + IntVec3.West * 2;
        var mop = Spawn.Thing(JanitorDefOf.Janitor_Mop, mopCell, stuff: ThingDefOf.WoodLog);
        var club = Spawn.Thing(ScenarioDefOf.MeleeWeapon_Club, clubCell, stuff: ThingDefOf.WoodLog);

        Require(pacifist.WorkTagIsDisabled(WorkTags.Violent));
        Check(OffersToEquip(pacifist, mop));
        Check(!OffersToEquip(pacifist, club));

        Camera.Frame(room);
        await Shot("room");
    }

    // True when choosing one of the pawn's right-click options on the thing starts an equip job for it.
    private static bool OffersToEquip(Pawn pawn, Thing thing)
    {
        foreach (var option in pawn.FloatMenuAt(thing.Position).Where(o => !o.Disabled))
        {
            option.action();
            bool equips = pawn.CurJobDef == JobDefOf.Equip && pawn.CurJob.targetA.Thing == thing;
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            if (equips) return true;
        }
        return false;
    }
}
