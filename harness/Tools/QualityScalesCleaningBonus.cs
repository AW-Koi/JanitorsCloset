using UnityEngine;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Tools;

/// <summary>A tool's cleaning bonus scales with its quality, on vanilla's tool-quality curve.</summary>
public sealed class QualityScalesCleaningBonus : HarnessScenario
{
    private const float PushBroomBonus = 0.6f;

    public override Task Run()
    {
        // An awful, a normal and a legendary push broom.
        Clock.Speed = TimeSpeed.Paused;
        var awful = Spawn.Thing(JanitorDefOf.Janitor_PushBroom).MadeOf(ThingDefOf.WoodLog).OfQuality(QualityCategory.Awful).At(World.Center).Spawn();
        var normal = Spawn.Thing(JanitorDefOf.Janitor_PushBroom).MadeOf(ThingDefOf.WoodLog).OfQuality(QualityCategory.Normal).At(World.Center).Spawn();
        var legendary = Spawn.Thing(JanitorDefOf.Janitor_PushBroom).MadeOf(ThingDefOf.WoodLog).OfQuality(QualityCategory.Legendary).At(World.Center).Spawn();

        // Each grants its bonus times 0.8, 1 and 1.5.
        Check(Mathf.Approximately(StatWorker.StatOffsetFromGear(awful, StatDefOf.CleaningSpeed), PushBroomBonus * 0.8f));
        Check(Mathf.Approximately(StatWorker.StatOffsetFromGear(normal, StatDefOf.CleaningSpeed), PushBroomBonus));
        Check(Mathf.Approximately(StatWorker.StatOffsetFromGear(legendary, StatDefOf.CleaningSpeed), PushBroomBonus * 1.5f));
        return Task.CompletedTask;
    }
}
