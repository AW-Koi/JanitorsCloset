using System;
using System.Collections.Generic;
using Verse;

namespace JanitorsCloset.Cleaning
{
    /// <summary>Freshly cleaned rooms by room ID, each with an anchor cell that survives reloads and remodels.</summary>
    /// <remarks>Room IDs change when rooms rebuild; callers pass a lookup from cell to current room ID, or null.</remarks>
    public class FreshRoomStamps
    {
        private readonly Dictionary<int, FreshRoomStamp> _byRoomId = new Dictionary<int, FreshRoomStamp>();
        private readonly List<int> _staleRoomIds = new List<int>();

        public int Count => _byRoomId.Count;

        public bool IsStamped(int roomId) => _byRoomId.ContainsKey(roomId);

        public void Stamp(int roomId, IntVec3 anchor, int tick) => _byRoomId[roomId] = new FreshRoomStamp(anchor, tick);

        public void Expire(int roomId) => _byRoomId.Remove(roomId);

        public void Prune(Func<IntVec3, int?> roomIdAt)
        {
            _staleRoomIds.Clear();
            foreach (var entry in _byRoomId)
            {
                if (roomIdAt(entry.Value.Anchor) != entry.Key) _staleRoomIds.Add(entry.Key);
            }
            for (int i = 0; i < _staleRoomIds.Count; i++) _byRoomId.Remove(_staleRoomIds[i]);
        }

        public List<IntVec3> Anchors()
        {
            var anchors = new List<IntVec3>(_byRoomId.Count);
            foreach (var stamp in _byRoomId.Values) anchors.Add(stamp.Anchor);
            return anchors;
        }

        public List<int> Ticks()
        {
            var ticks = new List<int>(_byRoomId.Count);
            foreach (var stamp in _byRoomId.Values) ticks.Add(stamp.Tick);
            return ticks;
        }

        public void Restore(List<IntVec3> anchors, List<int> ticks, Func<IntVec3, int?> roomIdAt)
        {
            _byRoomId.Clear();
            if (anchors == null || ticks == null) return;
            for (int i = 0; i < anchors.Count && i < ticks.Count; i++)
            {
                var roomId = roomIdAt(anchors[i]);
                if (roomId.HasValue) Stamp(roomId.Value, anchors[i], ticks[i]);
            }
        }
    }
}
