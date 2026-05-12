namespace JanitorsCloset.Cleaning
{
    // Per-tool animation parameters consumed by Patch_DrawEquipmentAiming. A tool whose
    // CleaningToolExtension supplies an animProfile gets its weapon sprite oscillated
    // while its bearer is on a cleaning job; without a profile the tool just sits in
    // the default carry pose.
    //
    // The motion is: base angle + sin(t)*wobbleDegrees rotation, plus a cos(t)*slideTiles
    // translation along the aim direction. t advances at basePhaseRate ticks⁻¹ and is
    // itself modulated by a slower speed-wobble of rate speedModRate and depth
    // speedModDepth (radians of phase wobble) so strokes don't feel metronomic.
    public class CleaningAnimProfile
    {
        public float basePhaseRate = 0.025f;
        public float speedModRate = 0.025f;
        public float speedModDepth = 3f;
        public float wobbleDegrees = 20f;
        public float slideTiles = 0.125f;
        public float reachFactor = 0.3f;
        public float rotationOffset = 30f;
        public float verticalOffset = 0.3f;

        // When true, the tool's aim direction is overridden to point from the pawn toward
        // the cell being cleaned — for wand-style tools (Glittervacuum) that should be
        // pointed AT the filth rather than swept perpendicular to it like a broom/mop.
        // equippedAngleOffset on the def still applies on top.
        public bool aimAtTarget = false;
    }
}
