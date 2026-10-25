using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Carts;

/// <summary>A janitor with a mop wheels their cart from the closet to a messy room, parks it out of the doorway and cleans.</summary>
public sealed class JanitorWheelsCartToTheMess : HarnessScenario
{
    public override async Task Run()
    {
        // A closet with a janitor and their cart, and a ward across an open yard.
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
        var walkingSpeed = janitor.GetStatValue(StatDefOf.MoveSpeed);

        // Blood across the ward floor.
        ward.Scatter(ThingDefOf.Filth_Blood, 6, seed: 3);
        Camera.Look(World.Center, zoom: 18f);
        await Shot("closet and ward");

        // The janitor takes the cart in hand and walks it over, slower than on foot.
        Clock.Speed = TimeSpeed.Normal;
        await Until(() => janitor.carryTracker.CarriedThing == cart, timeoutSeconds: 60);
        Check(janitor.CurrentJob() == JanitorDefOf.Janitor_PushCart);
        Check(Math.Abs(janitor.GetStatValue(StatDefOf.MoveSpeed) / walkingSpeed - 0.85f) < 0.01f);
        Camera.Follow(janitor, zoom: Camera.Close);
        await Ticks(240);
        await Shot("pushing");

        // The cart ends up parked inside the ward, clear of the door.
        await Until(() => cart.Spawned && ward.Contains(cart), timeoutSeconds: 60);
        Check(ward.Interior.Contains(cart.Position));
        Check(cart.Position.GetDoor(World.Map) == null);

        // The janitor cleans the ward, and every push made for a ward mess parks the cart inside the ward.
        Clock.Speed = TimeSpeed.Superfast;
        var parkingForWardMesses = new List<IntVec3>();
        Job lastPush = null;
        await Until(() =>
        {
            if (janitor.CurJobDef == JanitorDefOf.Janitor_PushCart && janitor.CurJob != lastPush)
            {
                lastPush = janitor.CurJob;
                if (lastPush.targetQueueB.Any(t => ward.Bounds.Contains(t.Cell))) parkingForWardMesses.Add(lastPush.targetB.Cell);
            }
            return ward.Filth().Count == 0;
        }, timeoutSeconds: 120);
        Clock.Speed = TimeSpeed.Paused;
        Check(parkingForWardMesses.All(cell => ward.Interior.Contains(cell)));
        Check(cart.Spawned);
        Camera.Frame(ward);
        await Shot("parked in the ward");
    }
}
