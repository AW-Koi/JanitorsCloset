using System;
using JanitorsCloset.Patches;
using Verse;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class CleanFilthScanTests : IDisposable
    {
        public CleanFilthScanTests()
        {
            Patch_TrackWorkScanPawn.ScanningPawn = null;
        }

        public void Dispose()
        {
            Patch_TrackWorkScanPawn.ScanningPawn = null;
        }

        [Fact]
        public void WorkScan_WhileRunning_ExposesTheScanningPawn()
        {
            var pawn = new Pawn();

            Patch_TrackWorkScanPawn.Prefix(pawn, out _);

            Assert.Same(pawn, Patch_TrackWorkScanPawn.ScanningPawn);
        }

        [Fact]
        public void WorkScan_AfterFinishing_RestoresThePreviousPawn()
        {
            var outer = new Pawn();
            Patch_TrackWorkScanPawn.ScanningPawn = outer;
            Patch_TrackWorkScanPawn.Prefix(new Pawn(), out var state);

            Patch_TrackWorkScanPawn.Finalizer(state);

            Assert.Same(outer, Patch_TrackWorkScanPawn.ScanningPawn);
        }
    }
}
