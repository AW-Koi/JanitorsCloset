using System.Collections.Generic;
using JanitorsCloset.Patches;
using RimWorld;
using Verse;
using JanitorMod = JanitorsCloset.JanitorsCloset;

namespace JanitorsCloset.Cleaning
{
    // Adds an equipped WeatherBuildup-tagged tool's CleaningSpeed offset to
    // GeneralLaborSpeed while the pawn is running JobDriver_ClearSnowAndSand. Snow/sand
    // clearing is terrain work scaled by GeneralLaborSpeed * delta (work-per-tile =
    // 50 * depth), so this is the right stat to ride. Named after vanilla's
    // WeatherBuildupCategory / WeatherBuildupUtility.
    //
    // Mirror of StatPart_PollutionToolBonus. The bonus applies at any depth — confining
    // it to light buildup only confused players ("why is my broom not working?") and the
    // tradeoff was already baked in via the tool's raw CleaningSpeed offset.
    public class StatPart_WeatherBuildupToolBonus : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            var bonus = ResolveBonus(req, diag: true);
            if (bonus > 0f) val += bonus;
        }

        public override string ExplanationPart(StatRequest req)
        {
            var bonus = ResolveBonus(req, diag: false);
            if (bonus <= 0f) return null;
            return "JanitorsCloset.CleaningTool.WeatherBuildupBonus".Translate()
                   + ": +" + bonus.ToStringPercent();
        }

        private static float ResolveBonus(StatRequest req, bool diag)
        {
            if (!(req.Thing is Pawn pawn)) return 0f;
            var tool = pawn.equipment?.Primary;
            if (tool == null) return 0f;

            var toolExt = tool.def.GetModExtension<CleaningToolExtension>();
            if (toolExt == null || !toolExt.Matches(CleaningCategory.WeatherBuildup)) return 0f;

            var driver = Patch_TrackCurrentJobDriver.Current as JobDriver_ClearSnowAndSand;
            if (driver == null || driver.pawn != pawn) return 0f;

            var bonus = EquippedCleaningSpeedOffset(tool.def);
            if (bonus > 0f && diag)
            {
                Diagnostics("[JC stat] WEATHER BUILDUP BONUS pawn='{0}' tool='{1}' +{2}",
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

        private static void Diagnostics(string fmt, params object[] args)
        {
            if (JanitorMod.Settings == null || !JanitorMod.Settings.DebugLogging) return;
            Log.Message(string.Format(fmt, args));
        }
    }
}
