namespace JanitorsCloset.Carts
{
    /// <summary>What a janitor with a mop does before cleaning a mess the mop is for: fetch water, wheel the cart over, or wet the mop.</summary>
    public static class CartErrands
    {
        public static CartErrand Next(CartSituation situation)
        {
            if (!situation.MessSuitsTool) return CartErrand.None;
            if (situation.CartDry) return situation.HasHome ? CartErrand.ReturnHome : CartErrand.None;
            if (situation.CartFarFromMess) return CartErrand.Push;
            return situation.MopSpent ? CartErrand.Dunk : CartErrand.None;
        }
    }
}
