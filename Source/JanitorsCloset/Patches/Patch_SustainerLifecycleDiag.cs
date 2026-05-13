using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Defs;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace JanitorsCloset.Patches
{
    // Diagnostic-only: logs every Sustainer.End and Sustainer.Cleanup call whose SoundDef
    // is the Glittervacuum cleaning sustainer. Lets us see whether vanilla's clean-toil
    // hard-cuts via Cleanup() (bypassing sustainRelease) or correctly goes via End() first.
    //
    // Field name compatibility: Sustainer.endRealtime exists in 1.5/1.6; we read it via
    // reflection so missing-field at runtime doesn't crash — we just log "?" instead.
    //
    // Remove this file once we know the lifecycle and the real fix is in place.
    public static class Patch_SustainerLifecycleDiag
    {
        private const int DiagnosticBudget = 30;
        private static int diagEnd;
        private static int diagCleanup;
        private static float lastEndTime = -1f;

        // Try several plausible field names — 1.6 may have renamed endRealtime.
        private static readonly FieldInfo EndRealtimeField =
            AccessTools.Field(typeof(Sustainer), "endRealtime")
            ?? AccessTools.Field(typeof(Sustainer), "endTime")
            ?? AccessTools.Field(typeof(Sustainer), "scheduledEndTime");

        private static bool IsOurSound(Sustainer s)
        {
            return s?.def != null && s.def == JanitorDefOf.Janitor_Glittervacuum_Cleaning;
        }

        private static string EndRealtimeStr(Sustainer s)
        {
            if (EndRealtimeField == null) return "?(field missing)";
            try
            {
                var v = EndRealtimeField.GetValue(s);
                return v?.ToString() ?? "null";
            }
            catch
            {
                return "?(read failed)";
            }
        }

        [HarmonyPatch(typeof(Sustainer), nameof(Sustainer.End))]
        public static class End_Diag
        {
            public static void Prefix(Sustainer __instance)
            {
                if (!IsOurSound(__instance)) return;
                if (diagEnd >= DiagnosticBudget) return;
                diagEnd++;
                lastEndTime = Time.realtimeSinceStartup;
                Log.Message($"[JC sustainer] End()  t={lastEndTime:F3}  endField(pre)={EndRealtimeStr(__instance)}");
                if (diagEnd == DiagnosticBudget)
                    Log.Message("[JC sustainer] End() diagnostic budget exhausted.");
            }
        }

        [HarmonyPatch(typeof(Sustainer), "Cleanup")]
        public static class Cleanup_Diag
        {
            public static void Prefix(Sustainer __instance)
            {
                if (!IsOurSound(__instance)) return;
                if (diagCleanup >= DiagnosticBudget) return;
                diagCleanup++;
                float now = Time.realtimeSinceStartup;
                float dt = lastEndTime > 0f ? (now - lastEndTime) : -1f;
                var st = new StackTrace(1, false);
                Log.Message($"[JC sustainer] Cleanup()  t={now:F3}  sinceEnd={dt:F3}s  endField={EndRealtimeStr(__instance)}\n{st}");
                if (diagCleanup == DiagnosticBudget)
                    Log.Message("[JC sustainer] Cleanup() diagnostic budget exhausted.");
            }
        }

        // Also dump the actual fields on Sustainer once at startup so we can see what
        // 1.6 named the end-time tracker (and any related state). One-shot via a static
        // ctor on a tiny init hook.
        [HarmonyPatch(typeof(Sustainer), nameof(Sustainer.Maintain))]
        public static class Maintain_FieldDump
        {
            private static bool dumped;
            public static void Prefix(Sustainer __instance)
            {
                if (dumped || !IsOurSound(__instance)) return;
                dumped = true;
                var fields = typeof(Sustainer).GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var lines = new System.Text.StringBuilder("[JC sustainer] Sustainer fields:\n");
                foreach (var f in fields)
                {
                    lines.Append("  ").Append(f.FieldType.Name).Append(' ').Append(f.Name).Append('\n');
                }
                Log.Message(lines.ToString());
            }
        }
    }
}
