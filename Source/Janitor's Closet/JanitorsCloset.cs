using HarmonyLib;
using UnityEngine;
using Verse;

namespace __ASSEMBLY__
{
    public class __ASSEMBLY__ : Mod
    {
        public static __ASSEMBLY__Settings Settings;

        public __ASSEMBLY__(ModContentPack content) : base(content)
        {
            Settings = GetSettings<__ASSEMBLY__Settings>();
        }

        public override string SettingsCategory() => "__MODNAME__";

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

    public class __ASSEMBLY__Settings : ModSettings
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
    public static class __ASSEMBLY__Init
    {
        static __ASSEMBLY__Init()
        {
            new Harmony("__PACKAGEID__").PatchAll();
            Log.Message("[__MODNAME__] Harmony patches applied.");
        }
    }
}
