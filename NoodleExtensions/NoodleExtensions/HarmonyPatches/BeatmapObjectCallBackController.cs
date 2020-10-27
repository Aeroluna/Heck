namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;

    [NoodlePatch(typeof(BeatmapObjectCallbackController))]
    [NoodlePatch("LateUpdate")]
    internal static class BeatmapObjectCallBackControllerLateUpdate
    {
        private static readonly MethodInfo _getAheadTime = SymbolExtensions.GetMethodInfo(() => GetAheadTime(null, null, 0));
        private static readonly MethodInfo _getReorderedLineData = SymbolExtensions.GetMethodInfo(() => GetReorderedLineData(null));
        private static readonly MethodInfo _beatmapObjectSpawnControllerCallback = typeof(BeatmapObjectSpawnController).GetMethod("HandleBeatmapObjectCallback", BindingFlags.Public | BindingFlags.Instance);

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundAheadTime = false;
            bool foundLineData = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundAheadTime &&
                    instructionList[i].opcode == OpCodes.Ldfld &&
                    ((FieldInfo)instructionList[i].operand).Name == "aheadTime")
                {
                    foundAheadTime = true;

                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, _getAheadTime));
                    instructionList.Insert(i - 1, new CodeInstruction(OpCodes.Ldloc_3));
                    instructionList.Insert(i - 1, new CodeInstruction(OpCodes.Ldloc_1));
                }

                if (instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "get_beatmapLinesData")
                {
                    foundLineData = true;

                    instructionList[i].opcode = OpCodes.Call;
                    instructionList[i].operand = _getReorderedLineData;
                }
            }

            if (!foundAheadTime)
            {
                NoodleLogger.Log("Failed to find aheadTime ldfld!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundLineData)
            {
                NoodleLogger.Log("Failed to find get_beatmapLinesData callvirt!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static float GetAheadTime(BeatmapObjectCallbackController.BeatmapObjectCallbackData beatmapObjectCallbackData, BeatmapObjectData beatmapObjectData, float @default)
        {
            if (beatmapObjectCallbackData.callback.Method == _beatmapObjectSpawnControllerCallback &&
                (beatmapObjectData is CustomObstacleData || beatmapObjectData is CustomNoteData))
            {
                dynamic dynData = ((dynamic)beatmapObjectData).customData;
                float? aheadTime = (float?)Trees.at(dynData, "aheadTime");
                if (aheadTime.HasValue)
                {
                    return aheadTime.Value;
                }
            }

            return @default;
        }

        private static IReadOnlyList<IReadonlyBeatmapLineData> GetReorderedLineData(BeatmapData beatmapData)
        {
            if (beatmapData is CustomBeatmapData customBeatmapData)
            {
                List<IReadonlyBeatmapLineData> redorderedLineData = (List<IReadonlyBeatmapLineData>)Trees.at(customBeatmapData.customData, "reorderedLineData");
                if (redorderedLineData != null)
                {
                    return redorderedLineData;
                }
            }

            return null;
        }
    }
}
