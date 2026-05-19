using System.Collections.Generic;
using JanitorsCloset.Patches;
using RimWorld;
using Verse;
using JCMod = JanitorsCloset.JanitorsCloset;

namespace JanitorsCloset.Cleaning
{
    // Adds an equipped WeatherBuildup-tagged tool's CleaningSpeed offset to
    // GeneralLaborSpeed while the pawn is running JobDriver_ClearSnowAndSand on a tile
    // within the tool's depth cap. Snow/sand clearing is terrain work scaled by
    // GeneralLaborSpeed * delta (work-per-tile = 50 * depth), so this is the right stat
    // to ride. Named after vanilla's WeatherBuildupCategory / WeatherBuildupUtility,
    // which already bucket depth into Dusting/Thin/Medium/Thick.
    //
    // Mirror of StatPart_PollutionToolBonus, with one extra gate: brooms have a per-tool
    // depth cap (weatherBuildupDepthCap). A straw broom on Thick buildup falls back to
    // vanilla labor speed — and Patch_DrawEquipmentAiming reads the same predicate, so
    // the cleaning anim is suppressed in lockstep with the bonus. Tools with a null cap
    // (Glittervacuum) get the bonus at any depth.
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

            if (!ToolEligibleAt(toolExt, pawn.Map, driver.job?.targetA.Cell ?? IntVec3.Invalid))
                return 0f;

            var bonus = EquippedCleaningSpeedOffset(tool.def);
            if (bonus > 0f && diag)
            {
                Diag("[JC stat] WEATHER BUILDUP BONUS pawn='{0}' tool='{1}' +{2}",
                    pawn.LabelShort, tool.def.defName, bonus.ToStringPercent());
            }
            return bonus;
        }

        // Shared with Patch_DrawEquipmentAiming so the broom anim is on exactly when the
        // bonus is on. The cap is the *upper bound at which the tool still helps*; null
        // means uncapped.
        public static bool ToolEligibleAt(CleaningToolExtension ext, Map map, IntVec3 cell)
        {
            if (ext == null || map == null || !cell.IsValid) return false;
            if (!ext.weatherBuildupDepthCap.HasValue) return true;
            float depth = CellDepth(map, cell);
            return depth <= ext.weatherBuildupDepthCap.Value;
        }

        public static float CellDepth(Map map, IntVec3 cell)
        {
            float snow = map.snowGrid?.GetDepth(cell) ?? 0f;
            float sand = map.sandGrid?.GetDepth(cell) ?? 0f;
            return snow > sand ? snow : sand;
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

        private static void Diag(string fmt, params object[] args)
        {
            if (JCMod.Settings == null || !JCMod.Settings.DebugLogging) return;
            Log.Message(string.Format(fmt, args));
        }
    }
}
