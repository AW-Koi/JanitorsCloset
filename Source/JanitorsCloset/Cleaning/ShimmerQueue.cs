using System.Collections.Generic;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Floor twinkles waiting for their tick, so a room's shimmer ripples rather than flashes.
    public class ShimmerQueue
    {
        private readonly List<IntVec3> _cells = new List<IntVec3>();
        private readonly List<int> _dueTicks = new List<int>();

        public int Count => _cells.Count;

        public void Enqueue(IntVec3 cell, int dueTick)
        {
            _cells.Add(cell);
            _dueTicks.Add(dueTick);
        }

        public void TakeDue(int now, List<IntVec3> released)
        {
            for (int i = _cells.Count - 1; i >= 0; i--)
            {
                if (_dueTicks[i] > now) continue;
                released.Add(_cells[i]);
                int last = _cells.Count - 1;
                _cells[i] = _cells[last];
                _dueTicks[i] = _dueTicks[last];
                _cells.RemoveAt(last);
                _dueTicks.RemoveAt(last);
            }
        }
    }
}
