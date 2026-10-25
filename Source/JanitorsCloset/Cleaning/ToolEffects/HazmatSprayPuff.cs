using JanitorsCloset.Defs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Cleaning.ToolEffects
{
    // Foam puffs thrown from the wand toward the worked tile.
    public static class HazmatSprayPuff
    {
        private const int SprayIntervalTicks = 10;

        public static void Tick(CleaningToilSession session, Job job, int delta)
        {
            if (!session.SprayInterval.Elapse(delta, SprayIntervalTicks)) return;
            var pawn = session.Pawn;
            SpawnSprayPuff(pawn.Map, pawn.DrawPos, job.targetA.CenterVector3);
        }

        // Land the puff just short of the target so it reads as splash on the foam.
        private static void SpawnSprayPuff(Map map, Vector3 pawnPos, Vector3 targetPos)
        {
            Vector3 dir = targetPos - pawnPos;
            if (dir.sqrMagnitude < 0.0001f) return;
            float dist = dir.magnitude;
            dir /= dist;

            Vector3 spawn = targetPos - dir * 0.35f;
            spawn.x += Rand.Range(-0.10f, 0.10f);
            spawn.z += Rand.Range(-0.10f, 0.10f);

            float baseAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float angle = baseAngle + Rand.Range(-12f, 12f);

            FleckCreationData data = FleckMaker.GetDataStatic(spawn, map,
                JanitorDefOf.Janitor_HazmatChemSpray, Rand.Range(0.9f, 1.2f));
            data.rotation = angle;
            data.velocityAngle = angle;
            data.velocitySpeed = Rand.Range(0.3f, 0.6f);
            map.flecks.CreateFleck(data);
        }
    }
}
