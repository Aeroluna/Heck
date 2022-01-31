using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using NoodleExtensions.Extras;

namespace NoodleExtensions.HarmonyPatches.ObjectProcessing
{
    [HeckPatch]
    [HarmonyPatch(typeof(BeatmapDataLoader))]
    internal static class SaveObjectDataBpm
    {
        private static readonly MethodInfo _noteTimeGetter = AccessTools.PropertyGetter(typeof(BeatmapSaveData.NoteData), nameof(BeatmapSaveData.NoteData.time));
        private static readonly MethodInfo _waypointTimeGetter = AccessTools.PropertyGetter(typeof(BeatmapSaveData.WaypointData), nameof(BeatmapSaveData.WaypointData.time));
        private static readonly MethodInfo _obstacleTimeGetter = AccessTools.PropertyGetter(typeof(BeatmapSaveData.ObstacleData), nameof(BeatmapSaveData.ObstacleData.time));
        private static readonly MethodInfo _saveBpm = AccessTools.Method(typeof(SaveObjectDataBpm), nameof(SaveBpmInData));

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapDataLoader.GetBeatmapDataFromBeatmapSaveData))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)

                // notes
                .MatchForward(false, new CodeMatch(n => n.opcode == OpCodes.Ldloc_S && n.operand is LocalBuilder { LocalIndex: 18 }))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_S, 18),
                    new CodeInstruction(OpCodes.Ldloc_S, 6),
                    new CodeInstruction(OpCodes.Callvirt, _noteTimeGetter),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, _saveBpm))

                // waypoints (lol)
                .MatchForward(false, new CodeMatch(n => n.opcode == OpCodes.Ldloc_S && n.operand is LocalBuilder { LocalIndex: 19 }))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_S, 19),
                    new CodeInstruction(OpCodes.Ldloc_S, 7),
                    new CodeInstruction(OpCodes.Callvirt, _waypointTimeGetter),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, _saveBpm))

                // obstacles
                .MatchForward(false, new CodeMatch(n => n.opcode == OpCodes.Ldloc_S && n.operand is LocalBuilder { LocalIndex: 20 }))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_S, 20),
                    new CodeInstruction(OpCodes.Ldloc_S, 8),
                    new CodeInstruction(OpCodes.Callvirt, _obstacleTimeGetter),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, _saveBpm))

                .InstructionEnumeration();
        }

        private static void SaveBpmInData(BeatmapObjectData beatmapObjectData, float bpmTime, List<BeatmapDataLoader.BpmChangeData> bpmChangesData)
        {
            Dictionary<string, object?> dynData = beatmapObjectData.GetDataForObject();

            // 2 years later and finally support variable bpm....
            int num = 0;
            while (num < bpmChangesData.Count - 1 && bpmChangesData[num + 1].bpmChangeStartBpmTime < bpmTime)
            {
                num++;
            }

            // for per object njs and spawn offset
            dynData["bpm"] = bpmChangesData[num].bpm;
        }
    }
}
