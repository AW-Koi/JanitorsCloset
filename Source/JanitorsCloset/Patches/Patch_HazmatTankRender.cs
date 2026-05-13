using HarmonyLib;
using JanitorsCloset.Defs;
using RimWorld;
using UnityEngine;
using Verse;

namespace JanitorsCloset.Patches
{
    // Draws the Hazmat Sprayer's tank/backpack as a separate sprite on the pawn's back
    // whenever they're carrying the sprayer. The weapon ThingDef's texture is the wand only
    // — this patch supplies the tank so the in-world pawn reads as "backpack + wand" without
    // the tank ever occupying an apparel slot (no smokepop/shield-belt conflict, no auto-equip
    // dance to coordinate during raids and downed-pawn rescues).
    //
    // Hooks Verse.PawnRenderUtility.DrawEquipmentAndApparelExtras because it's the canonical
    // post-body draw call used by both the cached and uncached pawn render paths, and it
    // receives drawPos already nudged to the right altitude for "draw stuff over the body."
    // We re-bias the altitude per facing so the tank ends up on the *back* layer:
    //   - North-facing (pawn back to camera): tank at high (in-front) altitude so it covers
    //     the visible back.
    //   - Other facings: tank at low (behind-body) altitude so it's hidden behind the torso
    //     and only peeks out via the small x/z back-offset.
    [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAndApparelExtras))]
    [StaticConstructorOnStartup] // We hold a static Graphic field — RimWorld requires this
                                 // attribute on any type that does so, so asset loads are
                                 // guaranteed to happen on the main thread.
    public static class Patch_HazmatTankRender
    {
        private const string TankTexPath = "Things/Item/Equipment/Janitor_HazmatTank";

        // The tank sprite's nominal in-world size (cells). Slightly smaller than a pawn so it
        // reads as a strapped accessory, not a second body. Tune in tandem with the texture.
        private static readonly Vector2 TankDrawSize = new Vector2(0.85f, 0.85f);

        private static Graphic _tankGraphic;
        // Sticky flag once we discover the texture is missing — otherwise every render frame
        // would re-probe ContentFinder and re-log the failure. Cleared by a domain reload
        // anyway, so adding the texture later and restarting the game is enough.
        private static bool _tankGraphicMissing;

        public static void Postfix(Pawn pawn, Vector3 drawPos, Rot4 facing, PawnRenderFlags flags)
        {
            // No Biotech → no sprayer def; without this guard the equipment-def
            // comparison below would treat any equipped weapon as a sprayer.
            if (JanitorDefOf.Janitor_HazmatSprayer == null) return;
            if (pawn?.equipment?.Primary == null) return;
            if (pawn.equipment.Primary.def != JanitorDefOf.Janitor_HazmatSprayer) return;
            if (pawn.Dead || pawn.Downed) return;
            // Hide while the pawn is sleeping/laying — equipment isn't drawn then either,
            // so the floating tank would look wrong.
            if (pawn.GetPosture() != PawnPosture.Standing) return;

            var graphic = ResolveGraphic();
            if (graphic == null) return;

            float bodyFactor = BodyOffsetFactor(pawn);
            var pos = ComputeTankDrawPos(drawPos, facing, bodyFactor);
            var mat = graphic.MatAt(facing);
            var mesh = graphic.MeshAt(facing);

            // South-facing sprite is mostly occluded by the pawn's torso, so we scale it
            // up a touch so it visibly pokes out around the shoulders/head silhouette.
            // Other facings draw at their nominal TankDrawSize — they already read cleanly
            // because the tank either sits in front of the back (N) or peeks out the side
            // of the body (E/W).
            float scale = (facing == Rot4.South) ? 1.08f * bodyFactor : 1f;
            if (Mathf.Abs(scale - 1f) > 0.001f)
            {
                var trs = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(scale, 1f, scale));
                Graphics.DrawMesh(mesh, trs, mat, 0);
            }
            else
            {
                Graphics.DrawMesh(mesh, pos, Quaternion.identity, mat, 0);
            }
        }

        // Scales the back-offset (and the south-facing tank size) by the pawn's body type
        // so a Hulk's tank doesn't disappear inside their silhouette and a Thin pawn's
        // doesn't float detached from theirs. Numbers are eyeballed from in-game sprite
        // widths — revisit if the tank texture changes drastically in proportion.
        private static float BodyOffsetFactor(Pawn pawn)
        {
            var bt = pawn?.story?.bodyType;
            if (bt == null) return 1f;
            if (bt == BodyTypeDefOf.Hulk) return 1.40f;
            if (bt == BodyTypeDefOf.Fat)  return 1.20f;
            if (bt == BodyTypeDefOf.Male) return 1.05f;
            // Thin, Female, and unknown body types stay at baseline.
            return 1.0f;
        }

        // Cached because GraphicDatabase.Get is not free and DrawEquipmentAndApparelExtras
        // runs once per pawn per frame. Probes ContentFinder first so we silently no-op
        // when art hasn't been provided yet — without this guard, Graphic_Multi spawns a
        // magenta "missing texture" sprite on the pawn's back and spams the log.
        private static Graphic ResolveGraphic()
        {
            if (_tankGraphicMissing) return null;
            if (_tankGraphic != null) return _tankGraphic;

            var probe = ContentFinder<UnityEngine.Texture2D>.Get(TankTexPath + "_south", reportFailure: false);
            if (probe == null)
            {
                _tankGraphicMissing = true;
                return null;
            }

            // CutoutComplex so the texture's mask can give the tank metallic shading without
            // tinting it the player's outfit colour the way Cutout would if we ever hand the
            // material an unintended color. Color.white is the sentinel "no tint."
            _tankGraphic = GraphicDatabase.Get<Graphic_Multi>(
                TankTexPath,
                ShaderDatabase.CutoutComplex,
                TankDrawSize,
                Color.white);
            return _tankGraphic;
        }

        // drawPos comes in at the equipment altitude (layer 90 for S/E/W, layer -10 for N).
        // We invert that — the tank wants the opposite layering vs. the weapon, since a
        // backpack reads as "on the back" and the weapon as "in the front hand."
        //
        // The small horizontal offset pushes the tank toward the pawn's back side per facing,
        // so the silhouette doesn't sit flat-centered on the body. Tuned by eye; revisit if
        // we ever swap to a wider tank texture.
        private static Vector3 ComputeTankDrawPos(Vector3 drawPos, Rot4 facing, float bodyFactor)
        {
            // Layer 90 = in front of body (visible). Layer -10 = behind body (hidden).
            // North-facing pawns show their back to the camera, so "on the back" means
            // ON TOP of the visible silhouette → layer 90. Every other facing wants the
            // tank tucked behind the torso → layer -10.
            float currentLayer = (facing == Rot4.North) ? -10f : 90f;
            float targetLayer  = (facing == Rot4.North) ?  90f : -10f;
            float yOffset = PawnRenderUtility.AltitudeForLayer(targetLayer)
                          - PawnRenderUtility.AltitudeForLayer(currentLayer);

            Vector3 pos = drawPos;
            pos.y += yOffset;

            // Position offsets. For N/S we push the tank UP the screen (+z) to sit on
            // the pawn's upper back / shoulder line — drawing it below the body center
            // would put it at the feet, which reads as "luggage dropped behind them."
            // For E/W we push it back-of-facing on the x-axis so it peeks out the side
            // of the silhouette. Body factor scales everything per body width.
            if (facing == Rot4.North)      pos.z += 0.02f * bodyFactor; // upper-mid back
            else if (facing == Rot4.South) pos.z += 0.08f * bodyFactor; // upper torso (shoulders)
            else if (facing == Rot4.East)  pos.x -= 0.30f * bodyFactor; // back is to the west
            else if (facing == Rot4.West)  pos.x += 0.30f * bodyFactor; // back is to the east

            return pos;
        }
    }
}
