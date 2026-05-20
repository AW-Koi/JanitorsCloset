using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;
using Verse.Sound;
using JanitorMod = JanitorsCloset.JanitorsCloset;

namespace JanitorsCloset.Patches
{
    // Forces a tool-specific sustainer for the cleaning sound. Single-category tools get
    // their dry/wet vanilla sustainer; tools with a customCleaningSound on their extension
    // override that entirely. Multi-category tools without a custom sound fall through to
    // the vanilla per-filth selection.
    [HarmonyPatch]
    public static class Patch_CleaningToolSound
    {
        public static MethodBase TargetMethod()
        {
            var ctor = typeof(Sustainer)
                .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length > 0
                                     && c.GetParameters()[0].ParameterType == typeof(SoundDef));
            if (ctor == null)
            {
                throw new InvalidOperationException(
                    "[Janitor's Closet] Could not find a Sustainer constructor taking SoundDef as " +
                    "the first parameter. Cleaning-sound swap will not apply.");
            }
            return ctor;
        }

        public static void Prefix(ref SoundDef __0)
        {
            if (__0 == null) return;
            // Catch the per-filth sustainers (Interact_CleanFilth_Fluid /
            // Interact_CleanFilth_Dirt) used by JobDriver_CleanFilth, plus the bare
            // Interact_CleanFilth that JobDriver_ClearPollution and JobDriver_ClearSnowAndSand
            // both play.
            if (!__0.defName.StartsWith("Interact_CleanFilth")) return;

            // Tracked driver covers filth, pollution, and weather-buildup jobs.
            var driver = Patch_TrackCurrentJobDriver.Current;
            Pawn pawn = null;
            bool isWeatherBuildupJob = false;
            if (driver is JobDriver_CleanFilth filthDriver) pawn = filthDriver.pawn;
            else if (driver is JobDriver_ClearPollution pollutionDriver) pawn = pollutionDriver.pawn;
            else if (driver is JobDriver_ClearSnowAndSand snowDriver)
            {
                pawn = snowDriver.pawn;
                isWeatherBuildupJob = true;
            }
            var toolDef = pawn?.equipment?.Primary?.def;
            if (toolDef == null)
            {
                Diagnostics("[JC sound] cleaning sustainer with no tool equipped; incoming='{0}'", __0.defName);
                return;
            }

            var ext = toolDef.GetModExtension<CleaningToolExtension>();
            if (ext == null)
            {
                Diagnostics("[JC sound] tool='{0}' has no CleaningToolExtension; incoming='{1}'",
                    toolDef.defName, __0.defName);
                return;
            }

            // Weather-buildup work: only swap when the tool is actually a buildup-clearing
            // specialist. A mop running a snow job stays on the vanilla sustainer.
            if (isWeatherBuildupJob && !ext.Matches(CleaningCategory.WeatherBuildup)) return;

            // Sound swap is a filth/buildup-specialty thing. A tool whose declared
            // categories are purely Toxic (Hazmat Sprayer) isn't a filth specialist — its
            // bonus is suppressed on filth and only fires during pollution-clearing work,
            // which uses its own vanilla sustainer. Silently leave the incoming sound alone.
            if (ext.customCleaningSound == null && !HasFilthCleaningCategory(ext)) return;

            SoundDef target = null;

            if (ext.customCleaningSound != null)
            {
                target = ext.customCleaningSound;
            }
            else
            {
                // Filth-cleaning category drives the sustainer. WeatherBuildup and Toxic
                // are non-filth tags; we ignore them here. A tool that's both Dry and Wet
                // is ambiguous — fall through to no swap rather than guess.
                bool hasDry = ext.Matches(CleaningCategory.Dry);
                bool hasWet = ext.Matches(CleaningCategory.Wet);
                if (hasDry && !hasWet) target = JanitorDefOf.Interact_CleanFilth_Dirt;
                else if (hasWet && !hasDry) target = JanitorDefOf.Interact_CleanFilth_Fluid;
            }

            if (target == null)
            {
                Diagnostics("[JC sound] tool='{0}' no target sound (custom={1}, categories={2}); leaving incoming='{3}'",
                    toolDef.defName,
                    ext.customCleaningSound?.defName ?? "<null>",
                    ext.categories?.Count ?? -1,
                    __0.defName);
                return;
            }
            if (__0 == target)
            {
                Diagnostics("[JC sound] tool='{0}' incoming already matches target='{1}', no swap",
                    toolDef.defName, target.defName);
                return;
            }

            Diagnostics("[JC sound] tool='{0}' SWAP incoming='{1}' -> target='{2}'",
                toolDef.defName, __0.defName, target.defName);
            __0 = target;
        }

        private static bool HasFilthCleaningCategory(CleaningToolExtension ext)
        {
            if (ext.categories == null) return false;
            for (int i = 0; i < ext.categories.Count; i++)
            {
                var c = ext.categories[i];
                if (c == CleaningCategory.Wet || c == CleaningCategory.Dry) return true;
            }
            return false;
        }

        private static void Diagnostics(string fmt, params object[] args)
        {
            if (JanitorMod.Settings == null || !JanitorMod.Settings.DebugLogging) return;
            Log.Message(string.Format(fmt, args));
        }
    }
}
