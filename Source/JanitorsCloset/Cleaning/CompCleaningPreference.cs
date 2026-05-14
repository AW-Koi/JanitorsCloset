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
                bgTint = area != null ? area.Color : Color.white,
                action = () => OpenAreaPicker(map),
            };
        }

        // Tints the gizmo background by the selected area's color while keeping the icon
        // texture untouched. Command's base draws BGTex through the current GUI.color, then
        // resets GUI.color to white before drawing the icon — so setting the color around
        // base.GizmoOnGUI tints only the background. Mouse-over still wins (vanilla forces
        // GenUI.MouseoverColor) which is the behavior we want — hover should look standard.
        private class Command_AreaTinted : Command_Action
        {
            public Color bgTint = Color.white;

            public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
            {
                var prev = GUI.color;
                GUI.color = bgTint;
                var result = base.GizmoOnGUI(topLeft, maxWidth, parms);
                GUI.color = prev;
                return result;
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
