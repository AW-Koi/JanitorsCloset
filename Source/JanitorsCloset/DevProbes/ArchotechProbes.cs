using System.Collections.Generic;
using System.Text.Json.Nodes;
using JanitorsCloset.Cleaning;
using RimWorld;
using Verse;

namespace JanitorsCloset.DevProbes
{
    /// <summary>Read-only probes Archotech Harness discovers by class name; unused in normal play.</summary>
    public static class ArchotechProbes
    {
        /// <summary>The room at "cell": ID, outdoors flag, size, fresh-room stamp and every filth inside.</summary>
        public static Dictionary<string, object> RoomState(string json)
        {
            var map = Find.CurrentMap;
            var cellNode = JsonNode.Parse(json)?["cell"]?.AsArray();
            if (map == null || cellNode == null) return new Dictionary<string, object> { ["error"] = "Needs a current map and a cell." };

            var cell = new IntVec3((int)cellNode[0], 0, (int)cellNode[1]);
            var room = cell.GetRoom(map);
            if (room == null) return new Dictionary<string, object> { ["room"] = null };

            var filth = new List<Dictionary<string, object>>();
            foreach (var roomCell in room.Cells)
            {
                foreach (var thing in roomCell.GetThingList(map))
                {
                    if (!(thing is Filth f)) continue;
                    filth.Add(new Dictionary<string, object>
                    {
                        ["def"] = f.def.defName,
                        ["marker"] = MarkerFilth.IsMarker(f.def),
                        ["cell"] = new[] { roomCell.x, roomCell.z },
                    });
                }
            }

            return new Dictionary<string, object>
            {
                ["roomId"] = room.ID,
                ["psychologicallyOutdoors"] = room.PsychologicallyOutdoors,
                ["cellCount"] = room.CellCount,
                ["stamped"] = map.GetComponent<MapComponent_FreshlyCleanedRooms>()?.IsRoomStamped(room) ?? false,
                ["filth"] = filth,
            };
        }
    }
}
