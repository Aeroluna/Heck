using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO),
        new Type[] { typeof(IDifficultyBeatmap), typeof(OverrideEnvironmentSettings), typeof(ColorScheme), typeof(GameplayModifiers),
            typeof(PlayerSpecificSettings), typeof(PracticeSettings), typeof(string), typeof(bool)})]
    [HarmonyPatch("Init")]
    internal class StandardLevelScenesTransitionSetupDataSOInit
    {
        private const float _startHalfJumpDurationInBeats = 4;
        private const float _maxHalfJumpDistance = 18;
        private const float _moveDuration = 0.5f;

        private static void Postfix(IDifficultyBeatmap difficultyBeatmap)
        {
            if (difficultyBeatmap.beatmapData is CustomBeatmapData customBeatmapData)
            {
                IEnumerable<string> requirements = ((List<object>)Trees.at(customBeatmapData.beatmapCustomData, "_requirements"))?.Cast<string>();
                bool noodleRequirement = requirements?.Contains(CAPABILITY) ?? false;
                NoodleController.ToggleNoodlePatches(noodleRequirement);
                if (noodleRequirement)
                {
                    foreach (BeatmapLineData beatmapLineData in difficultyBeatmap.beatmapData.beatmapLinesData)
                    {
                        foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
                        {
                            dynamic customData;
                            if (beatmapObjectData is CustomObstacleData || beatmapObjectData is CustomNoteData) customData = beatmapObjectData;
                            else return;
                            dynamic dynData = customData.customData;
                            float _noteJumpMovementSpeed = (float?)Trees.at(dynData, NOTEJUMPSPEED) ?? difficultyBeatmap.noteJumpMovementSpeed;
                            float _noteJumpStartBeatOffset = (float?)Trees.at(dynData, SPAWNOFFSET) ?? difficultyBeatmap.noteJumpStartBeatOffset;

                            float num = 60f / (float)Trees.at(dynData, "bpm");
                            float num2 = _startHalfJumpDurationInBeats;
                            while (_noteJumpMovementSpeed * num * num2 > _maxHalfJumpDistance)
                            {
                                num2 /= 2f;
                            }
                            num2 += _noteJumpStartBeatOffset;
                            if (num2 < 1f)
                            {
                                num2 = 1f;
                            }
                            float _jumpDuration = num * num2 * 2f;
                            dynData.aheadTime = _moveDuration + _jumpDuration * 0.5f;
                        }
                        beatmapLineData.beatmapObjectsData = beatmapLineData.beatmapObjectsData.OrderBy(n => n.time - (float)((dynamic)n).customData.aheadTime).ToArray();
                    }
                }
            }
        }
    }
}