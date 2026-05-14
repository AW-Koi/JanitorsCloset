using System.Collections.Generic;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    // When new filth appears inside a freshly-stamped room, expire the stamp and give
    // any pawn at the cell a "tracked filth into a clean room" memory. Pawn-tracked filth
    // (mud, blood from a wound, vomit) is the main trigger — the pawn was standing where
    // the filth spawned, so they're identifiable as the depositor.
    //
    // Hook is Filth.SpawnSetup so we cover every spawn path (FilthMaker.TryMakeFilth,
    // Pawn_FilthTracker drops, direct construction). Marker filth (mop marks, hazmat
    // foam) is excluded — those are byproducts of cleaning tools themselves and would
    // immediately destroy any stamp the cleaner just placed.
    [HarmonyPatch(typeof(Filth), nameof(Filth.SpawnSetup))]
    public static class Patch_TrackedFilthIntoCleanRoom
    {
        public static void Postfix(Filth __instance)
        {
            if (__instance == null || __instance.Map == null) return;

            var def = __instance.def;
            if (def == JanitorDefOf.Janitor_MopMark) return;
            if (def == JanitorDefOf.Janitor_HazmatFoam) return;

            var map = __instance.Map;
            var comp = map.GetComponent<MapComponent_FreshlyCleanedRooms>();
            if (comp == null) return;

            var room = __instance.Position.GetRoom(map);
            if (!comp.IsRoomStamped(room)) return;

            comp.ExpireStamp(room);

            if (JanitorDefOf.Janitor_TrackedFilthIntoCleanRoom == null) return;

            // Find the depositor — typically the pawn standing on the cell. For filth
            // sources without a pawn (rain leaks, decay), no thought is awarded; the
            // stamp expiry alone is the consequence.
            List<Thing> things = map.thingGrid.ThingsListAtFast(__instance.Position);
            for (int i = 0; i < things.Count; i++)
            {
                var pawn = things[i] as Pawn;
                if (pawn == null) continue;
                if (pawn.needs?.mood == null) continue;
                pawn.needs.mood.thoughts.memories.TryGainMemoryFast(JanitorDefOf.Janitor_TrackedFilthIntoCleanRoom);
            }
        }
    }
}
