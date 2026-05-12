using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Adds a "Cleaning specialty" entry alongside the equipped Cleaning speed offset on the
    // weapon's info card, so the conditional nature of the bonus is visible to the player.
    //
    // We tried two ways of rewriting the existing "Cleaning speed multiplier +X%" line in
    // place: a postfix on StatDrawEntry.LabelCap, and a thread-static set during the iteration
    // of StatsReportUtility.StatsToDraw. Neither worked reliably — vanilla constructs the
    // offset entries with the label-based StatDrawEntry constructor (so optionalReq is empty)
    // and materialises the entry list before rendering (so the thread-static is already gone
    // when LabelCap is queried). The additive route sidesteps both problems.
    //
    // The new line reads e.g. "Cleaning specialty: Dry filth" with a tooltip explaining
    // that the +CleaningSpeed bonus only applies to that filth category. Multi-category
    // tools (Glittervacuum) don't get an entry because their bonus applies universally.
    [HarmonyPatch(typeof(Thing), nameof(Thing.SpecialDisplayStats))]
    public static class Patch_AddCleaningSpecialtyEntry
    {
        public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> entries, Thing __instance)
        {
            foreach (var e in entries) yield return e;

            if (__instance?.def == null) yield break;
            var ext = __instance.def.GetModExtension<CleaningToolExtension>();
            if (ext?.categories == null || ext.categories.Count != 1) yield break;

            var cat = ext.categories[0];
            string valueKey, explanationKey;
            switch (cat)
            {
                case CleaningCategory.Dry:
                    valueKey = "JanitorsCloset.CleaningTool.SpecialtyDry";
                    explanationKey = "JanitorsCloset.CleaningTool.SpecialtyExplanationDry";
                    break;
                case CleaningCategory.Wet:
                    valueKey = "JanitorsCloset.CleaningTool.SpecialtyWet";
                    explanationKey = "JanitorsCloset.CleaningTool.SpecialtyExplanationWet";
                    break;
                default:
                    yield break;
            }

            // EquippedStatOffsets is the right category so our entry sits next to the
            // "Cleaning speed +X%" line. Falls back to Basics if vanilla ever renames it.
            var category = DefDatabase<StatCategoryDef>.GetNamedSilentFail("EquippedStatOffsets")
                           ?? StatCategoryDefOf.Basics;

            yield return new StatDrawEntry(
                category,
                "JanitorsCloset.CleaningTool.SpecialtyLabel".Translate(),
                valueKey.Translate(),
                explanationKey.Translate(),
                5000);
        }
    }
}
