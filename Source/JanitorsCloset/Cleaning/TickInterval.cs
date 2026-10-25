namespace JanitorsCloset.Cleaning
{
    // Fires on the first call, then whenever the summed deltas reach one period.
    public struct TickInterval
    {
        private int _elapsed;
        private bool _started;

        public bool Elapse(int delta, int period)
        {
            if (!_started)
            {
                _started = true;
                return true;
            }

            _elapsed += delta;
            if (_elapsed < period) return false;
            _elapsed -= period;
            if (_elapsed >= period) _elapsed = 0;
            return true;
        }
    }
}
