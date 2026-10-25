using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class BackstoryDefsTests
    {
        private const string BackstoriesFile = "mod/Common/Defs/BackstoryDefs/Backstories_Janitor.xml";

        [Fact]
        public void AdulthoodBackstories_EachOne_NamesABodyTypeForBothSexes()
        {
            var adulthood = XDocument.Load(Path.Combine(RepoRoot(), BackstoriesFile))
                .Descendants("BackstoryDef")
                .Where(b => (string)b.Element("slot") == "Adulthood")
                .ToList();

            var withoutBodyTypes = adulthood
                .Where(b => string.IsNullOrEmpty((string)b.Element("bodyTypeMale")) || string.IsNullOrEmpty((string)b.Element("bodyTypeFemale")))
                .Select(b => (string)b.Element("defName"));

            Assert.NotEmpty(adulthood);
            Assert.Empty(withoutBodyTypes);
        }

        private static string RepoRoot([CallerFilePath] string thisFile = "")
        {
            return Path.GetFullPath(Path.Combine(thisFile, "..", "..", ".."));
        }
    }
}
