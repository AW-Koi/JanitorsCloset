using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios;

/// <summary>A colonist incapable of violence can equip a mop from the right-click menu but still can't equip a club.</summary>
[Mod("../mod", Build = "dotnet build Source/JanitorsCloset/JanitorsCloset.csproj --source https://api.nuget.org/v3/index.json", BuildIn = "..")]
public sealed class PacifistTools : HarnessScenario
{
    public override async Task Run()
    {
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room()
            .Size(8, 7)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .WithDoor(Rot4.South)
            .Roofed()
            .Build();
        var pacifist = Spawn.Colonist()
            .WithName("Pacifist")
            .Inside(room)
            .Doing(WorkTypeDefOf.Cleaning)
            .IncapableOf(WorkTags.Violent)
            .Spawn();
        var mopCell = room.Center + IntVec3.East;
        var clubCell = room.Center + IntVec3.West * 2;
        var mop = Spawn.Thing(JanitorDefOf.Janitor_Mop)
            .MadeOf(ThingDefOf.WoodLog)
            .At(mopCell)
            .Spawn();
        var club = Spawn.Thing(ScenarioDefOf.MeleeWeapon_Club)
            .MadeOf(ThingDefOf.WoodLog)
            .At(clubCell)
            .Spawn();

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
