using HarmonyLib;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // When a mop-equipped pawn finishes cleaning a filth tile, drop a Janitor_MopMark on that cell.
    // Hooks Filth.ThinFilth, which is the cleaning toil's per-thickness reduction call. We act only when
    // the filth was destroyed (thickness hit 0), and only when the active cleaning JobDriver
    // belongs to a pawn carrying the mop. Cell + map are captured in the prefix because the
    // filth's Map goes null on Destroy.
    [HarmonyPatch(typeof(Filth), nameof(Filth.ThinFilth))]
    public static class Patch_SpawnMopMark
    {
        public class State
        {
            public Map Map;
            public IntVec3 Cell;
            public ThingDef OriginalDef;
        }

        public static void Prefix(Filth __instance, out State __state)
        {
            __state = new State
            {
                Map = __instance.Map,
                Cell = __instance.Position,
                OriginalDef = __instance.def,
            };
        }

        public static void Postfix(Filth __instance, State __state)
        {
            if (!__instance.Destroyed) return;
            if (__state.Map == null) return;

            // Defensive recursion guard — Patch_SkipCleaningMopMark should already prevent
            // mop marks from being cleaned, so this branch shouldn't fire in normal play.
            if (__state.OriginalDef == JanitorDefOf.Janitor_MopMark) return;

            // Filth can be removed by rain/age/fire too — only spawn marks for cleaning work
            // performed by a mop-equipped cleaner.
            var driver = Patch_TrackCurrentJobDriver.Current as JobDriver_CleanFilth;
            if (driver == null) return;
            if (driver.pawn?.equipment?.Primary?.def != JanitorDefOf.Janitor_Mop) return;

            // One mop mark per tile per cleaning pass — if the cell already has a mark
            // (because the cell had multiple filths and we just cleaned a second one),
            // don't spawn a redundant stacked mark.
            var things = __state.Cell.GetThingList(__state.Map);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i].def == JanitorDefOf.Janitor_MopMark) return;
            }

            FilthMaker.TryMakeFilth(__state.Cell, __state.Map, JanitorDefOf.Janitor_MopMark);
        }
    }
}
