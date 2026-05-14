using Verse;

namespace JanitorsCloset.Cleaning
{
    // Marker filth — cosmetic byproducts of cleaning tools (mop water, decon foam, any
    // future tool's signature deposit). Stamped on the filth ThingDef itself so:
    //   * adding a new tool's marker doesn't need a code change in every consumer
    //   * IsMarkerFilth becomes one extension check instead of a hardcoded def list
    //
    // Consumers treat marker filth as "not a real chore and not a stamp-breaker":
    // skipped from cleaning work queues, ignored by room-cleanliness scans, and
    // excluded from the tracked-filth mood trigger.
    public class JanitorMarkerFilthExtension : DefModExtension
    {
    }
}
