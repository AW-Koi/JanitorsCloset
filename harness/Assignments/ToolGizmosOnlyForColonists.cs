using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Assignments;

/// <summary>A visitor carrying a mop shows no cleaning-assignment gizmos; a colonist with one does.</summary>
public sealed class ToolGizmosOnlyForColonists : HarnessScenario
{
    public override Task Run()
    {
        // A colonist holding a mop.
        Clock.Speed = TimeSpeed.Paused;
        var colonist = Spawn.Colonist().WithName("Janitor").At(World.Center).Spawn();
        Spawn.Thing(JanitorDefOf.Janitor_Mop).MadeOf(ThingDefOf.WoodLog).GivenTo(colonist).Spawn();

        // A villager from a friendly faction, also holding a mop.
        var visitorFaction = Find.FactionManager.RandomNonHostileFaction(allowNonHumanlike: false);
        Require(visitorFaction != null);
        var visitor = PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, visitorFaction);
        GenSpawn.Spawn(visitor, World.Center + IntVec3.East * 2, World.Map);
        Spawn.Thing(JanitorDefOf.Janitor_Mop).MadeOf(ThingDefOf.WoodLog).GivenTo(visitor).Spawn();

        // Only the colonist gets the cleaning gizmos.
        Check(colonist.equipment.GetGizmos().Any(gizmo => gizmo is CleaningAreaCommand));
        Check(!visitor.equipment.GetGizmos().Any(gizmo => gizmo is CleaningAreaCommand));
        return Task.CompletedTask;
    }
}
