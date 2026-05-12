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
    // Diagnostic logging is on for the first DiagnosticBudget invocations of each branch,
    // so we can see exactly where the patch bails out if the qualifier isn't appearing.
    // Once the patch is confirmed working in-game, remove the Diag block and the budget
    // tracking — they're only here to flush out the construction path vanilla actually uses.
    [HarmonyPatch(typeof(StatDrawEntry), nameof(StatDrawEntry.LabelCap), MethodType.Getter)]
    public static class Patch_CleaningSpeedLabel
    {
        private const int DiagnosticBudget = 20;
        private static int diagCalls;
        private static int diagPrefixHits;
        private static int diagDefResolved;
        private static int diagExtensionHits;
        private static int diagAppends;

        public static void Postfix(StatDrawEntry __instance, ref string __result)
        {
            Diag(ref diagCalls, "[JC label] enter LabelCap postfix; result='{0}', stat={1}",
                __result, __instance.stat?.defName ?? "<null>");

            if (string.IsNullOrEmpty(__result)) return;

            var cleaningLabel = StatDefOf.CleaningSpeed.LabelCap.RawText;
            if (!__result.StartsWith(cleaningLabel)) return;
            Diag(ref diagPrefixHits, "[JC label] label-prefix matched CleaningSpeed; result='{0}'", __result);

            var req = __instance.optionalReq;
            var def = req.Def as ThingDef ?? req.Thing?.def;
            if (def == null)
            {
                Diag(ref diagDefResolved,
                    "[JC label] optionalReq has no Thing/Def; req.Def={0}, req.Thing={1}",
                    req.Def?.defName ?? "<null>", req.Thing?.def?.defName ?? "<null>");
                return;
            }
            Diag(ref diagDefResolved, "[JC label] resolved def='{0}'", def.defName);

            var ext = def.GetModExtension<CleaningToolExtension>();
            if (ext?.categories == null || ext.categories.Count != 1)
            {
                Diag(ref diagExtensionHits,
                    "[JC label] no single-category extension; ext={0}, count={1}",
                    ext == null ? "<null>" : "present",
                    ext?.categories?.Count ?? -1);
                return;
            }

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
            if (__result.Contains(qualifier)) return;
            __result += qualifier;
            Diag(ref diagAppends, "[JC label] appended qualifier; final='{0}'", __result);
        }

        private static void Diag(ref int counter, string fmt, params object[] args)
        {
            if (counter >= DiagnosticBudget) return;
            counter++;
            Log.Message(string.Format(fmt, args));
            if (counter == DiagnosticBudget)
                Log.Message("[JC label] diagnostic budget exhausted for this branch — future hits silent.");
        }
    }
}
