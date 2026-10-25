using UnityEngine;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Tools;

/// <summary>A glittervacuum speeds up clearing snow, like the brooms do.</summary>
public sealed class GlittervacuumClearsSnowFaster : HarnessScenario
{
    private const float GlittervacuumBonus = 1f;

    public override async Task Run()
    {
        // A cleaner holding a normal-quality glittervacuum, alone in an open-air yard.
        Clock.Speed = TimeSpeed.Paused;
        var yard = Build.Room()
            .Size(7, 7)
            .WithFloor(TerrainDefOf.Concrete)
            .WithDoor(Rot4.South)
            .Build();
        var janitor = Spawn.Colonist().WithName("Vacuumer").Inside(yard).Doing(WorkTypeDefOf.Cleaning).Spawn();
        Spawn.Thing(JanitorDefOf.Janitor_Glittervacuum).OfQuality(QualityCategory.Normal).GivenTo(janitor).Spawn();
        Colonists.RemoveAllExcept(janitor);
        var idleLabourSpeed = StatDefOf.GeneralLaborSpeed.Worker.GetValue(StatRequest.For(janitor));

        // Deep snow in the yard, inside the snow-clearing area.
        var snowCell = yard.Center + IntVec3.East * 2;
        World.Map.areaManager.SnowOrSandClear[snowCell] = true;
        World.Map.snowGrid.SetDepth(snowCell, 0.8f);

        // While clearing it, labour speed rises by the tool's bonus.
        Clock.Speed = TimeSpeed.Normal;
        await Until(() => janitor.CurrentJob() == JobDefOf.ClearSnow, timeoutSeconds: 60);
        var clearingLabourSpeed = StatDefOf.GeneralLaborSpeed.Worker.GetValue(StatRequest.For(janitor));
        Check(Mathf.Abs(clearingLabourSpeed - idleLabourSpeed - GlittervacuumBonus) < 0.01f);
    }
}
