namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using static Plugin;

    internal static class BeatmapDataCountHelper
    {
        internal static void GetCount(BeatmapData beatmapData, out int? notesCount, out int? obstaclesCount, out int? bombsCount)
        {
            notesCount = null;
            obstaclesCount = null;
            bombsCount = null;
            if (beatmapData is CustomBeatmapData customBeatmapData)
            {
                IEnumerable<string> requirements = ((List<object>)Trees.at(customBeatmapData.beatmapCustomData, "_requirements"))?.Cast<string>();
                bool noodleRequirement = requirements?.Contains(CAPABILITY) ?? false;

                if (noodleRequirement)
                {
                    // Recount for fake notes
                    notesCount = 0;
                    obstaclesCount = 0;
                    bombsCount = 0;
                    BeatmapLineData[] beatmapLinesData = customBeatmapData.beatmapLinesData;
                    for (int i = 0; i < beatmapLinesData.Length; i++)
                    {
                        foreach (BeatmapObjectData beatmapObjectData in beatmapLinesData[i].beatmapObjectsData)
                        {
                            if (beatmapObjectData is CustomObstacleData || beatmapObjectData is CustomNoteData)
                            {
                                dynamic customObjectData = beatmapObjectData;
                                dynamic dynData = customObjectData.customData;
                                bool? fake = Trees.at(dynData, FAKENOTE);
                                if (fake.HasValue && fake.Value)
                                {
                                    continue;
                                }
                            }

                            if (beatmapObjectData.beatmapObjectType == BeatmapObjectType.Note)
                            {
                                NoteType noteType = ((NoteData)beatmapObjectData).noteType;
                                if (noteType == NoteType.NoteA || noteType == NoteType.NoteB)
                                {
                                    notesCount++;
                                }
                                else if (noteType == NoteType.Bomb)
                                {
                                    bombsCount++;
                                }
                            }
                            else if (beatmapObjectData.beatmapObjectType == BeatmapObjectType.Obstacle)
                            {
                                obstaclesCount++;
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(BeatmapData))]
    [HarmonyPatch("notesCount")]
    [HarmonyPatch(MethodType.Getter)]
    internal static class BeatmapDataGetNotesCount
    {
#pragma warning disable SA1313
        private static void Postfix(BeatmapData __instance, ref int __result)
#pragma warning restore SA1313
        {
            BeatmapDataCountHelper.GetCount(__instance, out int? notesCount, out _, out _);
            if (notesCount.HasValue)
            {
                __result = notesCount.Value;
            }
        }
    }

    [HarmonyPatch(typeof(BeatmapData))]
    [HarmonyPatch("obstaclesCount")]
    [HarmonyPatch(MethodType.Getter)]
    internal static class BeatmapDataGetObstaclesCount
    {
#pragma warning disable SA1313
        private static void Postfix(BeatmapData __instance, ref int __result)
#pragma warning restore SA1313
        {
            BeatmapDataCountHelper.GetCount(__instance, out _, out int? obstaclesCount, out _);
            if (obstaclesCount.HasValue)
            {
                __result = obstaclesCount.Value;
            }
        }
    }

    [HarmonyPatch(typeof(BeatmapData))]
    [HarmonyPatch("bombsCount")]
    [HarmonyPatch(MethodType.Getter)]
    internal static class BeatmapDataGetBombsCount
    {
#pragma warning disable SA1313
        private static void Postfix(BeatmapData __instance, ref int __result)
#pragma warning restore SA1313
        {
            BeatmapDataCountHelper.GetCount(__instance, out _, out _, out int? bombsCount);
            if (bombsCount.HasValue)
            {
                __result = bombsCount.Value;
            }
        }
    }
}
