using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;
using Verse.Sound;

namespace JanitorsCloset.Patches
{
    // Forces a tool-specific sustainer for the cleaning sound. Single-category tools get
    // their dry/wet vanilla sustainer; tools with a customCleaningSound on their extension
    // override that entirely. Multi-category tools without a custom sound fall through to
    // the vanilla per-filth selection.
    //
    // Diagnostics are gated to the first N hits per branch — confirm the swap is firing
    // and observe which target SoundDef we chose for the Glittervacuum.
    [HarmonyPatch]
    public static class Patch_CleaningToolSound
    {
        private const int DiagnosticBudget = 20;
        private static int diagNoTool;
        private static int diagNoExtension;
        private static int diagSwapped;
        private static int diagNoSwap;

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
            // Catch both the per-filth sustainers (Interact_CleanFilth_Fluid /
            // Interact_CleanFilth_Dirt) used by JobDriver_CleanFilth and the bare
            // Interact_CleanFilth that JobDriver_ClearPollution plays.
            if (!__0.defName.StartsWith("Interact_CleanFilth")) return;

            // Tracked driver may be either a filth-clean or pollution-clear job — both
            // are tracked by Patch_TrackCurrentJobDriver. We just need a pawn off of it.
            var driver = Patch_TrackCurrentJobDriver.Current;
            Pawn pawn = null;
            if (driver is JobDriver_CleanFilth filthDriver) pawn = filthDriver.pawn;
            else if (driver is JobDriver_ClearPollution pollutionDriver) pawn = pollutionDriver.pawn;
            var toolDef = pawn?.equipment?.Primary?.def;
            if (toolDef == null)
            {
                Diag(ref diagNoTool, "[JC sound] cleaning sustainer with no tool equipped; incoming='{0}'", __0.defName);
                return;
            }

            var ext = toolDef.GetModExtension<CleaningToolExtension>();
            if (ext == null)
            {
                Diag(ref diagNoExtension,
                    "[JC sound] tool='{0}' has no CleaningToolExtension; incoming='{1}'",
                    toolDef.defName, __0.defName);
                return;
            }

            // Sound swap is a filth-cleaning specialty thing. A tool whose declared
            // categories are purely Toxic (Hazmat Sprayer) isn't a filth specialist — its
            // bonus is suppressed on filth and only fires during pollution-clearing work,
            // which uses its own vanilla sustainer. Silently leave the incoming sound alone.
            if (ext.customCleaningSound == null && !HasFilthCleaningCategory(ext)) return;

            SoundDef target = null;

            if (ext.customCleaningSound != null)
            {
                target = ext.customCleaningSound;
            }
            else if (ext.categories != null && ext.categories.Count == 1)
            {
                switch (ext.categories[0])
                {
                    case CleaningCategory.Wet:
                        target = JanitorDefOf.Interact_CleanFilth_Fluid;
                        break;
                    case CleaningCategory.Dry:
                        target = JanitorDefOf.Interact_CleanFilth_Dirt;
                        break;
                }
            }

            if (target == null)
            {
                Diag(ref diagNoSwap,
                    "[JC sound] tool='{0}' no target sound (custom={1}, categories={2}); leaving incoming='{3}'",
                    toolDef.defName,
                    ext.customCleaningSound?.defName ?? "<null>",
                    ext.categories?.Count ?? -1,
                    __0.defName);
                return;
            }
            if (__0 == target)
            {
                Diag(ref diagNoSwap,
                    "[JC sound] tool='{0}' incoming already matches target='{1}', no swap",
                    toolDef.defName, target.defName);
                return;
            }

            Diag(ref diagSwapped,
                "[JC sound] tool='{0}' SWAP incoming='{1}' -> target='{2}'",
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

        private static void Diag(ref int counter, string fmt, params object[] args)
        {
            if (counter >= DiagnosticBudget) return;
            counter++;
            Log.Message(string.Format(fmt, args));
            if (counter == DiagnosticBudget)
                Log.Message("[JC sound] diagnostic budget exhausted for this branch — future hits silent.");
        }
    }
}
