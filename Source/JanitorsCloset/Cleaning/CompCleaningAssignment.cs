using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace JanitorsCloset.Cleaning
{
    /// <summary>A tool's indoor/outdoor and area restriction; filth outside it is never offered to the bearer.</summary>
    /// <remarks>The area is stored by label, so the restriction follows the tool onto other maps.</remarks>
    public class CompCleaningAssignment : ThingComp
    {
        private static readonly string[] IconPaths =
        {
            "UI/Commands/Janitor_CleaningPref_Any",
            "UI/Commands/Janitor_CleaningPref_Indoors",
            "UI/Commands/Janitor_CleaningPref_Outdoors",
        };

        private static Texture2D[] _icons;
        private static Texture2D _areaIcon;
        private static string[] _labels;
        private static string[] _descriptions;
        private static string[] _inspectLabels;

        public CleaningAreaAssignment assignment = CleaningAreaAssignment.Any;
        private string _areaLabel;

        private Map _resolvedMap;
        private int _resolvedTick = -1;
        private Area _resolvedArea;

        private string _gizmoTextFor;
        private string _areaGizmoLabel;
        private string _areaGizmoDescription;

        /// <summary>The restricting area on this map, or null for none; resolved at most once per tick.</summary>
        public Area ResolveArea(Map map)
        {
            if (map == null || string.IsNullOrEmpty(_areaLabel)) return null;
            int now = Find.TickManager.TicksGame;
            if (map != _resolvedMap || now != _resolvedTick)
            {
                _resolvedArea = map.areaManager.GetLabeled(_areaLabel);
                _resolvedMap = map;
                _resolvedTick = now;
            }
            return _resolvedArea;
        }

        public void AssignArea(Area area)
        {
            _areaLabel = area?.Label;
            _resolvedMap = null;
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
            return area == null || area[cell];
        }

        public override string CompInspectStringExtra()
        {
            var area = ResolveArea(parent.MapHeld);
            if (assignment == CleaningAreaAssignment.Any && area == null) return null;
            EnsureText();
            var areaText = area != null ? area.Label : "JanitorsCloset.CleaningAssignment.Inspect.AnyArea".Translate().ToString();
            return "JanitorsCloset.CleaningAssignment.Inspect".Translate(_inspectLabels[(int)assignment], areaText);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref assignment, "cleaningAreaAssignment");
            Scribe_Values.Look(ref _areaLabel, "cleaningAreaLabel");
        }

        // Vanilla only asks CompEquippable for equipped gizmos, so Patch_CleaningAreaAssignment surfaces these.
        public IEnumerable<Gizmo> BuildGizmos()
        {
            EnsureText();
            yield return new Command_Action
            {
                defaultLabel = _labels[(int)assignment],
                defaultDesc = _descriptions[(int)assignment],
                icon = _icons[(int)assignment],
                action = Cycle,
            };

            var map = parent.MapHeld;
            var area = ResolveArea(map);
            RefreshAreaGizmoText(area);
            yield return new CleaningAreaCommand(this, map)
            {
                defaultLabel = _areaGizmoLabel,
                defaultDesc = _areaGizmoDescription,
                icon = _areaIcon,
                bgColor = area?.Color ?? Color.white,
            };
        }

        /// <summary>Picker entries that set the area on every given tool.</summary>
        public static List<FloatMenuOption> AreaOptions(IReadOnlyList<CompCleaningAssignment> tools, Map map)
        {
            var options = new List<FloatMenuOption>
            {
                new FloatMenuOption("NoAreaAllowed".Translate(), () => AssignAll(tools, null)),
            };
            if (map == null) return options;
            foreach (var area in map.areaManager.AllAreas)
            {
                if (!area.AssignableAsAllowed()) continue;
                options.Add(new FloatMenuOption(area.Label, () => AssignAll(tools, area), area.ColorTexture, Color.white)
                {
                    mouseoverGuiAction = _ => area.MarkForDraw(),
                });
            }
            return options;
        }

        private static void AssignAll(IReadOnlyList<CompCleaningAssignment> tools, Area area)
        {
            for (int i = 0; i < tools.Count; i++) tools[i].AssignArea(area);
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        }

        private void RefreshAreaGizmoText(Area area)
        {
            var label = area?.Label;
            if (_areaGizmoLabel != null && label == _gizmoTextFor) return;
            _gizmoTextFor = label;
            _areaGizmoLabel = label != null
                ? "JanitorsCloset.CleaningArea.Label.Restricted".Translate(label)
                : "JanitorsCloset.CleaningArea.Label.None".Translate();
            _areaGizmoDescription = label != null
                ? "JanitorsCloset.CleaningArea.Desc.Restricted".Translate(label)
                : "JanitorsCloset.CleaningArea.Desc.None".Translate();
        }

        private void Cycle()
        {
            assignment = (CleaningAreaAssignment)(((byte)assignment + 1) % 3);
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        }

        private static void EnsureText()
        {
            if (_labels != null) return;
            int count = IconPaths.Length;
            _labels = new string[count];
            _descriptions = new string[count];
            _inspectLabels = new string[count];
            _icons = new Texture2D[count];
            for (int i = 0; i < count; i++)
            {
                var value = (CleaningAreaAssignment)i;
                _labels[i] = ("JanitorsCloset.CleaningAssignment.Label." + value).Translate();
                _descriptions[i] = ("JanitorsCloset.CleaningAssignment.Desc." + value).Translate();
                _inspectLabels[i] = ("JanitorsCloset.CleaningAssignment.Inspect." + value).Translate();
                _icons[i] = ContentFinder<Texture2D>.Get(IconPaths[i], false) ?? BaseContent.BadTex;
            }
            _areaIcon = ContentFinder<Texture2D>.Get("UI/Commands/Janitor_CleaningPref_Area", false) ?? BaseContent.BadTex;
        }
    }
}
