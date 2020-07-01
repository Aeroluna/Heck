namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using IPA.Utilities;
    using static NoodleExtensions.Plugin;

    [HarmonyPatch(typeof(CustomLevelLoader))]
    [HarmonyPatch("LoadBeatmapDataBeatmapData")]
    internal class CustomLevelLoaderLoadBeatmapDataBeatmapData
    {
        [HarmonyPriority(Priority.Low)]
#pragma warning disable SA1313
        private static void Postfix(BeatmapData __result, string difficultyFileName, StandardLevelInfoSaveData standardLevelInfoSaveData)
#pragma warning restore SA1313
        {
            if (__result != null && __result is CustomBeatmapData customBeatmapData)
            {
                // heck you beat games for not passing StandardLevelInfoSaveData.DifficultyBeatmap down to this method
                StandardLevelInfoSaveData.DifficultyBeatmap difficultyBeatmap = null;
                int iCount = standardLevelInfoSaveData.difficultyBeatmapSets.Length;
                for (int i = 0; i < iCount; i++)
                {
                    StandardLevelInfoSaveData.DifficultyBeatmapSet difficultyBeatmapSet = standardLevelInfoSaveData.difficultyBeatmapSets[i];
                    int jCount = difficultyBeatmapSet.difficultyBeatmaps.Length;
                    for (int j = 0; j < jCount; j++)
                    {
                        StandardLevelInfoSaveData.DifficultyBeatmap difficultyBeatmaps = difficultyBeatmapSet.difficultyBeatmaps[j];
                        if (difficultyBeatmaps.beatmapFilename == difficultyFileName)
                        {
                            difficultyBeatmap = difficultyBeatmaps;
                        }
                    }
                }

                if (difficultyBeatmap != null)
                {
                    // there is some ambiguity with these variables but who frikkin cares
                    float startHalfJumpDurationInBeats = 4;
                    float maxHalfJumpDistance = 18;
                    float moveDuration = 0.5f;

                    foreach (BeatmapLineData beatmapLineData in __result.beatmapLinesData)
                    {
                        foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
                        {
                            dynamic customData;
                            if (beatmapObjectData is CustomObstacleData || beatmapObjectData is CustomNoteData)
                            {
                                customData = beatmapObjectData;
                            }
                            else
                            {
                                return;
                            }

                            dynamic dynData = customData.customData;
                            float noteJumpMovementSpeed = (float?)Trees.at(dynData, NOTEJUMPSPEED) ?? difficultyBeatmap.noteJumpMovementSpeed;
                            float noteJumpStartBeatOffset = (float?)Trees.at(dynData, NOTESPAWNOFFSET) ?? difficultyBeatmap.noteJumpStartBeatOffset;

                            // how do i not repeat this in a reasonable way
                            float num = 60f / (float)Trees.at(dynData, "bpm");
                            float num2 = startHalfJumpDurationInBeats;
                            while (noteJumpMovementSpeed * num * num2 > maxHalfJumpDistance)
                            {
                                num2 /= 2f;
                            }

                            num2 += noteJumpStartBeatOffset;
                            if (num2 < 1f)
                            {
                                num2 = 1f;
                            }

                            float jumpDuration = num * num2 * 2f;
                            dynData.aheadTime = moveDuration + (jumpDuration * 0.5f);
                        }

                        beatmapLineData.beatmapObjectsData = beatmapLineData.beatmapObjectsData.OrderBy(n => n.time - (float)((dynamic)n).customData.aheadTime).ToArray();
                    }
                }
            }
        }
    }
}
