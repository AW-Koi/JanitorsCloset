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
                "JanitorsCloset.Settings.Diagnostics.Enabled".Translate(),
                ref Settings.DebugEnabled,
                "JanitorsCloset.Settings.Diagnostics.Enabled.Tooltip".Translate());

            if (Settings.DebugEnabled)
            {
                listing.GapLine();
                Text.Font = GameFont.Medium;
                listing.Label("JanitorsCloset.Settings.Diagnostics.Heading".Translate());
                Text.Font = GameFont.Small;
                listing.Gap(2);
                listing.Label("JanitorsCloset.Settings.Diagnostics.Subtitle".Translate());
                listing.Gap(6);

                DiagnosticChannel(listing, "AI",           ref Settings.DebugLogAI);
                DiagnosticChannel(listing, "Anim",         ref Settings.DebugLogAnim);
                DiagnosticChannel(listing, "RenderTrace",  ref Settings.DebugLogRenderTrace);
                DiagnosticChannel(listing, "Stats",        ref Settings.DebugLogStats);
                DiagnosticChannel(listing, "Sound",        ref Settings.DebugLogSound);
                DiagnosticChannel(listing, "TrackedFilth", ref Settings.DebugLogTrackedFilth);
            }

            listing.End();
            base.DoSettingsWindowContents(inRect);
        }

        private static void DiagnosticChannel(Listing_Standard listing, string keySuffix, ref bool field)
        {
            listing.CheckboxLabeled(
                ("JanitorsCloset.Settings.Diagnostics." + keySuffix).Translate(),
                ref field,
                ("JanitorsCloset.Settings.Diagnostics." + keySuffix + ".Tooltip").Translate());
        }
    }

    public class JanitorsClosetSettings : ModSettings
    {
        // Master toggle — when off, all channels are gated off regardless of their
        // individual state. Channel values persist so toggling the master back on
        // restores the user's last selection.
        public bool DebugEnabled;

        public bool DebugLogAI;
        public bool DebugLogAnim;
        public bool DebugLogRenderTrace;
        public bool DebugLogStats;
        public bool DebugLogSound;
        public bool DebugLogTrackedFilth;

        // Gate-site helpers: master AND channel.
        public bool LogAI           => DebugEnabled && DebugLogAI;
        public bool LogAnim         => DebugEnabled && DebugLogAnim;
        public bool LogRenderTrace  => DebugEnabled && DebugLogRenderTrace;
        public bool LogStats        => DebugEnabled && DebugLogStats;
        public bool LogSound        => DebugEnabled && DebugLogSound;
        public bool LogTrackedFilth => DebugEnabled && DebugLogTrackedFilth;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DebugEnabled, "DebugEnabled", false);
            Scribe_Values.Look(ref DebugLogAI, "DebugLogAI", false);
            Scribe_Values.Look(ref DebugLogAnim, "DebugLogAnim", false);
            Scribe_Values.Look(ref DebugLogRenderTrace, "DebugLogRenderTrace", false);
            Scribe_Values.Look(ref DebugLogStats, "DebugLogStats", false);
            Scribe_Values.Look(ref DebugLogSound, "DebugLogSound", false);
            Scribe_Values.Look(ref DebugLogTrackedFilth, "DebugLogTrackedFilth", false);
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
