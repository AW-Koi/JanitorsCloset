using System.Collections.Generic;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Tracks rooms whose last non-marker filth was just removed. While a room is
    // "freshly cleaned", colonists inside it receive (and keep refreshing) the
    // Janitor_FreshlyCleanedRoom memory.
    //
    // Performance shape:
    //   * No per-tick filth scanning. Stamping is event-driven: Patch_StampFreshRoomOnClean
    //     calls OnFilthCleanedInRoom only when a Filth instance is actually destroyed by
    //     a cleaning JobDriver, and the room-emptiness check iterates the room's own
    //     region filth lists (Region.ListerThings is pre-grouped, so this is just a few
    //     ints in the common case).
    //   * The periodic refresher runs once every RefreshIntervalTicks. It iterates
    //     FreeColonistsSpawned only (small list) and calls GetRoom (O(1) via region grid)
    //     + a dictionary lookup per pawn. Skipped entirely when no rooms are stamped.
    //   * Stamp expiry is folded into the same throttled tick — no separate cleanup pass.
    //
    // Room.ID is reused as rooms split/merge from wall edits; we accept that resets the
    // bonus on remodel rather than tracking representative cells.
    public class MapComponent_FreshlyCleanedRooms : MapComponent
    {
        // How long after a clean a room continues to grant the memory to entrants.
        // 5000 ticks ≈ 2 in-game hours; latecomers within this window still pick it up,
        // and pawns who linger get their memory refreshed continuously.
        private const int FreshnessDurationTicks = 5000;

        // Scan cadence for the periodic refresher. 500 ticks ≈ 8 in-game seconds at normal
        // speed — fast enough that "walks into a clean room and gets the mood" feels
        // immediate, slow enough that the cost is negligible.
        private const int RefreshIntervalTicks = 500;

        private Dictionary<int, int> roomCleanedTick = new Dictionary<int, int>();
        private static readonly List<int> tmpExpiredKeys = new List<int>();

        public MapComponent_FreshlyCleanedRooms(Map map) : base(map) { }

        public override void MapComponentTick()
        {
            // Stagger across maps so multi-map saves don't all scan on the same tick.
            if ((Find.TickManager.TicksGame + map.uniqueID) % RefreshIntervalTicks != 0) return;
            if (roomCleanedTick.Count == 0) return;

            int now = Find.TickManager.TicksGame;

            tmpExpiredKeys.Clear();
            foreach (var kvp in roomCleanedTick)
            {
                if (now - kvp.Value > FreshnessDurationTicks) tmpExpiredKeys.Add(kvp.Key);
            }
            for (int i = 0; i < tmpExpiredKeys.Count; i++) roomCleanedTick.Remove(tmpExpiredKeys[i]);

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
                    if (IsMarkerFilth(f.def)) continue;
                    // Region thing lists can include things in adjacent rooms when the
                    // region spans a doorway; confirm the cell actually belongs to this
                    // room before counting it.
                    if (f.GetRoom() != room) continue;
                    return true;
                }
            }
            return false;
        }

        private static bool IsMarkerFilth(ThingDef def)
        {
            return def == JanitorDefOf.Janitor_MopMark || def == JanitorDefOf.Janitor_HazmatFoam;
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
