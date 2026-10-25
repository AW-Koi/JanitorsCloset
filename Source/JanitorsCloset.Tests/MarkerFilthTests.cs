using System.Collections.Generic;
using JanitorsCloset.Cleaning;
using RimWorld;
using Verse;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class MarkerFilthTests
    {
        private static readonly ThingDef DirtDef = new ThingDef { defName = "TestDirt" };
        private static readonly ThingDef MopMarkDef = new ThingDef
        {
            defName = "TestMopMark",
            modExtensions = new List<DefModExtension> { new JanitorMarkerFilthExtension() },
        };

        [Fact]
        public void RemoveMarkersAddedSince_WhenMarkersWereAppended_RemovesOnlyTheAppendedMarkers()
        {
            var listedDirt = new Filth { def = DirtDef };
            var listedMark = new Filth { def = MopMarkDef };
            var newDirt = new Filth { def = DirtDef };
            var newMark = new Filth { def = MopMarkDef };
            var homeFilth = new List<Thing> { listedDirt, listedMark, newDirt, newMark };

            MarkerFilth.RemoveMarkersAddedSince(homeFilth, 2);

            Assert.Equal(new List<Thing> { listedDirt, listedMark, newDirt }, homeFilth);
        }

        [Fact]
        public void RemoveMarkersAddedSince_WhenNothingWasAppended_LeavesTheListAlone()
        {
            var listedMark = new Filth { def = MopMarkDef };
            var homeFilth = new List<Thing> { listedMark };

            MarkerFilth.RemoveMarkersAddedSince(homeFilth, 1);

            Assert.Equal(new List<Thing> { listedMark }, homeFilth);
        }
    }
}
