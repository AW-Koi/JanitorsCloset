using JanitorsCloset.Cleaning;
using Verse;
using JanitorMod = JanitorsCloset.JanitorsCloset;

namespace JanitorsCloset.Patches
{
    // Logs one line per pawn per scan, not one per filth candidate.
    internal static class CleanFilthBiasDiagnostics
    {
        private static Pawn _lastPawn;
        private static int _lastTick;

        public static void OnceForScan(Pawn pawn, CleaningCategory? preferred)
        {
            if (JanitorMod.Settings?.LogAI != true) return;
            int tick = Find.TickManager?.TicksGame ?? 0;
            if (_lastPawn == pawn && _lastTick == tick) return;
            _lastPawn = pawn;
            _lastTick = tick;

            Log.Message($"[JC bias] scan pawn='{pawn.LabelShort}' tool='{pawn.equipment?.Primary?.def?.defName ?? "<none>"}' preferred={preferred?.ToString() ?? "<none>"}");
        }
    }
}
