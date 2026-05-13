using HarmonyLib;
using JanitorsCloset.Defs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Patches
{
    // Airborne foam-puff accent thrown from the wand toward the target tile while a
    // Hazmat Sprayer-equipped pawn is working. This is now visual flavour only — the
    // durable "janitor is here" signal is the Janitor_HazmatFoam filth deposited by
    // Patch_SpawnHazmatFoam, which renders at every zoom level. The puff just provides
    // the close-up "spray in flight" frame between wand and tile.
    //
    // Hook is identical in shape to Patch_GlittervacuumCleaningGlow — postfix on
    // JobDriver.DriverTickInterval, gated to the cleaning drivers and the sprayer.
    // Per-pawn phase prevents multiple sprayers pulsing in lockstep.
    [HarmonyPatch]
    public static class Patch_HazmatSprayerVFX
    {
        // A puff every 10 ticks reads as continuous flow; tighter would just stack puffs
        // on themselves before they finish fading.
        private const int SprayIntervalTicks = 10;

        [HarmonyPatch(typeof(JobDriver), "DriverTickInterval")]
        [HarmonyPostfix]
        public static void Postfix(JobDriver __instance)
        {
            // No Biotech → no sprayer def loaded; the equipped-weapon comparison below
            // would otherwise pass for any weapon (anyDef != null) and we'd try to spawn
            // a null FleckDef.
            if (JanitorDefOf.Janitor_HazmatSprayer == null) return;

            // Fire on both pollution-clearing and filth-cleaning. Even on a category
            // mismatch (the bonus is suppressed by StatPart_CleaningToolMatch), the
            // player equipped a sprayer — the wand should visibly do something. Speed
            // is gated by the stat layer; the show is independent.
            if (!(__instance is JobDriver_ClearPollution) && !(__instance is JobDriver_CleanFilth)) return;

            var pawn = __instance.pawn;
            if (pawn?.equipment?.Primary?.def != JanitorDefOf.Janitor_HazmatSprayer) return;
            if (pawn.Map == null) return;
            if (pawn.pather != null && pawn.pather.Moving) return;

            var job = __instance.job;
            if (job == null || !job.targetA.IsValid) return;

            int ticks = Find.TickManager.TicksGame;
            int phase = pawn.thingIDNumber & 0xFF;
            if ((ticks + phase) % SprayIntervalTicks != 0) return;

            SpawnSprayPuff(pawn.Map, pawn.DrawPos, job.targetA.CenterVector3);
        }

        // Spawn the puff close to the target cell — the foam filth is what the player
        // reads at any zoom anyway, so anchoring the puff near the impact point keeps
        // the visual cluster coherent. A bit of jitter and a short backward velocity
        // along pawn→target keeps it from looking static.
        private static void SpawnSprayPuff(Map map, Vector3 pawnPos, Vector3 targetPos)
        {
            Vector3 dir = targetPos - pawnPos;
            if (dir.sqrMagnitude < 0.0001f) return; // Pawn standing on the target cell.
            float dist = dir.magnitude;
            dir /= dist;

            // Spawn ~0.35 cells short of the target on the pawn→target line. That's
            // visually "just above the foam-filth on the tile" — close enough to read
            // as impact splash, far enough back from dead-centre that the velocity has
            // somewhere to travel.
            Vector3 spawn = targetPos - dir * 0.35f;
            spawn.x += Rand.Range(-0.10f, 0.10f);
            spawn.z += Rand.Range(-0.10f, 0.10f);

            float baseAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float angle = baseAngle + Rand.Range(-12f, 12f);

            FleckCreationData data = FleckMaker.GetDataStatic(spawn, map,
                JanitorDefOf.Janitor_HazmatChemSpray, Rand.Range(0.9f, 1.2f));
            data.rotation = angle;
            data.velocityAngle = angle;
            // Low velocity — the puff is meant to read as bouncing/dispersing on the
            // tile, not racing across the screen. The foam filth carries persistence.
            data.velocitySpeed = Rand.Range(0.3f, 0.6f);
            map.flecks.CreateFleck(data);
        }
    }
}
