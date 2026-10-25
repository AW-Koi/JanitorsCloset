using JanitorsCloset.Carts;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Carts;

/// <summary>Drafting a janitor halfway through a push leaves the cart parked where they stood, still theirs.</summary>
public sealed class DraftedJanitorLeavesCartParked : HarnessScenario
{
    public override async Task Run()
    {
        // A janitor, their cart in the closet, and a messy ward across an open yard.
        Clock.Speed = TimeSpeed.Paused;
        World.Calm();
        World.RemoveHostiles();
        Build.Clearing(CellRect.CenteredOn(World.Center, 18, 8));
        var closet = Build.Room()
            .CenteredOn(World.Center + IntVec3.West * 9)
            .Size(5, 5)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .WithDoor(Rot4.East)
            .Roofed()
            .Build();
        var ward = Build.Room()
            .CenteredOn(World.Center + IntVec3.East * 8)
            .Size(9, 7)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .WithDoor(Rot4.West)
            .Roofed()
            .Build();
        var janitor = Spawn.Colonist().WithName("Janitor").Inside(closet).Doing(WorkTypeDefOf.Cleaning).Spawn();
        Spawn.Thing(JanitorDefOf.Janitor_Mop).MadeOf(ThingDefOf.WoodLog).GivenTo(janitor).Spawn();
        var cart = Spawn.Thing(JanitorDefOf.Janitor_Cart).At(closet.Center + IntVec3.North).Spawn();
        Colonists.RemoveAllExcept(janitor);
        ward.Scatter(ThingDefOf.Filth_Blood, 6, seed: 3);

        // The janitor sets off with the cart and is drafted once outside the closet.
        Clock.Speed = TimeSpeed.Normal;
        await Until(() => janitor.carryTracker.CarriedThing == cart && !closet.Bounds.Contains(janitor.Position), timeoutSeconds: 90);
        janitor.drafter.Drafted = true;
        await Ticks(1);

        // The cart stands on the ground outside both rooms, and still belongs to the janitor.
        Check(janitor.carryTracker.CarriedThing == null);
        Check(cart.Spawned);
        Check(!closet.Contains(cart) && !ward.Contains(cart));
        Check(cart.TryGetComp<CompJanitorCart>().Owner == janitor);
        Camera.Look(cart.Position, zoom: Camera.Close);
        await Shot("left in the yard");
    }
}
