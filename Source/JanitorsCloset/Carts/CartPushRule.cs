namespace JanitorsCloset.Carts
{
    /// <summary>Whether a janitor wheels the cart over before cleaning a cluster of filth.</summary>
    public static class CartPushRule
    {
        public const float Reach = 12f;

        public static bool NeedsPush(bool cartInClusterRoom, float distanceToCluster)
        {
            return !cartInClusterRoom || distanceToCluster > Reach;
        }
    }
}
