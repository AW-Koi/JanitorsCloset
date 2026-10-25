namespace JanitorsCloset.Scenarios;

/// <summary>The mod's settings page, which holds only diagnostics, is listed only in development mode.</summary>
[Mod("../mod", Build = "dotnet build Source/JanitorsCloset/JanitorsCloset.csproj --source https://api.nuget.org/v3/index.json", BuildIn = "..")]
public sealed class SettingsOnlyInDevMode : HarnessScenario
{
    public override Task Run()
    {
        var mod = LoadedModManager.GetMod<JanitorsCloset>();
        var devModeBefore = Prefs.DevMode;

        // Out of development mode the settings page has no category, so the options list skips it.
        Prefs.DevMode = false;
        Check(mod.SettingsCategory().NullOrEmpty());

        // In development mode it is listed under the mod's name.
        Prefs.DevMode = true;
        Check(mod.SettingsCategory() == mod.Content.Name);

        // Leave the worker's development mode as it was.
        Prefs.DevMode = devModeBefore;
        return Task.CompletedTask;
    }
}
