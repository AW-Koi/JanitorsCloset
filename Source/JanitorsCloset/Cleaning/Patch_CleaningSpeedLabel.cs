using HarmonyLib;
using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // The CleaningSpeed bonus from a single-category tool (broom = Dry only, mop = Wet only)
    // is conditional, so the unmodified "Cleaning speed +30%" line on the weapon's info card
    // overstates the bonus. This postfix appends a "(dry filth)" / "(wet filth)" qualifier
    // to that line.
    //
    // Vanilla constructs equipped-offset entries via the label-based StatDrawEntry constructor
    // in some paths (so __instance.stat is null) and via the stat-based one in others. We
    // therefore match by label prefix rather than relying on the stat field — that catches
    // both construction styles.
    //
    // Multi-category tools (Glittervacuum = Dry + Wet) keep the bare label because the bonus
    // really does apply to anything the tool can clean.
    [HarmonyPatch(typeof(StatDrawEntry), nameof(StatDrawEntry.LabelCap), MethodType.Getter)]
    public static class Patch_CleaningSpeedLabel
    {
        public static void Postfix(StatDrawEntry __instance, ref string __result)
        {
            if (string.IsNullOrEmpty(__result)) return;
            if (!__result.StartsWith(StatDefOf.CleaningSpeed.LabelCap)) return;

            var req = __instance.optionalReq;
            var def = req.Def as ThingDef ?? req.Thing?.def;
            if (def == null) return;

            var ext = def.GetModExtension<CleaningToolExtension>();
            if (ext?.categories == null || ext.categories.Count != 1) return;

            string key;
            switch (ext.categories[0])
            {
                case CleaningCategory.Dry:
                    key = "JanitorsCloset.CleaningTool.DryQualifier";
                    break;
                case CleaningCategory.Wet:
                    key = "JanitorsCloset.CleaningTool.WetQualifier";
                    break;
                default:
                    return;
            }

            string qualifier = " " + key.Translate();
            if (__result.Contains(qualifier)) return; // Defensive: don't compound if called twice.
            __result += qualifier;
        }
    }
}
