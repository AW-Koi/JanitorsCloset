using Verse;

namespace JanitorsCloset.Cleaning
{
    // A cell inside the cleaned room and the tick it came clean.
    public struct FreshRoomStamp
    {
        public IntVec3 Anchor;
        public int Tick;

        public FreshRoomStamp(IntVec3 anchor, int tick)
        {
            Anchor = anchor;
            Tick = tick;
        }
    }
}
