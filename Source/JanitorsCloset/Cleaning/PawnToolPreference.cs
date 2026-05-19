using System;
using Verse;

namespace JanitorsCloset.Cleaning
{
    // Single source of truth for "what filth category does this pawn's equipped tool
    // prefer?". Consumed by the category-bias and queue-reorder patches so future
    // changes to the preference rule touch one file.
    //
    // Returns Dry or Wet only when the tool has *exactly one* of those categories
    // (XOR). Tools with both (Glittervacuum) or neither (Hazmat Sprayer) get null,
    // meaning vanilla closest-first ordering survives untouched.
    //
    // Per-scan cache: WorkGiver_CleanFilth.GetPriority is called once per home-area
    // filth candidate during a single pawn's selection scan. Resolving the tool
    // extension that many times is wasteful. Thread-static cache keyed by
    // (Pawn, TicksGame) reuses the resolution for all candidates within one tick,
    // and auto-invalidates the moment the pawn's tick advances — covers mid-game
    // equipment swaps without needing an explicit invalidation hook.
    public static class PawnToolPreference
    {
        [ThreadStatic] private static Pawn _cachedPawn;
        [ThreadStatic] private static int _cachedTick;
        [ThreadStatic] private static CleaningCategory? _cachedCategory;

        public static CleaningCategory? TryResolvePreferredCategory(Pawn pawn)
        {
            if (pawn == null) return null;
            int tick = Find.TickManager?.TicksGame ?? 0;
            if (_cachedPawn == pawn && _cachedTick == tick) return _cachedCategory;

            var resolved = ResolveUncached(pawn);
            _cachedPawn = pawn;
            _cachedTick = tick;
            _cachedCategory = resolved;
            return resolved;
        }

        private static CleaningCategory? ResolveUncached(Pawn pawn)
        {
            var ext = pawn.equipment?.Primary?.def?.GetModExtension<CleaningToolExtension>();
            if (ext == null) return null;
            bool hasDry = ext.Matches(CleaningCategory.Dry);
            bool hasWet = ext.Matches(CleaningCategory.Wet);
            if (hasDry == hasWet) return null;
            return hasDry ? CleaningCategory.Dry : CleaningCategory.Wet;
        }
    }
}
