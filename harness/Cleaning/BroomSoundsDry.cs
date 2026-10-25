using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Cleaning;

/// <summary>A push broom sweeping blood plays the dry sweeping sound, not vanilla's wet one.</summary>
public sealed class BroomSoundsDry : HarnessScenario
{
    public override async Task Run()
    {
        // A lone janitor with a push broom and one thick pool of blood.
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room()
            .Size(7, 7)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .WithDoor(Rot4.South)
            .Roofed()
            .Build();
        var janitor = Spawn.Colonist()
            .WithName("Sweeper")
            .Inside(room)
            .Doing(WorkTypeDefOf.Cleaning)
            .Spawn();
        Spawn.Thing(JanitorDefOf.Janitor_PushBroom)
            .MadeOf(ThingDefOf.WoodLog)
            .GivenTo(janitor)
            .Spawn();
        Colonists.RemoveAllExcept(janitor);
        foreach (var filth in room.Filth()) filth.Destroy();
        Spawn.Filth(ThingDefOf.Filth_Blood, room.Center, thickness: 5);
        Camera.Follow(janitor, zoom: 11f);

        // While sweeping the blood, the dry sound plays and the wet one doesn't.
        Clock.Speed = TimeSpeed.Normal;
        await Until(() => janitor.CurrentJob() == JobDefOf.Clean && Playing(JanitorDefOf.Interact_CleanFilth_Dirt), timeoutSeconds: 60);
        Check(!Playing(JanitorDefOf.Interact_CleanFilth_Fluid));
    }

    private static bool Playing(SoundDef sound) =>
        Find.SoundRoot.sustainerManager.AllSustainers.Any(sustainer => sustainer.def == sound);
}
