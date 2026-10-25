namespace JanitorsCloset.Carts
{
    /// <summary>A cart left unused away from home for four in-game hours goes back to the closet.</summary>
    public static class CartIdleRule
    {
        public const int IdleTicks = 4 * 2500;

        public static bool ShouldGoHome(int ticksIdle, bool atHome) => !atHome && ticksIdle >= IdleTicks;
    }
}
