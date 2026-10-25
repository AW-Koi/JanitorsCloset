using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Crafting;

/// <summary>A tribe with no hay can still make a straw broom from wood alone.</summary>
public sealed class StrawBroomWithoutHay : HarnessScenario
{
    public override async Task Run()
    {
        // Housekeeping researched, and the wood-only recipe needs no hay.
        Clock.Speed = TimeSpeed.Paused;
        Find.ResearchManager.FinishProject(ScenarioDefOf.Housekeeping);
        var recipe = ScenarioDefOf.Make_Janitor_StrawBroomFromWood;
        Check(!recipe.ingredients.Any(ingredient => ingredient.filter.Allows(ThingDefOf.Hay)));

        // A crafter in a closed room with a crafting spot, a bill for the recipe and a pile of wood.
        var room = Build.Room()
            .Size(7, 7)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .WithDoor(Rot4.South)
            .Roofed()
            .Build();
        var crafter = Spawn.Colonist().WithName("Crafter").Inside(room).Doing(WorkTypeDefOf.Crafting).Spawn();
        Colonists.RemoveAllExcept(crafter);
        var spot = (Building_WorkTable)Spawn.Thing(ScenarioDefOf.CraftingSpot).At(room.Center + IntVec3.North * 2).Spawn();
        spot.BillStack.AddBill(recipe.MakeNewBill());
        Spawn.Thing(ThingDefOf.WoodLog).Stack(40).At(room.Center + IntVec3.East * 2).Spawn();

        // The crafter makes a straw broom with no hay on the map.
        Clock.Speed = TimeSpeed.Superfast;
        await Until(() => World.Things(JanitorDefOf.Janitor_StrawBroom).Count > 0, timeoutSeconds: 120);
    }
}
