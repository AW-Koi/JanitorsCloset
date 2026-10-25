using JanitorsCloset.Carts;
using JanitorsCloset.Defs;

namespace JanitorsCloset.Scenarios.Showcase;

/// <summary>
/// Brightwater, an industrial colony on a working morning. Marta, a former academy custodian, wheels her cart
/// between the rooms, wets her mop at it and takes it home to the closet sink when the water runs out.
/// </summary>
[Biome(nameof(BiomeDefOf.TemperateForest))]
public sealed class ColonyAtWork : HarnessScenario
{
    public override async Task Run()
    {
        // A calm, cleared site in the morning, with soil to build on.
        Clock.Speed = TimeSpeed.Paused;
        World.Calm();
        World.RemoveHostiles();
        var origin = World.Center + new IntVec3(-8, 0, -18);
        IntVec3 At(int x, int z) => origin + new IntVec3(x, 0, z);
        CellRect Area(int x, int z, int width, int height) => new CellRect(origin.x + x, origin.z + z, width, height);
        var site = Area(-28, -6, 76, 48);
        Build.Clearing(site);
        Build.Floor(site, TerrainDefOf.Soil);
        Clock.SkipToHour(9);

        // The colonists, at work where their stories put them.
        var marta = Spawn.Colonist().WithName("Marta").At(At(28, 14)).Doing(WorkTypeDefOf.Cleaning).Spawn();
        var bertram = Spawn.Colonist().WithName("Bertram").At(At(27, 28)).Doing(ShowcaseDefOf.Cooking).Spawn();
        var rook = Spawn.Colonist().WithName("Rook").At(At(9, 12)).Doing(WorkTypeDefOf.Hauling).Spawn();
        var okafor = Spawn.Colonist().WithName("Okafor").At(At(35, 12)).Doing(WorkTypeDefOf.Doctor).Spawn();
        var hazel = Spawn.Colonist().WithName("Hazel").At(At(-10, 15)).Doing(WorkTypeDefOf.Growing, WorkTypeDefOf.PlantCutting).Spawn();
        var tomas = Spawn.Colonist().WithName("Tomas").At(At(20, 12)).Doing(WorkTypeDefOf.Crafting, WorkTypeDefOf.Smithing, ShowcaseDefOf.Art).Spawn();
        var pike = Spawn.Colonist().WithName("Pike").At(At(-14, 3)).Doing(WorkTypeDefOf.Hauling).Spawn();
        Colonists.RemoveAllExcept(marta, bertram, rook, okafor, hazel, tomas, pike);
        var mop = Spawn.Thing(JanitorDefOf.Janitor_Mop).MadeOf(ThingDefOf.WoodLog).OfQuality(QualityCategory.Good).GivenTo(marta).Spawn();

        // The bedroom wing: one room each, and a double for Bertram and Rook.
        var bedroomX = new[] { -20, -14, -8, -2, 4 };
        var bedrooms = bedroomX.Select(x => Build.Room().Covering(Area(x, 24, 7, 7)).Adjoining().WithDoor(Rot4.South).Roofed().Powered().Build()).ToArray();

        // The commons: a dining and rec hall, the kitchen, butchery and freezer behind it.
        var hall = Build.Room().Covering(Area(10, 22, 15, 11)).Adjoining().WithFloor(TerrainDefOf.WoodPlankFloor).WithDoor(Rot4.South).WithDoorAt(At(24, 28)).Roofed().Powered().Build();
        var kitchen = Build.Room().Covering(Area(24, 26, 8, 7)).Adjoining().WithFloor(ShowcaseDefOf.SterileTile).WithDoorAt(At(31, 29)).WithDoorAt(At(27, 32)).Roofed().Powered().Build();
        var butchery = Build.Room().Covering(Area(31, 26, 6, 7)).Adjoining().WithFloor(ShowcaseDefOf.SterileTile).WithDoorAt(At(34, 32)).WithDoorAt(At(34, 26)).Roofed().Powered().Build();
        var freezer = Build.Room().Covering(Area(24, 32, 13, 6)).Adjoining().WithFloor(TerrainDefOf.Concrete).Roofed().Powered().Build();

        // The working wing: storeroom, workshop, Marta's closet, the battery room and the hospital.
        var storeroom = Build.Room().Covering(Area(4, 8, 11, 9)).Adjoining().WithFloor(TerrainDefOf.Concrete).WithDoor(Rot4.North).Roofed().Powered().Build();
        var workshop = Build.Room().Covering(Area(14, 8, 13, 9)).Adjoining().WithFloor(TerrainDefOf.MetalTile).WithDoor(Rot4.North).Roofed().Powered().Build();
        Build.Room().Covering(Area(26, 12, 5, 5)).Adjoining().WithFloor(ShowcaseDefOf.SterileTile).WithDoor(Rot4.North).Roofed().Powered().Build();
        Build.Room().Covering(Area(26, 8, 5, 5)).Adjoining().WithFloor(TerrainDefOf.Concrete).WithDoorAt(At(26, 11)).Roofed().Powered().Build();
        var hospital = Build.Room().Covering(Area(30, 8, 10, 9)).Adjoining().WithFloor(ShowcaseDefOf.SterileTile).WithDoor(Rot4.North).Roofed().Powered().Build();
        World.Home(Area(-21, 8, 61, 30));

        // Okafor reads at night; Hazel keeps roses; Tomas lives among his own carvings; Marta keeps a daylily.
        Furnish.Bedroom(bedrooms[0]).MadeOf(ThingDefOf.WoodLog).Floor(ShowcaseDefOf.CarpetBlueSubtle).Lamp().Art(ShowcaseDefOf.SculptureSmall, ThingDefOf.Silver).Owner(okafor).Build();
        Furnish.Bedroom(bedrooms[1]).MadeOf(ThingDefOf.WoodLog).Floor(ShowcaseDefOf.CarpetGreenForest).Lamp().Plants(ShowcaseDefOf.Plant_Rose).Owner(hazel).Build();
        Furnish.Bedroom(bedrooms[2]).MadeOf(ThingDefOf.WoodLog).Floor(TerrainDefOf.WoodPlankFloor).Lamp().Art(ShowcaseDefOf.SculptureLarge, ShowcaseDefOf.BlocksMarble).Owner(tomas).Build();
        Furnish.Bedroom(bedrooms[3]).Bed(ShowcaseDefOf.DoubleBed).MadeOf(ThingDefOf.WoodLog).Floor(ShowcaseDefOf.CarpetBurgundy).Lamp().Plants(ShowcaseDefOf.Plant_Daylily).Owner(bertram, rook).Build();
        Furnish.Bedroom(bedrooms[4]).MadeOf(ThingDefOf.WoodLog).Floor(ShowcaseDefOf.CarpetCream).Lamp().Plants(ShowcaseDefOf.Plant_Daylily).Art(ShowcaseDefOf.SculptureSmall, ThingDefOf.WoodLog).Owner(marta).Build();

        // The hall: dining tables on the west side, billiards, chess and a television on a rug to the east.
        Furnish.Dining(hall, Area(11, 24, 5, 7)).Table(ShowcaseDefOf.Table2x2c).MadeOf(ThingDefOf.WoodLog).Build();
        Build.Floor(Area(17, 27, 7, 5), ShowcaseDefOf.CarpetBurgundy);
        Spawn.Thing(ShowcaseDefOf.BilliardsTable).MadeOf(ThingDefOf.WoodLog).OfQuality(QualityCategory.Excellent).At(At(21, 25)).Facing(Rot4.North).Spawn();
        Spawn.Thing(ShowcaseDefOf.ChessTable).MadeOf(ThingDefOf.WoodLog).At(At(18, 30)).Facing(Rot4.North).Spawn();
        Spawn.Thing(ThingDefOf.DiningChair).MadeOf(ThingDefOf.WoodLog).At(At(17, 30)).Facing(Rot4.East).Spawn();
        Spawn.Thing(ThingDefOf.DiningChair).MadeOf(ThingDefOf.WoodLog).At(At(19, 30)).Facing(Rot4.West).Spawn();
        Spawn.Thing(ShowcaseDefOf.TubeTelevision).At(At(22, 31)).Facing(Rot4.South).Spawn();
        Spawn.Thing(ShowcaseDefOf.Couch).MadeOf(ThingDefOf.Cloth).At(At(21, 29)).Facing(Rot4.North).Spawn();
        Spawn.Thing(ShowcaseDefOf.Bookcase).At(At(11, 31)).Facing(Rot4.North).Spawn();
        Spawn.Thing(ShowcaseDefOf.SculptureLarge).MadeOf(ShowcaseDefOf.BlocksMarble).OfQuality(QualityCategory.Masterwork).At(At(16, 31)).Facing(Rot4.South).Spawn();
        Spawn.Thing(ShowcaseDefOf.AnimalBed).MadeOf(ThingDefOf.Cloth).At(At(23, 31)).Facing(Rot4.North).Spawn();
        Decorate.Room(hall).PlantPots(ShowcaseDefOf.Plant_Rose).Lamps().Build();

        // Bertram's kitchen and butchery, and the freezer they share.
        Spawn.Thing(ShowcaseDefOf.ElectricStove).At(At(29, 31)).Facing(Rot4.North).WithBill(ShowcaseDefOf.CookMealSimple).Spawn();
        Spawn.Thing(ShowcaseDefOf.ShelfSmall).At(At(25, 31)).Holding(ThingDefOf.MealSimple, 10).Spawn();
        Spawn.Thing(ShowcaseDefOf.ShelfSmall).At(At(26, 31)).Holding(ThingDefOf.RawPotatoes, 40).Spawn();
        Decorate.Room(kitchen).Lamps().Build();
        Spawn.Thing(ShowcaseDefOf.TableButcher).MadeOf(ThingDefOf.WoodLog).At(At(35, 29)).Facing(Rot4.East).Spawn();
        Decorate.Room(butchery).Lamps().Build();
        Build.InWall(ShowcaseDefOf.Cooler, At(36, 34), Rot4.East);
        Build.InWall(ShowcaseDefOf.Cooler, At(36, 35), Rot4.East);
        Spawn.Thing(ThingDefOf.Shelf).MadeOf(ThingDefOf.Steel).At(At(25, 36)).Facing(Rot4.North).Holding(ShowcaseDefOf.Meat_Muffalo, 60).Holding(ShowcaseDefOf.Meat_Muffalo, 40).Spawn();
        Spawn.Thing(ThingDefOf.Shelf).MadeOf(ThingDefOf.Steel).At(At(28, 36)).Facing(Rot4.North).Holding(ShowcaseDefOf.RawRice, 75).Holding(ThingDefOf.RawPotatoes, 75).Spawn();
        Spawn.Thing(ThingDefOf.Shelf).MadeOf(ThingDefOf.Steel).At(At(31, 36)).Facing(Rot4.North).Holding(ThingDefOf.MealSimple, 20).Holding(ShowcaseDefOf.Beer, 25).Spawn();
        Decorate.Room(freezer).Lamps().Build();

        // Rook's storeroom: shelves in two rows either side of an aisle.
        foreach (var (x, z, def, count) in new[]
        {
            (5, 13, ThingDefOf.Steel, 75), (7, 13, ThingDefOf.WoodLog, 75), (11, 13, ThingDefOf.Cloth, 75),
            (5, 10, ThingDefOf.ComponentIndustrial, 20), (7, 10, ThingDefOf.Silver, 300), (11, 10, ShowcaseDefOf.Kibble, 75),
        })
        {
            Spawn.Thing(ThingDefOf.Shelf).MadeOf(ThingDefOf.Steel).At(At(x, z)).Facing(Rot4.North).Holding(def, count).Holding(def, count).Spawn();
        }
        Decorate.Room(storeroom).Lamps().Build();

        // Tomas's workshop: machining and tailoring along the north wall, smithy and sculpting along the south.
        Spawn.Thing(ShowcaseDefOf.TableMachining).At(At(17, 15)).Facing(Rot4.North).Spawn();
        Spawn.Thing(ShowcaseDefOf.ElectricTailoringBench).MadeOf(ThingDefOf.Steel).At(At(23, 15)).Facing(Rot4.North).WithBill(ShowcaseDefOf.Make_Apparel_BasicShirt).Spawn();
        Spawn.Thing(ShowcaseDefOf.ElectricSmithy).At(At(17, 9)).Facing(Rot4.South).Spawn();
        Spawn.Thing(ShowcaseDefOf.TableSculpting).MadeOf(ThingDefOf.WoodLog).At(At(22, 9)).Facing(Rot4.South).WithBill(ShowcaseDefOf.Make_SculptureSmall).Spawn();
        Spawn.Thing(ShowcaseDefOf.ToolCabinet).At(At(15, 12)).Facing(Rot4.North).Spawn();
        Spawn.Thing(ShowcaseDefOf.ToolCabinet).At(At(24, 12)).Facing(Rot4.North).Spawn();
        Spawn.Thing(ShowcaseDefOf.BlocksMarble).Stack(60).At(At(20, 11)).Spawn();
        Decorate.Room(workshop).Lamps().Build();

        // Marta's closet: the utility sink, her cart, a shelf with a spare broom and her daylily.
        var sink = Spawn.Thing(JanitorDefOf.Janitor_UtilitySink).At(At(27, 15)).Facing(Rot4.North).Spawn();
        Spawn.Thing(ShowcaseDefOf.PlantPot).MadeOf(ThingDefOf.Steel).At(At(29, 15)).Growing(ShowcaseDefOf.Plant_Daylily).Spawn();
        var cart = Spawn.Thing(JanitorDefOf.Janitor_Cart).At(At(27, 13)).Spawn();
        Spawn.Thing(ShowcaseDefOf.ShelfSmall).At(At(29, 13)).Holding(JanitorDefOf.Janitor_PushBroom, 1).Spawn();
        Spawn.Thing(ShowcaseDefOf.StandingLamp).At(At(29, 14)).Spawn();

        // Power: batteries in their own room, solar panels behind the wing, and a conduit under the courtyard to the north building.
        Spawn.Thing(ShowcaseDefOf.Battery).At(At(28, 9)).Facing(Rot4.North).Charged().Spawn();
        Spawn.Thing(ShowcaseDefOf.Battery).At(At(29, 9)).Facing(Rot4.North).Charged().Spawn();
        foreach (var x in new[] { 25, 30, 35 }) Spawn.Thing(ShowcaseDefOf.SolarGenerator).At(At(x, 3)).Facing(Rot4.North).Spawn();
        foreach (var x in new[] { 26, 30, 35 }) Build.Conduit(At(x, 6), At(x, 7));
        foreach (var z in Enumerable.Range(17, 5)) Spawn.Thing(ShowcaseDefOf.HiddenConduit).At(At(24, z)).Facing(Rot4.North).Spawn();

        // Okafor's hospital: three beds with vitals monitors, medicine to hand and a few books.
        foreach (var x in new[] { 32, 34, 37 }) Spawn.Thing(ShowcaseDefOf.HospitalBed).MadeOf(ThingDefOf.Steel).At(At(x, 14)).Facing(Rot4.North).Spawn();
        foreach (var x in new[] { 33, 36 }) Spawn.Thing(ShowcaseDefOf.VitalsMonitor).At(At(x, 15)).Spawn();
        Spawn.Thing(ThingDefOf.Shelf).MadeOf(ThingDefOf.Steel).At(At(31, 9)).Facing(Rot4.North).Holding(ThingDefOf.MedicineIndustrial, 20).Holding(ThingDefOf.MedicineIndustrial, 10).Spawn();
        Spawn.Thing(ShowcaseDefOf.BookcaseSmall).At(At(38, 9)).Spawn();
        Spawn.Thing(ShowcaseDefOf.PlantPot).MadeOf(ThingDefOf.Steel).At(At(38, 12)).Growing(ShowcaseDefOf.Plant_Daylily).Spawn();
        Decorate.Room(hospital).Lamps().Build();

        // The courtyard: a flagstone walk, a marble plaza with Tomas's grand piece, and a patio off the kitchen.
        Build.Floor(Area(-21, 19, 61, 2), TerrainDefOf.FlagstoneSandstone);
        foreach (var x in bedroomX) Build.Floor(Area(x + 3, 21, 1, 3), TerrainDefOf.FlagstoneSandstone);
        foreach (var (x, z, h) in new[] { (17, 21, 1), (9, 17, 2), (20, 17, 2), (28, 17, 2), (35, 17, 2), (34, 21, 5) })
        {
            Build.Floor(Area(x, z, 1, h), TerrainDefOf.FlagstoneSandstone);
        }
        Build.Floor(Area(2, 21, 4, 3), ShowcaseDefOf.TileMarble);
        Spawn.Thing(ShowcaseDefOf.SculptureGrand).MadeOf(ShowcaseDefOf.BlocksMarble).OfQuality(QualityCategory.Excellent).At(At(3, 22)).Facing(Rot4.South).Spawn();
        foreach (var cell in new[] { At(2, 21), At(5, 21) }) Spawn.Thing(ShowcaseDefOf.PlantPot).MadeOf(ThingDefOf.Steel).At(cell).Growing(ShowcaseDefOf.Plant_Rose).Spawn();
        Build.Floor(Area(25, 20, 9, 5), TerrainDefOf.TileSandstone);
        Spawn.Thing(ShowcaseDefOf.Table2x2c).MadeOf(ThingDefOf.WoodLog).At(At(28, 21)).Facing(Rot4.North).Spawn();
        foreach (var (x, z, facing) in new[] { (27, 21, Rot4.East), (27, 22, Rot4.East), (30, 21, Rot4.West), (30, 22, Rot4.West) })
        {
            Spawn.Thing(ThingDefOf.DiningChair).MadeOf(ThingDefOf.WoodLog).At(At(x, z)).Facing(facing).Spawn();
        }
        foreach (var cell in new[] { At(25, 24), At(33, 24) }) Spawn.Thing(ShowcaseDefOf.PlantPot).MadeOf(ThingDefOf.Steel).At(cell).Growing(ShowcaseDefOf.Plant_Rose).Spawn();
        Spawn.Thing(ShowcaseDefOf.StandingLamp).At(At(33, 20)).Spawn();

        // Flower beds by the bedroom doors and trees around the edges.
        foreach (var x in bedroomX)
        {
            Spawn.Plant(ShowcaseDefOf.Plant_Daylily).At(At(x + 1, 22)).Grown(1f).Spawn();
            Spawn.Plant(ShowcaseDefOf.Plant_Rose).At(At(x + 5, 22)).Grown(1f).Spawn();
        }
        foreach (var (x, z) in new[] { (-24, 22), (-25, 30), (-22, 36), (-12, 35), (-3, 36), (8, 34), (14, 36), (40, 30), (42, 21), (43, 12), (-25, 12), (2, 3), (14, 2) })
        {
            Spawn.Plant(x % 2 == 0 ? ShowcaseDefOf.Plant_TreeOak : ShowcaseDefOf.Plant_TreePoplar).At(At(x, z)).Grown(1f).Spawn();
        }

        // Hazel's fields, southwest of the storeroom.
        Build.GrowingZone(Area(-20, 6, 8, 8)).Planting(ThingDefOf.Plant_Potato).Grown(0.6f).Build();
        Build.GrowingZone(Area(-11, 6, 7, 8)).Planting(ShowcaseDefOf.Plant_Rice).Grown(0.4f).Build();
        Build.GrowingZone(Area(-3, 9, 6, 5)).Planting(ShowcaseDefOf.Plant_Corn).Grown(0.7f).Build();

        // The colony's husky and cat.
        Spawn.Animal(ShowcaseDefOf.Husky).At(At(16, 20)).Tame().WithName("Biscuit").Spawn();
        Spawn.Animal(ShowcaseDefOf.Cat).At(At(20, 30)).Tame().WithName("Soot").Spawn();

        // The morning's mess, and Pike limping home from a hunt that went wrong.
        storeroom.Scatter(ThingDefOf.Filth_Dirt, 4, seed: 1);
        workshop.Scatter(ThingDefOf.Filth_Dirt, 6, seed: 2);
        hall.Scatter(ThingDefOf.Filth_Vomit, 4, seed: 3);
        butchery.Scatter(ThingDefOf.Filth_Blood, 6, seed: 4);
        kitchen.Scatter(ThingDefOf.Filth_Blood, 4, seed: 5);
        hospital.Scatter(ThingDefOf.Filth_Blood, 4, seed: 6);
        HealthUtility.DamageUntilDowned(pike);

        // Marta's cart has one dunk left and her mop is nearly dry, so the morning's wet mess runs it dry.
        var water = cart.TryGetComp<CompJanitorCart>().Water;
        for (int i = 0; i < water.Capacity - 1; i++) water.TryUse();
        var wetness = mop.TryGetComp<CompMopCharge>().Wetness;
        for (int i = 0; i < wetness.Capacity - 3; i++) wetness.TryUse();

        // Everything electric comes on once the grid settles.
        var consumers = World.Map.listerBuildings.allBuildingsColonist.Select(b => b.TryGetComp<CompPowerTrader>()).Where(c => c != null).ToList();
        Clock.Speed = TimeSpeed.Normal;
        await Until(() => consumers.All(c => c.PowerOn), timeoutSeconds: 30);
        Clock.Speed = TimeSpeed.Paused;

        var colony = Area(-21, 1, 62, 36);
        Camera.Frame(colony);
        await Shot("Brightwater");

        // Marta wheels the cart out of the closet to the nearest mess.
        Clock.Speed = TimeSpeed.Normal;
        await Until(() => marta.carryTracker.CarriedThing == cart, timeoutSeconds: 120);
        Camera.Follow(marta, zoom: 18f);
        await Ticks(240);
        await Shot("on the move");

        // She wets the mop at the cart when it runs dry.
        Clock.Speed = TimeSpeed.Fast;
        await Until(() => marta.CurrentJob() == JanitorDefOf.Janitor_DunkMop, timeoutSeconds: 240);
        await Ticks(60);
        await Shot("wetting the mop");

        // Once the cart is dry she wheels it home to the closet sink and refills it.
        Clock.Speed = TimeSpeed.Superfast;
        await Until(() => marta.CurrentJob() == JanitorDefOf.Janitor_ReturnCart && marta.carryTracker.CarriedThing == cart, timeoutSeconds: 300);
        Clock.Speed = TimeSpeed.Normal;
        await Ticks(180);
        await Shot("heading home");
        Clock.Speed = TimeSpeed.Superfast;
        await Until(() => water.Remaining == water.Capacity, timeoutSeconds: 180);
        Check(JanitorCarts.IsHome(cart, sink));

        // Later that morning, the colony carries on around her.
        await Ticks(1500);
        Clock.Speed = TimeSpeed.Paused;
        Camera.Frame(colony);
        await Shot("later that morning");
    }
}
