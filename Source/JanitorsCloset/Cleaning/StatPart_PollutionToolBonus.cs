using System.Collections.Generic;
using JanitorsCloset.Patches;
using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Adds an equipped Toxic-tagged tool's CleaningSpeed offset to GeneralLaborSpeed while
    // the pawn is actively running JobDriver_ClearPollution. Pollution clearing is terrain
    // work — vanilla scales it by GeneralLaborSpeed * delta, not CleaningSpeed — so this
    // is the right hook to make tools matter for it.
    //
    // Symmetry with StatPart_CleaningToolMatch: that one *subtracts* the CleaningSpeed
    // bonus when a tool's category doesn't match the filth being cleaned. This one *adds*
    // the same magnitude to GeneralLaborSpeed while clearing pollution with a Toxic tool.
    // Net effect for the player: one cohesive "cleaning bonus" number on the tool, applied
    // wherever the tool's declared categories say it should apply.
    //
    // The bonus only shows during an active pollution job for the pawn — generic stat
    // queries (info card, work tab) get vanilla GeneralLaborSpeed back, which is correct:
    // a Hazmat Sprayer doesn't speed up cooking or smithing.
    public class StatPart_PollutionToolBonus : StatPart
    {
        private const int DiagnosticBudget = 20;
        private static int diagApplied;

        public override void TransformValue(StatRequest req, ref float val)
        {
            var bonus = ResolveBonus(req, diag: true);
            if (bonus > 0f) val += bonus;
        }

        public override string ExplanationPart(StatRequest req)
        {
            var bonus = ResolveBonus(req, diag: false);
            if (bonus <= 0f) return null;
            return "JanitorsCloset.CleaningTool.ToxicBonus".Translate()
                   + ": +" + bonus.ToStringPercent();
        }

        private static float ResolveBonus(StatRequest req, bool diag)
        {
            if (!(req.Thing is Pawn pawn)) return 0f;
            var tool = pawn.equipment?.Primary;
            if (tool == null) return 0f;

            var toolExt = tool.def.GetModExtension<CleaningToolExtension>();
            if (toolExt == null || !toolExt.Matches(CleaningCategory.Toxic)) return 0f;

            var driver = Patch_TrackCurrentJobDriver.Current as JobDriver_ClearPollution;
            if (driver == null || driver.pawn != pawn) return 0f;

            var bonus = EquippedCleaningSpeedOffset(tool.def);
            if (bonus > 0f && diag)
            {
                Diag(ref diagApplied,
                    "[JC stat] POLLUTION BONUS pawn='{0}' tool='{1}' +{2}",
                    pawn.LabelShort, tool.def.defName, bonus.ToStringPercent());
            }
            return bonus;
        }

        private static float EquippedCleaningSpeedOffset(ThingDef toolDef)
        {
            List<StatModifier> offsets = toolDef.equippedStatOffsets;
            if (offsets == null) return 0f;
            for (int i = 0; i < offsets.Count; i++)
            {
                if (offsets[i].stat == StatDefOf.CleaningSpeed) return offsets[i].value;
            }
            return 0f;
        }

        private static void Diag(ref int counter, string fmt, params object[] args)
        {
            if (!Prefs.DevMode) return;
            if (counter >= DiagnosticBudget) return;
            counter++;
            Log.Message(string.Format(fmt, args));
            if (counter == DiagnosticBudget)
                Log.Message("[JC stat] pollution-bonus diagnostic budget exhausted — future hits silent.");
        }
    }
}
