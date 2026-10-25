using System.Collections.Generic;
using JanitorsCloset.Cleaning;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class PawnToolPreferenceTests
    {
        [Theory]
        [InlineData(new[] { CleaningCategory.Dry, CleaningCategory.WeatherBuildup }, CleaningCategory.Dry)]
        [InlineData(new[] { CleaningCategory.Wet }, CleaningCategory.Wet)]
        public void PreferredCategory_WithExactlyOneFilthCategory_ReturnsIt(CleaningCategory[] categories, CleaningCategory expected)
        {
            var tool = new CleaningToolExtension { categories = new List<CleaningCategory>(categories) };

            Assert.Equal(expected, PawnToolPreference.PreferredCategory(tool));
        }

        [Theory]
        [InlineData(new[] { CleaningCategory.Dry, CleaningCategory.Wet, CleaningCategory.Toxic })]
        [InlineData(new[] { CleaningCategory.Toxic })]
        [InlineData(new CleaningCategory[0])]
        public void PreferredCategory_WithBothOrNeitherFilthCategory_ReturnsNull(CleaningCategory[] categories)
        {
            var tool = new CleaningToolExtension { categories = new List<CleaningCategory>(categories) };

            Assert.Null(PawnToolPreference.PreferredCategory(tool));
        }

        [Fact]
        public void PreferredCategory_WithoutATool_ReturnsNull()
        {
            Assert.Null(PawnToolPreference.PreferredCategory(null));
        }
    }
}
