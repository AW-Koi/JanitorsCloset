namespace JanitorsCloset.Scenarios.Settings;

/// <summary>The mod's settings page, which holds only diagnostics, is listed only in development mode.</summary>
public sealed class SettingsOnlyInDevMode : HarnessScenario
{
    public override Task Run()
    {
        var mod = LoadedModManager.GetMod<JanitorsCloset>();
        var devModeBefore = Prefs.DevMode;

        // Out of development mode the settings page isn't listed.
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
