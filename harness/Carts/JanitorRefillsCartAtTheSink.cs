using JanitorsCloset.Carts;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Carts;

/// <summary>A janitor wets their mop from the cart until its water runs out, then wheels it home to the closet sink and refills it.</summary>
public sealed class JanitorRefillsCartAtTheSink : HarnessScenario
{
    public override async Task Run()
    {
        // A closet with a utility sink and the janitor's cart, low on water, and a bloody ward across an open yard.
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
        var sink = Spawn.Thing(JanitorDefOf.Janitor_UtilitySink).At(closet.Center + IntVec3.West).Spawn();
        var cart = Spawn.Thing(JanitorDefOf.Janitor_Cart).At(closet.Center + IntVec3.North).Spawn();
        var water = cart.TryGetComp<CompJanitorCart>().Water;
        water.TryUse();
        water.TryUse();
        water.TryUse();

        // A janitor with a wet mop, and more blood in the ward than one wetting cleans.
        var janitor = Spawn.Colonist().WithName("Janitor").Inside(closet).Doing(WorkTypeDefOf.Cleaning).Spawn();
        var mop = Spawn.Thing(JanitorDefOf.Janitor_Mop).MadeOf(ThingDefOf.WoodLog).GivenTo(janitor).Spawn();
        var wetness = mop.TryGetComp<CompMopCharge>().Wetness;
        Colonists.RemoveAllExcept(janitor);
        ward.Scatter(ThingDefOf.Filth_Blood, wetness.Capacity + 4, seed: 5);

        // The janitor wheels the cart out, mops until the mop runs dry, then wets it from the cart's last dunk within reach.
        Clock.Speed = TimeSpeed.Superfast;
        await Until(() => janitor.CurrentJob() == JanitorDefOf.Janitor_PushCart, timeoutSeconds: 60);
        await Until(() => janitor.CurrentJob() == JanitorDefOf.Janitor_DunkMop, timeoutSeconds: 120);
        Check(cart.Spawned && cart.Position.DistanceTo(janitor.Position) <= CartPushRule.Reach);
        await Until(() => water.Empty, timeoutSeconds: 30);

        // With the cart dry and the mop spent again, they wheel it back to the sink and refill it.
        await Until(() => janitor.CurrentJob() == JanitorDefOf.Janitor_ReturnCart, timeoutSeconds: 120);
        Clock.Speed = TimeSpeed.Normal;
        await Ticks(200);
        Camera.Look(janitor.Position, zoom: 16f);
        await Shot("heading home");
        Clock.Speed = TimeSpeed.Superfast;
        await Until(() => water.Remaining == water.Capacity, timeoutSeconds: 90);
        Check(JanitorCarts.IsHome(cart, sink));
        Check(!wetness.Empty);
        Clock.Speed = TimeSpeed.Paused;
        Camera.Frame(closet);
        await Shot("refilled at the sink");
    }
}
