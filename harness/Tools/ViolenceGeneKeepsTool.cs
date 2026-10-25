using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Tools;

/// <summary>Gaining a violence-disabling gene drops a club but leaves a mop in hand.</summary>
[Dlc("biotech")]
public sealed class ViolenceGeneKeepsTool : HarnessScenario
{
    public override Task Run()
    {
        // One colonist holding a mop and another holding a club.
        Clock.Speed = TimeSpeed.Paused;
        var janitor = Spawn.Colonist().WithName("Janitor").At(World.Center).Spawn();
        var brawler = Spawn.Colonist().WithName("Brawler").At(World.Center + IntVec3.East * 2).Spawn();
        var mop = Spawn.Thing(JanitorDefOf.Janitor_Mop).MadeOf(ThingDefOf.WoodLog).GivenTo(janitor).Spawn();
        Spawn.Thing(ScenarioDefOf.MeleeWeapon_Club).MadeOf(ThingDefOf.WoodLog).GivenTo(brawler).Spawn();

        // Both gain the violence-disabled gene: the club drops, the mop stays.
        janitor.genes.AddGene(ScenarioDefOf.ViolenceDisabled, xenogene: true);
        brawler.genes.AddGene(ScenarioDefOf.ViolenceDisabled, xenogene: true);
        Check(janitor.equipment.Primary == mop);
        Check(brawler.equipment.Primary == null);
        return Task.CompletedTask;
    }
}
