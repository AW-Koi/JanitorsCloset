using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace JanitorsCloset.Cleaning
{
    public enum CleaningAreaAssignment : byte
    {
        Any = 0,
        Indoors = 1,
        Outdoors = 2,
    }

    public class CompProperties_CleaningAssignment : CompProperties
    {
        public CompProperties_CleaningAssignment()
        {
            compClass = typeof(CompCleaningAssignment);
        }
    }

    // Per-tool indoor/outdoor and area assignment. Lives on the weapon, so swapping tools
    // moves the assignment with the gear. Existence of the comp doubles as the "is a
    // janitor tool" check used by Patch_CleaningAreaAssignment — non-janitor weapons have
    // no comp and are never filtered. The assignment is a hard restriction, not a soft
    // preference: filth outside the assignment is filtered out at HasJobOnThing time.
    public class CompCleaningAssignment : ThingComp
    {
        public CleaningAreaAssignment assignment = CleaningAreaAssignment.Any;

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
            if (assignment != CleaningAreaAssignment.Any)
            {
                var room = cell.GetRoom(map);
                bool outdoors = room == null || room.PsychologicallyOutdoors;
                bool assignmentOk = assignment == CleaningAreaAssignment.Outdoors ? outdoors : !outdoors;
                if (!assignmentOk) return false;
            }
            var area = ResolveArea(map);
            if (area != null && !area[cell]) return false;
            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref assignment, "cleaningAreaAssignment", CleaningAreaAssignment.Any);
            Scribe_Values.Look(ref areaLabel, "cleaningAreaLabel");
        }

        // Surfaced via Patch_CleaningAreaAssignment's Pawn_EquipmentTracker.GetGizmos hook —
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
                defaultLabel = ("JanitorsCloset.CleaningAssignment.Label." + assignment).Translate(),
                defaultDesc = ("JanitorsCloset.CleaningAssignment.Desc." + assignment).Translate(),
                icon = IconFor(assignment),
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

        // Mirrors what vanilla does for ability gizmos (Command_Ability swaps BGTexture to
        // AbilityButBG, a pre-baked green-tinted DesButBG). We do the same thing at runtime,
        // multiplying DesButBG by the area's color so border and vignette survive. One-time
        // bake per Color32, cached for the session.
        private class Command_AreaTinted : Command_Action
        {
            public Color bgColor = Color.white;

            public override Texture2D BGTexture => TintedBgFor(bgColor);
            public override Texture2D BGTextureShrunk => TintedBgFor(bgColor);

            private static readonly Dictionary<Color32, Texture2D> TintedBgCache = new Dictionary<Color32, Texture2D>();

            private static Texture2D TintedBgFor(Color color)
            {
                if (color == Color.white) return Command.BGTex;
                Color32 key = color;
                if (TintedBgCache.TryGetValue(key, out var tex) && tex != null) return tex;
                tex = BakeTinted(Command.BGTex, color);
                TintedBgCache[key] = tex;
                return tex;
            }

            private static Texture2D BakeTinted(Texture2D src, Color tint)
            {
                // DesButBG is loaded non-readable; round-trip through a RenderTexture to
                // pull its pixels back into a CPU-side Texture2D we can recolor and Apply.
                var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
                var prevActive = RenderTexture.active;
                Graphics.Blit(src, rt);
                RenderTexture.active = rt;
                var copy = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
                copy.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
                RenderTexture.active = prevActive;
                RenderTexture.ReleaseTemporary(rt);

                // Treat DesButBG as a luminance mask and re-color with the tint, the way
                // AbilityButBG looks vs DesButBG. Plain multiply went black-dark because the
                // BG is a mid-grey; normalising against the texture's peak brightness lets
                // the brightest pixel land at the full area color and the vignette/border
                // fall off from there.
                var px = copy.GetPixels();
                float maxV = 0f;
                for (int i = 0; i < px.Length; i++)
                {
                    var p = px[i];
                    float v = Mathf.Max(p.r, p.g, p.b);
                    if (v > maxV) maxV = v;
                }
                if (maxV < 0.001f) maxV = 1f;
                float inv = 1f / maxV;
                for (int i = 0; i < px.Length; i++)
                {
                    var p = px[i];
                    float v = Mathf.Max(p.r, p.g, p.b) * inv;
                    px[i] = new Color(tint.r * v, tint.g * v, tint.b * v, p.a);
                }
                copy.SetPixels(px);
                copy.Apply(false, true);
                return copy;
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
                    }, captured.ColorTexture, Color.white)
                    {
                        // Vanilla allowed-area picker calls MarkForDraw each frame the option
                        // is hovered so the area overlay paints on the map.
                        mouseoverGuiAction = _ => captured.MarkForDraw(),
                    });
                }
            }
            Find.WindowStack.Add(new FloatMenu(opts));
        }

        private void Cycle()
        {
            assignment = (CleaningAreaAssignment)(((byte)assignment + 1) % 3);
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        }

        private static Texture2D IconFor(CleaningAreaAssignment a)
        {
            string path;
            switch (a)
            {
                case CleaningAreaAssignment.Indoors: path = "UI/Commands/Janitor_CleaningPref_Indoors"; break;
                case CleaningAreaAssignment.Outdoors: path = "UI/Commands/Janitor_CleaningPref_Outdoors"; break;
                default: path = "UI/Commands/Janitor_CleaningPref_Any"; break;
            }
            return ContentFinder<Texture2D>.Get(path, false) ?? BaseContent.BadTex;
        }
    }
}
