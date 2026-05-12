using HarmonyLib;
using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // The CleaningSpeed bonus from a single-category tool (broom = Dry only, mop = Wet only)
    // is conditional, so the unmodified "Cleaning speed +30%" line on the weapon's info card
    // overstates the bonus. This postfix appends a "(dry filth)" / "(wet filth)" qualifier
    // for those entries.
    //
    // Multi-category tools (Glittervacuum = Dry + Wet) keep the bare label because the bonus
    // really does apply to anything the tool can clean.
    //
    // The pawn's own CleaningSpeed entry on their stat panel is unaffected: the StatRequest
    // there points at the pawn, who has no CleaningToolExtension, so our check no-ops.
    [HarmonyPatch(typeof(StatDrawEntry), nameof(StatDrawEntry.LabelCap), MethodType.Getter)]
    public static class Patch_CleaningSpeedLabel
    {
        public static void Postfix(StatDrawEntry __instance, ref string __result)
        {
            if (__instance.stat != StatDefOf.CleaningSpeed) return;

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
            __result += " " + key.Translate();
        }
    }
}
