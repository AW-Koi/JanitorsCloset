using System;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Defs;
using UnityEngine;
using Verse;

namespace JanitorsCloset.Patches
{
    // When a pawn is holding the HAZMAT Sprayer, draw the wand-only sprite at a slightly
    // smaller scale instead of the def's combo (backpack + wand) texture. The combo art is
    // intentionally the def's texPath so it shows on the ground and in the drafted-weapon
    // gizmo — but rendering the combo in-hand would visually double up the backpack against
    // the separately-drawn tank on the pawn's back. This patch suppresses vanilla's
    // DrawEquipmentAiming for the sprayer and does the equivalent draw with a substituted
    // graphic.
    //
    // Runs as a Prefix at Priority.Low so the existing Patch_DrawEquipmentAiming (which
    // applies the CleaningAnimProfile wobble/aim to drawLoc and aimAngle) has already
    // executed by the time we read the values. Returning false skips vanilla's own draw.
    [HarmonyPatch(typeof(Verse.PawnRenderUtility), "DrawEquipmentAiming")]
    [StaticConstructorOnStartup] // Holds a static Graphic field — RimWorld's main-thread
                                 // asset-load check requires this on any such type.
    public static class Patch_HazmatSprayerInHandDraw
    {
        private const string WandTexPath = "Things/Item/Equipment/Janitor_HazmatSprayer";

        // Wand drawSize, in cells. Matches the original wand-only sprite's intended in-hand
        // proportions. The 0.8 scale knock-down happens at draw time below.
        private static readonly Vector2 WandDrawSize = new Vector2(1.25f, 0.55f);

        // How much smaller the in-hand wand sits vs. its nominal drawSize. User-tuned —
        // a sprayer is a one-handed item, not the broom-length implements other tools are.
        private const float InHandScale = 0.8f;

        private static Graphic _wandGraphic;
        private static bool _wandGraphicMissing;

        [HarmonyPriority(Priority.Low)]
        public static bool Prefix(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            // No Biotech → no sprayer def. The eq?.def check below would otherwise pass
            // for non-sprayer items (anyDef != null), suppressing vanilla's own draw.
            if (JanitorDefOf.Janitor_HazmatSprayer == null) return true;
            if (eq?.def != JanitorDefOf.Janitor_HazmatSprayer) return true;

            var graphic = ResolveWandGraphic();
            if (graphic == null) return true; // texture missing → fall back to vanilla draw

            // Vanilla DrawEquipmentAiming chooses between plane10 (default) and plane10Flip
            // (flipped horizontally for left-facing angles) and bakes equippedAngleOffset
            // into the rotation. We reproduce that exactly so the wand orientation matches
            // what the player would see if we hadn't intercepted.
            float angle = aimAngle - 90f;
            Mesh mesh;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh = MeshPool.plane10;
                angle += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh = MeshPool.plane10Flip;
                angle -= 180f;
                angle -= eq.def.equippedAngleOffset;
            }
            else
            {
                mesh = MeshPool.plane10;
                angle += eq.def.equippedAngleOffset;
            }
            angle %= 360f;

            // Recoil offset (vanilla applies this if the weapon has CompEquippable). We
            // skip it — cleaning isn't a recoil-firing verb. If the sprayer ever gets a
            // ranged verb the patch will need to mirror the EquipmentUtility.Recoil call.

            var material = graphic.MatSingleFor(eq);
            var scale = new Vector3(WandDrawSize.x * InHandScale, 0f, WandDrawSize.y * InHandScale);
            var matrix = Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(angle, Vector3.up), scale);
            Graphics.DrawMesh(mesh, matrix, material, 0);
            return false;
        }

        private static Graphic ResolveWandGraphic()
        {
            if (_wandGraphicMissing) return null;
            if (_wandGraphic != null) return _wandGraphic;

            var probe = ContentFinder<Texture2D>.Get(WandTexPath, reportFailure: false);
            if (probe == null)
            {
                _wandGraphicMissing = true;
                return null;
            }

            _wandGraphic = GraphicDatabase.Get<Graphic_Single>(
                WandTexPath,
                ShaderDatabase.CutoutComplex,
                WandDrawSize,
                Color.white);
            return _wandGraphic;
        }
    }
}
