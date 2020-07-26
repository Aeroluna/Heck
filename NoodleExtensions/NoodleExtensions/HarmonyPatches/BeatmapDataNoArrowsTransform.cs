namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;

    [NoodlePatch(typeof(BeatmapDataNoArrowsTransform))]
    [NoodlePatch("CreateTransformedData")]
    internal static class BeatmapDataNoArrowsTransformCreateTransformedData
    {
        private static readonly MethodInfo _sortBeatmapLinesData = SymbolExtensions.GetMethodInfo(() => SortBeatmapLinesData(null));

        // Because we reorder beatmapLinesData for per object njs/offset, it messes up this script (which is super pepega pls beat games fix), so we resort beatmapLinesData in here
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundLinesData = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundLinesData &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "get_beatmapLinesData")
                {
                    foundLinesData = true;

                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, _sortBeatmapLinesData));
                }
            }

            if (!foundLinesData)
            {
                NoodleLogger.Log("Failed to find callvirt to get_beatmapLinesData!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static BeatmapLineData[] SortBeatmapLinesData(BeatmapLineData[] beatmapLineDatas)
        {
            int length = beatmapLineDatas.Length;
            for (int j = 0; j < length; j++)
            {
                // Sorting algorithm taken from BeatmapDataLoader
                Array.Sort(beatmapLineDatas[j].beatmapObjectsData, (x, y) =>
                {
                    if (x.time == y.time)
                    {
                        return 0;
                    }

                    if (x.time <= y.time)
                    {
                        return -1;
                    }

                    return 1;
                });
            }

            return beatmapLineDatas;
        }
    }
}
