namespace JanitorsCloset.Carts
{
    /// <summary>What a janitor knows about their mop and cart when a clean job comes up.</summary>
    public class CartSituation
    {
        public bool MessSuitsTool { get; set; }
        public bool MopSpent { get; set; }
        public bool CartDry { get; set; }
        public bool CartFarFromMess { get; set; }
        public bool HasHome { get; set; }
    }
}
