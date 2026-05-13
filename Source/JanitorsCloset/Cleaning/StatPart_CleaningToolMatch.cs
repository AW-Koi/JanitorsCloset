using System.Collections.Generic;
using JanitorsCloset.Patches;
using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // CleaningSpeed bonus from an equipped cleaning tool only applies when the tool's
    // declared categories include the filth currently being cleaned. On mismatch we
    // subtract the tool's equipped CleaningSpeed offset back out, leaving the pawn at
    // their vanilla baseline (skills, gear other than the tool, etc.).
    //
    // This is a "bonus-only" gate: a broom-equipped pawn cleaning blood is no worse than
    // an empty-handed pawn cleaning blood — they just don't get the broom's dry-filth boost.
    //
    // We rely on Patch_TrackCurrentJobDriver.Current to identify which filth is the active
    // target. When no cleaning job is active (UI inspections, generic stat queries),
    // the full bonus shows — the inspect window reports the tool's *potential* speed.
    //
    // Diagnostics are gated to the first N hits per branch — useful for confirming the
    // mismatch path actually runs while you're playtesting tools against filth types.
    public class StatPart_CleaningToolMatch : StatPart
    {
        private const int DiagnosticBudget = 20;
        private static int diagMatched;
        private static int diagSuppressed;

        public override void TransformValue(StatRequest req, ref float val)
        {
            var penalty = ResolveOffsetToSuppress(req, diag: true);
            if (penalty > 0f) val -= penalty;
        }

        public override string ExplanationPart(StatRequest req)
        {
            var penalty = ResolveOffsetToSuppress(req, diag: false);
            if (penalty <= 0f) return null;
            return "JanitorsCloset.CleaningTool.WrongFilthCategory".Translate()
                   + ": -" + penalty.ToStringPercent();
        }

        // Returns the equipped CleaningSpeed offset that should be suppressed because the
        // pawn's tool doesn't match the filth they're currently cleaning. Zero means
        // "no suppression" — either no cleaning job is active, the pawn isn't holding
        // a tagged tool, the filth has no category, or the categories already match.
        private static float ResolveOffsetToSuppress(StatRequest req, bool diag)
        {
            if (!(req.Thing is Pawn pawn)) return 0f;
            var tool = pawn.equipment?.Primary;
            if (tool == null) return 0f;

            var toolExt = tool.def.GetModExtension<CleaningToolExtension>();
            if (toolExt == null) return 0f;

            // Only suppress while this pawn is actively cleaning. The thread-static is set
            // during JobDriver_CleanFilth ticks; outside of that, leave the bonus visible.
            var driver = Patch_TrackCurrentJobDriver.Current as JobDriver_CleanFilth;
            if (driver == null || driver.pawn != pawn) return 0f;

            var filth = driver.job?.targetA.Thing as Filth;
            var category = FilthCategoryResolver.Resolve(filth?.def);
            if (category == null) return 0f; // Defensive — no filth target.

            if (toolExt.Matches(category.Value))
            {
                if (diag)
                {
                    Diag(ref diagMatched,
                        "[JC stat] MATCH pawn='{0}' tool='{1}' filth='{2}' cat={3} -> keep bonus",
                        pawn.LabelShort, tool.def.defName, filth.def.defName, category.Value);
                }
                return 0f;
            }

            // Mismatch: subtract the weapon's own equipped CleaningSpeed offset.
            var penalty = EquippedCleaningSpeedOffset(tool.def);
            if (diag)
            {
                Diag(ref diagSuppressed,
                    "[JC stat] MISMATCH pawn='{0}' tool='{1}' filth='{2}' cat={3} -> suppress {4}",
                    pawn.LabelShort, tool.def.defName, filth.def.defName, category.Value, penalty.ToStringPercent());
            }
            return penalty;
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
            if (counter >= DiagnosticBudget) return;
            counter++;
            Log.Message(string.Format(fmt, args));
            if (counter == DiagnosticBudget)
                Log.Message("[JC stat] diagnostic budget exhausted for this branch — future hits silent.");
        }
    }
}
