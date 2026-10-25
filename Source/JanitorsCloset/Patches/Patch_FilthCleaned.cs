using HarmonyLib;
using JanitorsCloset.Carts;
using JanitorsCloset.Cleaning;
using JanitorsCloset.Cleaning.ToolEffects;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    // Runs a tool holder's after-clean effects when the filth they work on is destroyed; rain, fire and age don't count.
    [HarmonyPatch(typeof(Filth), nameof(Filth.ThinFilth))]
    public static class Patch_FilthCleaned
    {
        // Filth.Map goes null on Destroy, so the prefix captures where the filth lay.
        public struct State
        {
            public Map Map;
            public IntVec3 Cell;
            public ThingDef Def;
        }

        public static void Prefix(Filth __instance, out State __state)
        {
            __state = new State
            {
                Map = __instance.Map,
                Cell = __instance.Position,
                Def = __instance.def,
            };
        }

        public static void Postfix(Filth __instance, State __state)
        {
            if (!__instance.Destroyed || __state.Map == null || MarkerFilth.IsMarker(__state.Def)) return;
            if (!(ActiveCleaningDriver.Current is JobDriver_CleanFilth driver)) return;
            var janitor = driver.pawn;
            var toolDef = janitor?.equipment?.Primary?.def;
            var tool = toolDef?.GetModExtension<CleaningToolExtension>();
            if (tool == null) return;

            // Clear the field first, so the room check below sees the whole patch gone.
            if (tool.clearsFilthStack) GlittervacuumFieldCollapse.Clear(__state.Cell, __state.Map);
            if (toolDef == JanitorDefOf.Janitor_Mop) MopMarks.Leave(__state.Cell, __state.Map, janitor);
            if (FilthCategoryResolver.Resolve(__state.Def) == CleaningCategory.Wet) janitor.equipment.Primary.TryGetComp<CompMopCharge>()?.Wetness.TryUse();

            var room = __state.Cell.GetRoom(__state.Map);
            if (room != null) MapComponent_FreshlyCleanedRooms.For(__state.Map)?.OnFilthCleanedInRoom(room, __state.Cell, janitor);
        }
    }
}
