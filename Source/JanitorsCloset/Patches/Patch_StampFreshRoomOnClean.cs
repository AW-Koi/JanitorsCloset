using HarmonyLib;
using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // When a Filth is destroyed by a cleaning JobDriver, ask the FreshlyCleanedRooms
    // MapComponent to check whether this was the *last* filth in the room — and if so,
    // stamp the room as freshly cleaned.
    //
    // Filth.ThinFilth is the per-thickness reduction call; we only act when the filth
    // hit zero thickness (Destroyed). Cell + Map are captured in the prefix because
    // Filth.Map goes null on Destroy.
    //
    // Gated on Patch_TrackCurrentJobDriver.Current so we ignore natural decay paths
    // (rain, fire, age) — only deliberate cleaning work counts as "the room got cleaned".
    [HarmonyPatch(typeof(Filth), nameof(Filth.ThinFilth))]
    public static class Patch_StampFreshRoomOnClean
    {
        public class State
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
            if (!__instance.Destroyed) return;
            if (__state.Map == null) return;

            // Marker filth (mop marks, hazmat foam) are decorative — cleaning one doesn't
            // mean the room got fresher.
            if (__state.Def == JanitorDefOf.Janitor_MopMark) return;
            if (__state.Def == JanitorDefOf.Janitor_HazmatFoam) return;

            // Only count this as "cleaning" if a cleaning JobDriver was actually running.
            // Filth also disappears from rain/fire/age — those shouldn't grant moodlets.
            var driver = Patch_TrackCurrentJobDriver.Current as JobDriver_CleanFilth;
            if (driver == null) return;

            // Restrict the room-fresh moodlet to janitor work — the cleaner must be wielding
            // a tool that declares a CleaningToolExtension. Plain-hands cleaning by any
            // colonist shouldn't broadcast a mood bonus to the whole room.
            var equipped = driver.pawn?.equipment?.Primary?.def;
            if (equipped == null) return;
            if (equipped.GetModExtension<CleaningToolExtension>() == null) return;

            var room = __state.Cell.GetRoom(__state.Map);
            if (room == null) return;

            var comp = __state.Map.GetComponent<MapComponent_FreshlyCleanedRooms>();
            comp?.OnFilthCleanedInRoom(room);
        }
    }
}
