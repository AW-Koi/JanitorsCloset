namespace JanitorsCloset.Scenarios;

/// <summary>Blood's info card says it is wet filth, so players know which tool suits it.</summary>
[Mod("../mod", Build = "dotnet build Source/JanitorsCloset/JanitorsCloset.csproj --source https://api.nuget.org/v3/index.json", BuildIn = "..")]
public sealed class FilthInfoCardShowsCategory : HarnessScenario
{
    public override Task Run()
    {
        // A pool of blood on a wooden floor.
        Clock.Speed = TimeSpeed.Paused;
        var room = Build.Room()
            .Size(4, 4)
            .WithFloor(TerrainDefOf.WoodPlankFloor)
            .Build();
        var blood = Spawn.Filth(ThingDefOf.Filth_Blood, room.Center);
        Require(blood != null);

        // Its info card carries a cleaning-category line reading "Wet filth".
        var entry = blood.SpecialDisplayStats()
            .SingleOrDefault(stat => stat.LabelCap == "JanitorsCloset.FilthCategory.Label".Translate().CapitalizeFirst());
        Require(entry != null);
        Check(entry.ValueString == "JanitorsCloset.CleaningTool.SpecialtyWet".Translate());
        return Task.CompletedTask;
    }
}
