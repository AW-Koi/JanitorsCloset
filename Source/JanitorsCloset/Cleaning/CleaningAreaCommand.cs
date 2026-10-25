using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace JanitorsCloset.Cleaning
{
    /// <summary>The cleaning-area gizmo, tinted with the area's colour; one click sets every selected janitor's tool.</summary>
    public class CleaningAreaCommand : Command
    {
        private const int GroupKey = 0x4A43_4152;
        private static readonly Dictionary<Color32, Texture2D> TintedBgCache = new Dictionary<Color32, Texture2D>(Color32Comparer.Instance);

        private readonly CompCleaningAssignment _tool;
        private readonly Map _map;
        public Color bgColor = Color.white;

        public CleaningAreaCommand(CompCleaningAssignment tool, Map map)
        {
            _tool = tool;
            _map = map;
            groupKeyIgnoreContent = GroupKey;
        }

        public override bool InheritInteractionsFrom(Gizmo other) => false;

        public override void ProcessGroupInput(Event ev, List<Gizmo> group)
        {
            Find.WindowStack.Add(new FloatMenu(AreaOptions(group)));
        }

        /// <summary>Picker entries that set the area on the tool behind every command in the group.</summary>
        public List<FloatMenuOption> AreaOptions(List<Gizmo> group)
        {
            return CompCleaningAssignment.AreaOptions(ToolsIn(group), _map);
        }

        /// <summary>This command's tool, then each other tool in the group, once each.</summary>
        public List<CompCleaningAssignment> ToolsIn(List<Gizmo> group)
        {
            var tools = new List<CompCleaningAssignment> { _tool };
            for (int i = 0; i < group.Count; i++)
            {
                if (group[i] is CleaningAreaCommand other && !tools.Contains(other._tool)) tools.Add(other._tool);
            }
            return tools;
        }

        public override Texture2D BGTexture => TintedBgFor(bgColor);
        public override Texture2D BGTextureShrunk => TintedBgFor(bgColor);

        private sealed class Color32Comparer : IEqualityComparer<Color32>
        {
            public static readonly Color32Comparer Instance = new Color32Comparer();
            public bool Equals(Color32 a, Color32 b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
            public int GetHashCode(Color32 c) => (c.r << 24) | (c.g << 16) | (c.b << 8) | c.a;
        }

        private static Texture2D TintedBgFor(Color color)
        {
            if (color == Color.white) return BGTex;
            Color32 key = color;
            if (TintedBgCache.TryGetValue(key, out var tex) && tex != null) return tex;
            tex = BakeTinted(BGTex, color);
            TintedBgCache[key] = tex;
            return tex;
        }

        // DesButBG is non-readable, so copy it through a RenderTexture, then recolour it as a luminance mask.
        private static Texture2D BakeTinted(Texture2D src, Color tint)
        {
            var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
            var prevActive = RenderTexture.active;
            Graphics.Blit(src, rt);
            RenderTexture.active = rt;
            var copy = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
            RenderTexture.active = prevActive;
            RenderTexture.ReleaseTemporary(rt);

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
}
