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
            // TODO: add settings UI, e.g.:
            // listing.CheckboxLabeled("Enable feature", ref Settings.SomeFlag, "Tooltip text.");
            listing.End();
            base.DoSettingsWindowContents(inRect);
        }
    }

    public class JanitorsClosetSettings : ModSettings
    {
        // TODO: add settings fields, e.g.:
        // public bool SomeFlag = true;

        public override void ExposeData()
        {
            base.ExposeData();
            // TODO: scribe each field, e.g.:
            // Scribe_Values.Look(ref SomeFlag, "SomeFlag", true);
        }
    }

    [StaticConstructorOnStartup]
    public static class JanitorsClosetInit
    {
        static JanitorsClosetInit()
        {
            new Harmony("terraincognita.janitorscloset").PatchAll();
            Log.Message("[Janitor's Closet] Harmony patches applied.");
        }
    }
}
