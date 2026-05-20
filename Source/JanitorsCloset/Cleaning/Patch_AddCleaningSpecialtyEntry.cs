using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Adds "Cleaning specialty" entries alongside the equipped Cleaning speed offset on the
    // weapon's info card, so the conditional / unusual aspects of the bonus are visible to
    // the player. Multiple entries can stack on one tool — e.g. a broom shows both
    // "Dry filth" and "Weather buildup". Each entry is informational: hover for the rules.
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

            // Count only filth categories — WeatherBuildup is an auxiliary capability
            // that doesn't change what kind of filth the tool specialises in. A broom
            // declaring [Dry, WeatherBuildup] is still a single-filth-category tool.
            var filthCategories = ext.categories?
                .Where(c => c == CleaningCategory.Dry || c == CleaningCategory.Wet || c == CleaningCategory.Toxic)
                .ToList();
            if (filthCategories != null && filthCategories.Count == 1)
            {
                string valueKey = null, explanationKey = null;
                switch (filthCategories[0])
                {
                    case CleaningCategory.Dry:
                        valueKey = "JanitorsCloset.CleaningTool.SpecialtyDry";
                        explanationKey = "JanitorsCloset.CleaningTool.SpecialtyExplanationDry";
                        break;
                    case CleaningCategory.Wet:
                        valueKey = "JanitorsCloset.CleaningTool.SpecialtyWet";
                        explanationKey = "JanitorsCloset.CleaningTool.SpecialtyExplanationWet";
                        break;
                    case CleaningCategory.Toxic:
                        valueKey = "JanitorsCloset.CleaningTool.SpecialtyToxic";
                        explanationKey = "JanitorsCloset.CleaningTool.SpecialtyExplanationToxic";
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

            if (ext.Matches(CleaningCategory.WeatherBuildup))
            {
                yield return new StatDrawEntry(
                    category,
                    "JanitorsCloset.CleaningTool.SpecialtyLabel".Translate(),
                    "JanitorsCloset.CleaningTool.SpecialtyWeatherBuildup".Translate(),
                    "JanitorsCloset.CleaningTool.SpecialtyExplanationWeatherBuildup".Translate(),
                    4999);
            }

            if (ext.clearsFilthStack)
            {
                yield return new StatDrawEntry(
                    category,
                    "JanitorsCloset.CleaningTool.SpecialtyLabel".Translate(),
                    "JanitorsCloset.CleaningTool.SpecialtyStackClear".Translate(),
                    "JanitorsCloset.CleaningTool.SpecialtyExplanationStackClear".Translate(),
                    4998);
            }
        }
    }
}
