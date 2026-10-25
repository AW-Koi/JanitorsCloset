using UnityEngine;

namespace JanitorsCloset.Scenarios.Inspirations;

/// <summary>A cleaning frenzy doubles a colonist's cleaning speed while it lasts.</summary>
public sealed class CleaningFrenzyDoublesSpeed : HarnessScenario
{
    public override Task Run()
    {
        // A colonist who cleans, measured before any inspiration.
        Clock.Speed = TimeSpeed.Paused;
        var cleaner = Spawn.Colonist().WithName("Cleaner").At(World.Center).Doing(WorkTypeDefOf.Cleaning).Spawn();
        var calmSpeed = StatDefOf.CleaningSpeed.Worker.GetValue(StatRequest.For(cleaner));

        // Inspired with a cleaning frenzy, they clean twice as fast.
        Require(cleaner.mindState.inspirationHandler.TryStartInspiration(ScenarioDefOf.Janitor_CleaningFrenzy));
        var inspiredSpeed = StatDefOf.CleaningSpeed.Worker.GetValue(StatRequest.For(cleaner));
        Check(Mathf.Approximately(inspiredSpeed, calmSpeed * 2f));
        return Task.CompletedTask;
    }
}
