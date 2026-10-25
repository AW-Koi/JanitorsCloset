using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    /// <summary>The pawn whose work scan is running, so the cleaning scanner can decide per pawn.</summary>
    /// <remarks>WorkGiver_Scanner.Prioritized has no pawn argument; JobGiver_Work is its only reader.</remarks>
    [HarmonyPatch(typeof(JobGiver_Work), nameof(JobGiver_Work.TryIssueJobPackage))]
    [UsedImplicitly]
    public static class Patch_TrackWorkScanPawn
    {
        public static Pawn ScanningPawn;

        public static void Prefix(Pawn pawn, out Pawn __state)
        {
            __state = ScanningPawn;
            ScanningPawn = pawn;
        }

        public static void Finalizer(Pawn __state)
        {
            ScanningPawn = __state;
        }
    }
}
