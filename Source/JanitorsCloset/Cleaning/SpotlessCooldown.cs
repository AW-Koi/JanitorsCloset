using System.Collections.Generic;

namespace JanitorsCloset.Cleaning
{
    // Limits the spotless glint and chime to once per in-game hour for each janitor.
    public class SpotlessCooldown
    {
        public const int CooldownTicks = 2500;

        private readonly Dictionary<int, int> _lastTickByPawnId = new Dictionary<int, int>();

        public bool TryStart(int pawnId, int now)
        {
            if (_lastTickByPawnId.TryGetValue(pawnId, out int last) && now - last < CooldownTicks) return false;
            _lastTickByPawnId[pawnId] = now;
            return true;
        }
    }
}
