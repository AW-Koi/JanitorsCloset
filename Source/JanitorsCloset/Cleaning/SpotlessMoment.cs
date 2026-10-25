using System.Collections.Generic;
using JanitorsCloset.Defs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace JanitorsCloset.Cleaning
{
    /// <summary>The janitor's reward when a room comes clean: a pride thought, a glint, a faint chime and a floor shimmer.</summary>
    /// <remarks>Subtle by design: one small glint, a chime audible only up close, at most ten dim twinkles.</remarks>
    public static class SpotlessMoment
    {
        private const int CellsPerShimmer = 12;
        private const int MaxShimmers = 10;
        private const int MaxShimmerRoomCells = 240;
        private const int ShimmerSpreadTicks = 110;
        private static readonly Vector3 GlintAboveHead = new Vector3(0f, 0f, 0.8f);

        public static int ShimmerCount(int roomCellCount)
        {
            if (roomCellCount > MaxShimmerRoomCells) return 0;
            return Mathf.Clamp(roomCellCount / CellsPerShimmer, 1, MaxShimmers);
        }

        public static void GrantPride(Pawn janitor)
        {
            if (JanitorDefOf.Janitor_SpotlessWork == null) return;
            janitor.needs?.mood?.thoughts?.memories?.TryGainMemoryFast(JanitorDefOf.Janitor_SpotlessWork);
        }

        public static void Play(Pawn janitor, Room room, ShimmerQueue shimmer, int now)
        {
            var map = janitor.Map;
            if (map == null) return;

            FleckMaker.Static(janitor.DrawPos + GlintAboveHead, map, JanitorDefOf.Janitor_SpotlessGlint, 0.6f);
            JanitorDefOf.Janitor_SpotlessChime?.PlayOneShot(new TargetInfo(janitor.Position, map));

            int count = ShimmerCount(room.CellCount);
            for (int i = 0; i < count; i++)
            {
                shimmer.Enqueue(room.Cells.RandomElement(), now + Rand.Range(4, ShimmerSpreadTicks));
            }
        }

        public static void Twinkle(Map map, List<IntVec3> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                var jitter = new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
                FleckMaker.Static(cells[i].ToVector3Shifted() + jitter, map, JanitorDefOf.Janitor_SpotlessShimmer, Rand.Range(0.35f, 0.55f));
            }
        }
    }
}
