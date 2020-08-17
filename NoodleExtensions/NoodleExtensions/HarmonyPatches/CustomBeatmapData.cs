namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using IPA.Utilities;
    using static Plugin;

    // Yeah i just harmony patched my own mod, you got a problem with it?
    [HarmonyPatch(typeof(CustomBeatmapData))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(BeatmapLineData[]), typeof(BeatmapEventData[]), typeof(CustomEventData[]), typeof(object), typeof(object), typeof(object) })]
    internal static class CustomBeatmapDataCtor
    {
        private static readonly PropertyAccessor<BeatmapData, int>.Setter _notesCountSetter = PropertyAccessor<BeatmapData, int>.GetSetter("notesCount");
        private static readonly PropertyAccessor<BeatmapData, int>.Setter _obstaclesCountSetter = PropertyAccessor<BeatmapData, int>.GetSetter("obstaclesCount");
        private static readonly PropertyAccessor<BeatmapData, int>.Setter _bombsCountSetter = PropertyAccessor<BeatmapData, int>.GetSetter("bombsCount");

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(CustomBeatmapData __instance)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            IEnumerable<string> requirements = ((List<object>)Trees.at(__instance.beatmapCustomData, "_requirements"))?.Cast<string>();
            bool noodleRequirement = requirements?.Contains(CAPABILITY) ?? false;

            if (noodleRequirement)
            {
                // Recount for fake notes
                int notesCount = 0;
                int obstaclesCount = 0;
                int bombsCount = 0;
                BeatmapLineData[] beatmapLinesData = __instance.beatmapLinesData;
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

                BeatmapData beatmapData = __instance as BeatmapData;
                _notesCountSetter(ref beatmapData, notesCount);
                _obstaclesCountSetter(ref beatmapData, obstaclesCount);
                _bombsCountSetter(ref beatmapData, bombsCount);
            }
        }
    }
}
