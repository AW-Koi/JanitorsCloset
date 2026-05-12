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
    // Cleaning sounds are picked by vanilla from the filth being cleaned
    // (Filth_Dirt -> Interact_CleanFilth_Dirt = dry brushy noise; Filth_Vomit -> Fluid).
    // A single-category tool should always sound like its medium: a mop should be wet
    // even when scrubbing dust, a push broom should be dry even when sweeping blood.
    // Multi-category tools (Glittervacuum: Dry + Wet) leave the vanilla sound alone since
    // they don't have a single sonic identity.
    //
    // Hook: the Sustainer constructor. Cleaning sounds have <sustain>True</sustain>, so
    // vanilla starts them as Sustainers when the cleaning toil begins. Catching every
    // Sustainer with a cleaning SoundDef as first arg is the simplest spot — we don't have
    // to find the exact toil setup code path or worry about iterator/lambda patching.
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

            // Only touch cleaning sustainers so a tool-equipped pawn triggering some unrelated
            // sustained sound (footstep loops, ambient effects, etc.) isn't accidentally swapped.
            if (!__0.defName.StartsWith("Interact_CleanFilth_")) return;

            var driver = Patch_TrackCurrentJobDriver.Current as JobDriver_CleanFilth;
            var toolDef = driver?.pawn?.equipment?.Primary?.def;
            if (toolDef == null) return;

            var ext = toolDef.GetModExtension<CleaningToolExtension>();
            if (ext?.categories == null || ext.categories.Count != 1) return;

            SoundDef target;
            switch (ext.categories[0])
            {
                case CleaningCategory.Wet:
                    target = JanitorDefOf.Interact_CleanFilth_Fluid;
                    break;
                case CleaningCategory.Dry:
                    target = JanitorDefOf.Interact_CleanFilth_Dirt;
                    break;
                default:
                    return;
            }

            if (target == null || __0 == target) return;
            __0 = target;
        }
    }
}
