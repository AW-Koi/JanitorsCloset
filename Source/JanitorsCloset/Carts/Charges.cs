using Verse;

namespace JanitorsCloset.Carts
{
    /// <summary>A refillable count of uses, such as a mop's wetness or the cart's water.</summary>
    public class Charges : IExposable
    {
        private int capacity;
        private int remaining;

        public Charges()
        {
        }

        public Charges(int capacity)
        {
            this.capacity = capacity;
            remaining = capacity;
        }

        public int Capacity => capacity;
        public int Remaining => remaining;
        public bool Empty => remaining <= 0;

        public bool TryUse()
        {
            if (Empty) return false;
            remaining--;
            return true;
        }

        public void Refill() => remaining = capacity;

        public void ExposeData()
        {
            Scribe_Values.Look(ref capacity, "capacity");
            Scribe_Values.Look(ref remaining, "remaining");
        }
    }
}
