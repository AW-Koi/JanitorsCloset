using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JanitorsCloset.Defs;
using RimWorld;
using Verse;

namespace JanitorsCloset.Cleaning
{
    /// <summary>Rooms a janitor cleaned to zero filth; colonists inside keep a small mood bonus until filth returns.</summary>
    /// <remarks>Event-driven: cleaning _stamps a room, a filth spawn expires it, and a 500-tick pass refreshes moods.</remarks>
    public class MapComponent_FreshlyCleanedRooms : MapComponent
    {
        private const int RefreshIntervalTicks = 500;

        private static readonly ConditionalWeakTable<Map, MapComponent_FreshlyCleanedRooms> ByMap =
            new ConditionalWeakTable<Map, MapComponent_FreshlyCleanedRooms>();

        private readonly FreshRoomStamps _stamps = new FreshRoomStamps();
        private readonly ShimmerQueue _shimmer = new ShimmerQueue();
        private readonly SpotlessCooldown _spotlessCooldown = new SpotlessCooldown();
        private readonly List<IntVec3> _dueShimmers = new List<IntVec3>();
        private readonly Func<IntVec3, int?> _roomIdAt;

        private List<IntVec3> _savedAnchors;
        private List<int> _savedTicks;

        public MapComponent_FreshlyCleanedRooms(Map map) : base(map)
        {
            _roomIdAt = RoomIdAt;
            ByMap.AddOrUpdate(map, this);
        }

        /// <summary>The map's component, without GetComponent's linear search.</summary>
        public static MapComponent_FreshlyCleanedRooms For(Map map)
        {
            return map != null && ByMap.TryGetValue(map, out var component) ? component : null;
        }

        public bool HasStamps => _stamps.Count > 0;

        public override void FinalizeInit()
        {
            _stamps.Restore(_savedAnchors, _savedTicks, _roomIdAt);
            _savedAnchors = null;
            _savedTicks = null;
        }

        public override void MapComponentTick()
        {
            int now = Find.TickManager.TicksGame;
            if (_shimmer.Count > 0)
            {
                _shimmer.TakeDue(now, _dueShimmers);
                SpotlessMoment.Twinkle(map, _dueShimmers);
                _dueShimmers.Clear();
            }

            // Stagger across maps so multi-map saves don't all scan on the same tick.
            if ((now + map.uniqueID) % RefreshIntervalTicks != 0) return;
            if (_stamps.Count == 0) return;

            _stamps.Prune(_roomIdAt);
            var colonists = map.mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < colonists.Count; i++)
            {
                TryAward(colonists[i]);
            }
        }

        public void OnFilthCleanedInRoom(Room room, IntVec3 anchor, Pawn janitor)
        {
            if (room == null || room.PsychologicallyOutdoors) return;
            if (_stamps.IsStamped(room.ID) || RoomStillHasFilth(room)) return;

            int now = Find.TickManager.TicksGame;
            _stamps.Stamp(room.ID, anchor, now);

            if (janitor != null)
            {
                SpotlessMoment.GrantPride(janitor);
                if (_spotlessCooldown.TryStart(janitor.thingIDNumber, now)) SpotlessMoment.Play(janitor, room, _shimmer, now);
            }

            // Award now to anyone already inside, rather than waiting for the next refresh.
            var colonists = map.mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < colonists.Count; i++)
            {
                var p = colonists[i];
                if (p?.GetRoom() == room) TryAward(p);
            }
        }

        public bool IsRoomStamped(Room room)
        {
            return room != null && _stamps.IsStamped(room.ID);
        }

        public void ExpireStamp(Room room)
        {
            if (room != null) _stamps.Expire(room.ID);
        }

        private int? RoomIdAt(IntVec3 cell)
        {
            if (!cell.InBounds(map)) return null;
            var room = cell.GetRoom(map);
            if (room == null || room.PsychologicallyOutdoors) return null;
            return room.ID;
        }

        private void TryAward(Pawn pawn)
        {
            if (pawn?.needs?.mood == null) return;
            // Janitors get their own pride thought instead of praising their own work.
            if (pawn.equipment?.Primary?.def.GetModExtension<CleaningToolExtension>() != null) return;
            var room = pawn.GetRoom();
            if (room == null || !_stamps.IsStamped(room.ID)) return;
            if (RoomStillHasFilth(room))
            {
                _stamps.Expire(room.ID);
                return;
            }
            if (JanitorDefOf.Janitor_FreshlyCleanedRoom == null) return;
            pawn.needs.mood.thoughts.memories.TryGainMemoryFast(JanitorDefOf.Janitor_FreshlyCleanedRoom);
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
                    if (f == null || MarkerFilth.IsMarker(f.def)) continue;
                    // A region spanning a doorway can list filth from the next room.
                    if (f.GetRoom() != room) continue;
                    return true;
                }
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                _savedAnchors = _stamps.Anchors();
                _savedTicks = _stamps.Ticks();
            }
            Scribe_Collections.Look(ref _savedAnchors, "freshRoomAnchors", LookMode.Value);
            Scribe_Collections.Look(ref _savedTicks, "freshRoomTicks", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                _savedAnchors = null;
                _savedTicks = null;
            }
        }
    }
}
