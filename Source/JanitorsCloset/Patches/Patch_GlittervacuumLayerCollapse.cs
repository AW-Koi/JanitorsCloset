using System.Collections.Generic;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;
using RimWorld;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // Collapses the targeted Filth to thickness=1 at the start of each cleaning toil when
    // an area-clearing tool (CleaningToolExtension.clearsFilthStack) is driving the work.
    //
    // Why this hook and not Filth.ThinFilth:
    //   JobDriver_CleanFilth's cleaning toil captures `totalCleaningWorkRequired =
    //   cleaningWorkToReduceThickness * thickness` in its initAction, then runs the
    //   progress bar as totalCleaningWorkDone / totalCleaningWorkRequired. If we collapse
    //   thickness inside ThinFilth (which only fires *during* the tickAction), the
    //   denominator is already locked in for the original layer count, so a 3-layer
    //   targeted filth completes after one tick at ~1/3 progress — the bar visibly snaps
    //   instead of running smoothly. By collapsing thickness before initAction runs, the
    //   captured denominator is "one layer's worth," progress runs to 100%, and the
    //   field-collapse fantasy stays layer-agnostic for the targeted filth too.
    //
    // The collapse is idempotent and runs on every toil's initAction, including the goto
    // toil — the cleaning toil's initAction is the one that actually captures the value,
    // and applying it to the goto toil as well is harmless (and guards against vanilla
    // ever reordering its toils).
    [HarmonyPatch(typeof(JobDriver_CleanFilth), "MakeNewToils")]
    public static class Patch_GlittervacuumLayerCollapse
    {
        [HarmonyPostfix]
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> toils, JobDriver_CleanFilth __instance)
        {
            foreach (var toil in toils)
            {
                WrapInit(toil, __instance);
                yield return toil;
            }
        }

        private static void WrapInit(Toil toil, JobDriver_CleanFilth driver)
        {
            var originalInit = toil.initAction;
            toil.initAction = () =>
            {
                CollapseTargetFilthIfArea(driver);
                originalInit?.Invoke();
            };
        }

        private static void CollapseTargetFilthIfArea(JobDriver_CleanFilth driver)
        {
            var weaponDef = driver?.pawn?.equipment?.Primary?.def;
            if (weaponDef == null) return;
            var ext = weaponDef.GetModExtension<CleaningToolExtension>();
            if (ext == null || !ext.clearsFilthStack) return;

            var filth = driver.job?.GetTarget(TargetIndex.A).Thing as Filth;
            if (filth == null || filth.Destroyed) return;
            if (filth.def == JanitorDefOf.Janitor_MopMark) return;

            if (filth.thickness > 1) filth.thickness = 1;
        }
    }
}
