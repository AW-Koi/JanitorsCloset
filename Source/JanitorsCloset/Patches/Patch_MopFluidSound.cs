using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace JanitorsCloset.Patches
{
    // Cleaning sounds are picked by vanilla from the filth being cleaned
    // (Filth_Dirt -> Interact_CleanFilth_Dirt = dry brushy noise; Filth_Vomit -> Fluid).
    // A mop should always sound wet, regardless of the filth, so swap the SoundDef to
    // Interact_CleanFilth_Fluid whenever the active cleaning driver belongs to a
    // mop-equipped pawn.
    //
    // Hook: the Sustainer constructor. Cleaning sounds have <sustain>True</sustain>, so
    // vanilla starts them as Sustainers when the cleaning toil begins. Catching every
    // Sustainer with a cleaning SoundDef as first arg is the simplest spot — we don't have
    // to find the exact toil setup code path or worry about iterator/lambda patching.
    [HarmonyPatch]
    public static class Patch_MopFluidSound
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
                    "the first parameter. Mop cleaning-sound swap will not apply.");
            }
            return ctor;
        }

        public static void Prefix(ref SoundDef __0)
        {
            if (__0 == null) return;

            // Only touch cleaning sustainers so a mop-equipped pawn triggering some unrelated
            // sustained sound (footstep loops, ambient effects, etc.) isn't accidentally swapped.
            if (!__0.defName.StartsWith("Interact_CleanFilth_")) return;
            if (__0 == JanitorDefOf.Interact_CleanFilth_Fluid) return;

            var driver = Patch_TrackCurrentJobDriver.Current as JobDriver_CleanFilth;
            if (driver?.pawn?.equipment?.Primary?.def != JanitorDefOf.Janitor_Mop) return;

            __0 = JanitorDefOf.Interact_CleanFilth_Fluid;
        }
    }
}
