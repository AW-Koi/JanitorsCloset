using HarmonyLib;
using UnityEngine;
using Verse;

namespace JanitorsCloset
{
    public class JanitorsCloset : Mod
    {
        public static JanitorsClosetSettings Settings;

        public JanitorsCloset(ModContentPack content) : base(content)
        {
            Settings = GetSettings<JanitorsClosetSettings>();
        }

        public override string SettingsCategory() => "Janitor's Closet";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled(
                "JanitorsCloset.Settings.DebugLogging".Translate(),
                ref Settings.DebugLogging,
                "JanitorsCloset.Settings.DebugLoggingTooltip".Translate());
            listing.End();
            base.DoSettingsWindowContents(inRect);
        }
    }

    public class JanitorsClosetSettings : ModSettings
    {
        public bool DebugLogging;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DebugLogging, "DebugLogging", false);
        }
    }

    [StaticConstructorOnStartup]
    public static class JanitorsClosetInit
    {
        static JanitorsClosetInit()
        {
            new Harmony("TerraIncognita.JanitorsCloset").PatchAll();
            Log.Message("[Janitor's Closet] Harmony patches applied.");
        }
    }
}
