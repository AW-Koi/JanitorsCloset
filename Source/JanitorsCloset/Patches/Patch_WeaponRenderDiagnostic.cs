using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using JanitorMod = JanitorsCloset.JanitorsCloset;

namespace JanitorsCloset.Patches
{
    // Diagnostic-only: hook the call upstream of DrawEquipmentAiming to see whether
    // vanilla even reaches the weapon-draw step during snow/sand cleaning. If this
    // logs but Patch_DrawEquipmentAiming doesn't, vanilla early-outs *inside*
    // DrawEquipmentAndApparelExtras (likely on CarryTracker.CarriedThing != null or
    // a posture check). If even this doesn't log, the entire weapon draw path is
    // being skipped further upstream and we need to patch the render-node worker
    // instead. Gated on DebugLogging; one line per (pawn, state) change.
    [HarmonyPatch]
    [UsedImplicitly]
    public static class Patch_WeaponRenderDiagnostic
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(PawnRenderUtility), "DrawEquipmentAndApparelExtras");
        }

        public static void Prefix(Pawn pawn)
        {
            if (JanitorMod.Settings == null || !JanitorMod.Settings.DebugLogging) return;
            if (pawn?.equipment?.Primary?.def?.GetModExtension<CleaningToolExtension>() == null) return;
            WeaponRenderDiagnostics.Note(pawn, "DrawEquipmentAndApparelExtras-entered");
        }

        private static class WeaponRenderDiagnostics
        {
            [ThreadStatic] private static Dictionary<Pawn, string> _lastByPawn;

            public static void Note(Pawn pawn, string stage)
            {
                if (pawn == null) return;
                var driverName = pawn.jobs?.curDriver?.GetType().Name ?? "<null>";
                var jobName = pawn.CurJobDef?.defName ?? "<null>";
                var carried = pawn.carryTracker?.CarriedThing?.def?.defName ?? "<none>";
                string key = stage + "|" + jobName + "|" + driverName + "|carry=" + carried;
                var map = _lastByPawn ?? (_lastByPawn = new Dictionary<Pawn, string>());
                if (map.TryGetValue(pawn, out var last) && last == key) return;
                map[pawn] = key;
                Log.Message(string.Format(
                    "[JC render] pawn='{0}' stage={1} jobDef='{2}' driver='{3}' carrying='{4}'",
                    pawn.LabelShort, stage, jobName, driverName, carried));
            }
        }
    }
}
