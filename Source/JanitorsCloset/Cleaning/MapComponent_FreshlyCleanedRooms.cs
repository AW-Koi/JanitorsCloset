using System.Collections.Generic;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Tracks rooms whose last non-marker filth was removed. While a room remains in
    // that state, colonists inside it receive (and keep refreshing) the
    // Janitor_FreshlyCleanedRoom memory. The stamp dies only when filth reappears in
    // the room — no fixed timer.
    //
    // Performance shape:
    //   * No per-tick filth scanning. Stamping is event-driven: Patch_StampFreshRoomOnClean
    //     calls OnFilthCleanedInRoom only when a Filth instance is actually destroyed by
    //     a cleaning JobDriver, and the room-emptiness check iterates the room's own
    //     region filth lists (Region.ListerThings is pre-grouped, so this is just a few
    //     ints in the common case).
    //   * Stamp expiry is event-driven too: Patch_TrackedFilthIntoCleanRoom drops the
    //     stamp the moment filth spawns inside it.
    //   * The periodic refresher runs once every RefreshIntervalTicks. It iterates
    //     FreeColonistsSpawned only, and verifies cleanliness only for rooms that
    //     actually have a colonist in them. Skipped entirely when no rooms are stamped.
    //
    // Room.ID is reused as rooms split/merge from wall edits; we accept that resets the
    // bonus on remodel rather than tracking representative cells. Stamps on orphaned
    // room IDs cost a few bytes apiece and never grant a mood (no colonist will ever
    // resolve to them), so we leave them alone.
    public class MapComponent_FreshlyCleanedRooms : MapComponent
    {
        // Scan cadence for the periodic refresher. 500 ticks ≈ 8 in-game seconds at normal
        // speed — fast enough that "walks into a clean room and gets the mood" feels
        // immediate, slow enough that the cost is negligible.
        private const int RefreshIntervalTicks = 500;

        private Dictionary<int, int> roomCleanedTick = new Dictionary<int, int>();

        public MapComponent_FreshlyCleanedRooms(Map map) : base(map) { }

        public override void MapComponentTick()
        {
            // Stagger across maps so multi-map saves don't all scan on the same tick.
            if ((Find.TickManager.TicksGame + map.uniqueID) % RefreshIntervalTicks != 0) return;
            if (roomCleanedTick.Count == 0) return;

            var colonists = map.mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < colonists.Count; i++)
            {
                TryAward(colonists[i]);
            }
        }

        // Called from Patch_StampFreshRoomOnClean when a Filth is destroyed by a cleaning
        // JobDriver. We re-check room cleanliness here rather than maintaining a running
        // count — running counts drift across save/load and wall edits, and the one-shot
        // region scan on cleaning completion is cheap.
        public void OnFilthCleanedInRoom(Room room)
        {
            if (room == null || room.PsychologicallyOutdoors) return;
            if (RoomStillHasFilth(room)) return;

            roomCleanedTick[room.ID] = Find.TickManager.TicksGame;

            // Award immediately to anyone already in the room — the periodic refresher
            // wouldn't reach them for up to RefreshIntervalTicks otherwise.
            var colonists = map.mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < colonists.Count; i++)
            {
                var p = colonists[i];
                if (p?.GetRoom() == room) GrantMemory(p);
            }
        }

        public bool IsRoomStamped(Room room)
        {
            return room != null && roomCleanedTick.ContainsKey(room.ID);
        }

        public void ExpireStamp(Room room)
        {
            if (room == null) return;
            roomCleanedTick.Remove(room.ID);
        }

        private void TryAward(Pawn pawn)
        {
            if (pawn?.needs?.mood == null) return;
            var room = pawn.GetRoom();
            if (room == null) return;
            if (!roomCleanedTick.ContainsKey(room.ID)) return;
            // Re-verify the room is still filth-free before refreshing — corpses, animal
            // mess, or blood trails between cleanings should expire the stamp early.
            // This is one region-filth scan per stamped, occupied room per 500-tick scan.
            if (RoomStillHasFilth(room))
            {
                roomCleanedTick.Remove(room.ID);
                return;
            }
            GrantMemory(pawn);
        }

        private static void GrantMemory(Pawn pawn)
        {
            var memories = pawn?.needs?.mood?.thoughts?.memories;
            if (memories == null) return;
            if (JanitorDefOf.Janitor_FreshlyCleanedRoom == null) return;
            // stackLimit=1 on the ThoughtDef means this refreshes the existing memory's
            // age rather than stacking a new instance.
            memories.TryGainMemoryFast(JanitorDefOf.Janitor_FreshlyCleanedRoom);
        }

        private static bool RoomStillHasFilth(Room room)
        {
            var regions = room.Regions;
            for (int i = 0; i < regions.Count; i++)
            {
                var things = regions[i].ListerThings.ThingsInGroup(ThingRequestGroup.Filth);
                for (int j = 0; j < things.Count; j++)
                {
                    var f = things[j] as Filth;
                    if (f == null) continue;
                    if (MarkerFilth.IsMarker(f.def)) continue;
                    // Region thing lists can include things in adjacent rooms when the
                    // region spans a doorway; confirm the cell actually belongs to this
                    // room before counting it.
                    if (f.GetRoom() != room) continue;
                    return true;
                }
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref roomCleanedTick, "roomCleanedTick", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && roomCleanedTick == null)
            {
                roomCleanedTick = new Dictionary<int, int>();
            }
        }
    }
}
