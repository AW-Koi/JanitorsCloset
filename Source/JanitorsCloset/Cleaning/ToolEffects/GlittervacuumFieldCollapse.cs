using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace JanitorsCloset.Cleaning.ToolEffects
{
    // An area-clearing tool unmakes every filth on the cleaned tile and its eight neighbours, with cyan puffs drifting off.
    public static class GlittervacuumFieldCollapse
    {
        private static readonly Color GlitterTint = new Color(0.7f, 0.95f, 1.0f, 0.55f);
        private static readonly List<Filth> Targets = new List<Filth>();

        public static void Clear(IntVec3 focal, Map map)
        {
            SpawnBurst(focal, map);
            ClearCell(focal, map);
            for (int i = 0; i < GenAdj.AdjacentCells.Length; i++)
            {
                var neighbour = focal + GenAdj.AdjacentCells[i];
                if (neighbour.InBounds(map) && ClearCell(neighbour, map)) SpawnNeighbourPuff(neighbour, map);
            }
        }

        // Marker filth is another tool's trace and fades on its own.
        private static bool ClearCell(IntVec3 cell, Map map)
        {
            var things = map.thingGrid.ThingsListAtFast(cell);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is Filth filth && !filth.Destroyed && !MarkerFilth.IsMarker(filth)) Targets.Add(filth);
            }
            if (Targets.Count == 0) return false;
            for (int i = 0; i < Targets.Count; i++) Targets[i].Destroy();
            Targets.Clear();
            return true;
        }

        private static void SpawnBurst(IntVec3 cell, Map map)
        {
            var center = cell.ToVector3Shifted();
            int puffCount = Rand.RangeInclusive(2, 3);
            for (int i = 0; i < puffCount; i++)
            {
                var offset = new Vector3(Rand.Range(-0.15f, 0.15f), 0f, Rand.Range(-0.15f, 0.15f));
                FleckMaker.ThrowDustPuffThick(center + offset, map, Rand.Range(0.45f, 0.7f), GlitterTint);
            }
        }

        private static void SpawnNeighbourPuff(IntVec3 cell, Map map)
        {
            var offset = new Vector3(Rand.Range(-0.15f, 0.15f), 0f, Rand.Range(-0.15f, 0.15f));
            FleckMaker.ThrowDustPuffThick(cell.ToVector3Shifted() + offset, map, Rand.Range(0.35f, 0.55f), GlitterTint);
        }
    }
}
