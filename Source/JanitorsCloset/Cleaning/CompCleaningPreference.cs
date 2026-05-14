using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace JanitorsCloset.Cleaning
{
    public enum CleaningAreaPreference : byte
    {
        Any = 0,
        Indoors = 1,
        Outdoors = 2,
    }

    public class CompProperties_CleaningPreference : CompProperties
    {
        public CompProperties_CleaningPreference()
        {
            compClass = typeof(CompCleaningPreference);
        }
    }

    // Per-tool indoor/outdoor cleaning preference. Lives on the weapon, so swapping tools
    // moves the preference with the gear. Existence of the comp doubles as the "is a
    // janitor tool" check used by Patch_CleaningAreaPreference — non-janitor weapons have
    // no comp and are never filtered.
    public class CompCleaningPreference : ThingComp
    {
        public CleaningAreaPreference preference = CleaningAreaPreference.Any;

        public bool Matches(IntVec3 cell, Map map)
        {
            if (preference == CleaningAreaPreference.Any) return true;
            if (map == null || !cell.InBounds(map)) return true;
            var room = cell.GetRoom(map);
            bool outdoors = room == null || room.PsychologicallyOutdoors;
            return preference == CleaningAreaPreference.Outdoors ? outdoors : !outdoors;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref preference, "cleaningAreaPref", CleaningAreaPreference.Any);
        }

        // Surfaced via Patch_CleaningAreaPreference's Pawn_EquipmentTracker.GetGizmos hook —
        // vanilla only invokes CompGetEquippedGizmosExtra on the weapon's CompEquippable,
        // not on every comp, so we can't rely on CompGetGizmosExtra here.
        public Gizmo BuildGizmo()
        {
            return new Command_Action
            {
                defaultLabel = ("JanitorsCloset.CleaningPref.Label." + preference).Translate(),
                defaultDesc = ("JanitorsCloset.CleaningPref.Desc." + preference).Translate(),
                icon = IconFor(preference),
                action = Cycle,
            };
        }

        private void Cycle()
        {
            preference = (CleaningAreaPreference)(((byte)preference + 1) % 3);
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        }

        private static Texture2D IconFor(CleaningAreaPreference pref)
        {
            string path;
            switch (pref)
            {
                case CleaningAreaPreference.Indoors: path = "UI/Commands/Janitor_CleaningPref_Indoors"; break;
                case CleaningAreaPreference.Outdoors: path = "UI/Commands/Janitor_CleaningPref_Outdoors"; break;
                default: path = "UI/Commands/Janitor_CleaningPref_Any"; break;
            }
            return ContentFinder<Texture2D>.Get(path, false) ?? BaseContent.BadTex;
        }
    }
}
