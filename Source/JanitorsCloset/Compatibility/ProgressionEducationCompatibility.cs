using System.Linq;
using JanitorsCloset.Defs;
using Verse;

namespace JanitorsCloset.Compatibility
{
    [StaticConstructorOnStartup]
    public static class ProgressionEducationCompatibility
    {
        private const string PackageId = "ferny.ProgressionEducation";
        private const string ExtensionTypeName = "ProgressionEducation.ItemProficiencyRequirement";

        static ProgressionEducationCompatibility()
        {
            if (!ModsConfig.IsActive(PackageId))
            {
                return;
            }

            Log.Message("[Janitor's Closet] Progression: Education detected, verifying proficiency patches.");
            Verify(JanitorDefOf.Janitor_PushBroom);
            Verify(JanitorDefOf.Janitor_Mop);
        }

        private static void Verify(ThingDef def)
        {
            if (def == null) return;

            var ext = def.modExtensions?.FirstOrDefault(e => e.GetType().FullName == ExtensionTypeName);
            if (ext != null)
            {
                Log.Message($"[Janitor's Closet]   {def.defName}: LowTech proficiency override applied.");
            }
            else
            {
                Log.Warning($"[Janitor's Closet]   {def.defName}: LowTech proficiency override MISSING. Pawns may need Firearm proficiency to equip.");
            }
        }
    }
}
