using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Adds "Cleaning specialty" entries alongside the equipped Cleaning speed offset on the
    // weapon's info card, so the conditional / unusual aspects of the bonus are visible to
    // the player.
    //
    // We tried two ways of rewriting the existing "Cleaning speed multiplier +X%" line in
    // place: a postfix on StatDrawEntry.LabelCap, and a thread-static set during the iteration
    // of StatsReportUtility.StatsToDraw. Neither worked reliably — vanilla constructs the
    // offset entries with the label-based StatDrawEntry constructor (so optionalReq is empty)
    // and materialises the entry list before rendering (so the thread-static is already gone
    // when LabelCap is queried). The additive route sidesteps both problems.
    //
    // Two kinds of entry can be emitted:
    //   - Filth-category specialty (e.g. "Dry filth only") for single-category tools whose
    //     bonus is conditional on filth type.
    //   - Stack-clearing trait (clearsFilthStack) for tools that vaporise the whole filth
    //     stack on a cell in one pass — a structural behavior, not a conditional restriction.
    // A tool with both flags gets both entries.
    [HarmonyPatch(typeof(Thing), nameof(Thing.SpecialDisplayStats))]
    public static class Patch_AddCleaningSpecialtyEntry
    {
        public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> entries, Thing __instance)
        {
            foreach (var e in entries) yield return e;

            if (__instance?.def == null) yield break;
            var ext = __instance.def.GetModExtension<CleaningToolExtension>();
            if (ext == null) yield break;

            // EquippedStatOffsets is the right category so our entries sit next to the
            // "Cleaning speed +X%" line. Falls back to Basics if vanilla ever renames it.
            var category = DefDatabase<StatCategoryDef>.GetNamedSilentFail("EquippedStatOffsets")
                           ?? StatCategoryDefOf.Basics;

            if (ext.categories != null && ext.categories.Count == 1)
            {
                string valueKey = null, explanationKey = null;
                switch (ext.categories[0])
                {
                    case CleaningCategory.Dry:
                        valueKey = "JanitorsCloset.CleaningTool.SpecialtyDry";
                        explanationKey = "JanitorsCloset.CleaningTool.SpecialtyExplanationDry";
                        break;
                    case CleaningCategory.Wet:
                        valueKey = "JanitorsCloset.CleaningTool.SpecialtyWet";
                        explanationKey = "JanitorsCloset.CleaningTool.SpecialtyExplanationWet";
                        break;
                }
                if (valueKey != null)
                {
                    yield return new StatDrawEntry(
                        category,
                        "JanitorsCloset.CleaningTool.SpecialtyLabel".Translate(),
                        valueKey.Translate(),
                        explanationKey.Translate(),
                        5000);
                }
            }

            if (ext.clearsFilthStack)
            {
                yield return new StatDrawEntry(
                    category,
                    "JanitorsCloset.CleaningTool.SpecialtyLabel".Translate(),
                    "JanitorsCloset.CleaningTool.SpecialtyStackClear".Translate(),
                    "JanitorsCloset.CleaningTool.SpecialtyExplanationStackClear".Translate(),
                    4999);
            }
        }
    }
}
