using System.Collections.Generic;
using JanitorsCloset.Cleaning;
using RimWorld;
using Verse;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class FilthCategoryResolverTests
    {
        [Theory]
        [InlineData(CleaningCategory.Wet)]
        [InlineData(CleaningCategory.Dry)]
        public void Resolve_WhenTheFilthHasACategoryExtension_ReturnsItsCategoryWhateverTheSound(CleaningCategory tagged)
        {
            var filth = new ThingDef
            {
                defName = "TestTaggedFilth" + tagged,
                filth = new FilthProperties { cleaningSound = new SoundDef { defName = "TestUnrelatedSound" } },
                modExtensions = new List<DefModExtension> { new FilthCategoryExtension { category = tagged } },
            };

            var resolved = FilthCategoryResolver.Resolve(filth);

            Assert.Equal(tagged, resolved);
        }

        [Fact]
        public void Resolve_ForNoDef_ReturnsNull()
        {
            Assert.Null(FilthCategoryResolver.Resolve(null));
        }
    }
}
