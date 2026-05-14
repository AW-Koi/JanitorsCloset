using System.Collections.Generic;
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

        // Stored by label rather than reference: areas are map-scoped but the tool travels
        // with the pawn across maps. On lookup we resolve against the filth's map, so the
        // restriction follows the player's "Home"-equivalent on each map. If the area is
        // renamed or deleted the resolve returns null and we treat it as no restriction.
        public string areaLabel;

        public Area ResolveArea(Map map)
        {
            if (map == null || string.IsNullOrEmpty(areaLabel)) return null;
            return map.areaManager.GetLabeled(areaLabel);
        }

        public bool Matches(IntVec3 cell, Map map)
        {
            if (map == null || !cell.InBounds(map)) return true;
            if (preference != CleaningAreaPreference.Any)
            {
                var room = cell.GetRoom(map);
                bool outdoors = room == null || room.PsychologicallyOutdoors;
                bool prefOk = preference == CleaningAreaPreference.Outdoors ? outdoors : !outdoors;
                if (!prefOk) return false;
            }
            var area = ResolveArea(map);
            if (area != null && !area[cell]) return false;
            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref preference, "cleaningAreaPref", CleaningAreaPreference.Any);
            Scribe_Values.Look(ref areaLabel, "cleaningAreaLabel");
        }

        // Surfaced via Patch_CleaningAreaPreference's Pawn_EquipmentTracker.GetGizmos hook —
        // vanilla only invokes CompGetEquippedGizmosExtra on the weapon's CompEquippable,
        // not on every comp, so we can't rely on CompGetGizmosExtra here.
        public IEnumerable<Gizmo> BuildGizmos()
        {
            yield return BuildIndoorOutdoorGizmo();
            yield return BuildAreaGizmo();
        }

        private Gizmo BuildIndoorOutdoorGizmo()
        {
            return new Command_Action
            {
                defaultLabel = ("JanitorsCloset.CleaningPref.Label." + preference).Translate(),
                defaultDesc = ("JanitorsCloset.CleaningPref.Desc." + preference).Translate(),
                icon = IconFor(preference),
                action = Cycle,
            };
        }

        private Gizmo BuildAreaGizmo()
        {
            var map = parent.MapHeld;
            var area = ResolveArea(map);
            return new Command_AreaTinted
            {
                defaultLabel = area != null
                    ? "JanitorsCloset.CleaningArea.Label.Restricted".Translate(area.Label)
                    : "JanitorsCloset.CleaningArea.Label.None".Translate(),
                defaultDesc = area != null
                    ? "JanitorsCloset.CleaningArea.Desc.Restricted".Translate(area.Label)
                    : "JanitorsCloset.CleaningArea.Desc.None".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Janitor_CleaningPref_Area", false) ?? BaseContent.BadTex,
                bgColor = area != null ? area.Color : Color.white,
                action = () => OpenAreaPicker(map),
            };
        }

        // Vanilla GizmoOnGUIInt forces GUI.color to white right before drawing the BG, so a
        // GUI.color wrapper around base does nothing. Returning a solid-color texture for
        // BGTexture paints the gizmo's background as the area's exact color — easier to
        // recognise at a glance than a tint multiplied over DesButBG. Cached by Color32 to
        // avoid leaking a fresh Texture2D every frame (NewSolidColorTexture doesn't cache).
        private class Command_AreaTinted : Command_Action
        {
            public Color bgColor = Color.white;

            public override Texture2D BGTexture => SolidTexFor(bgColor);
            public override Texture2D BGTextureShrunk => SolidTexFor(bgColor);

            private static readonly Dictionary<Color32, Texture2D> SolidTexCache = new Dictionary<Color32, Texture2D>();

            private static Texture2D SolidTexFor(Color color)
            {
                Color32 key = color;
                if (SolidTexCache.TryGetValue(key, out var tex) && tex != null) return tex;
                tex = SolidColorMaterials.NewSolidColorTexture(color);
                SolidTexCache[key] = tex;
                return tex;
            }
        }

        private void OpenAreaPicker(Map map)
        {
            var opts = new List<FloatMenuOption>
            {
                new FloatMenuOption("NoAreaAllowed".Translate(), () =>
                {
                    areaLabel = null;
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                }),
            };
            if (map != null)
            {
                foreach (var a in map.areaManager.AllAreas)
                {
                    if (!a.AssignableAsAllowed()) continue;
                    var captured = a;
                    opts.Add(new FloatMenuOption(captured.Label, () =>
                    {
                        areaLabel = captured.Label;
                        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    }, captured.ColorTexture, Color.white));
                }
            }
            Find.WindowStack.Add(new FloatMenu(opts));
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
