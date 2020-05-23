using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapObjectSpawnMovementData))]
    [HarmonyPatch("Init")]
    internal class BeatmapObjectSpawnMovementDataInit
    {
        private static void Postfix(BeatmapObjectSpawnMovementData __instance)
        {
            InitBeatmapObjectSpawnController(__instance);
        }
    }

    [NoodlePatch(typeof(BeatmapObjectSpawnMovementData))]
    [NoodlePatch("GetObstacleSpawnMovementData")]
    internal class BeatmapObjectSpawnMovementDataGetObstacleSpawnMovementData
    {
        private static void Postfix(ObstacleData obstacleData, ref Vector3 moveStartPos, ref Vector3 moveEndPos, ref Vector3 jumpEndPos, ref float obstacleHeight)
        {
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float?> position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                IEnumerable<float?> scale = ((List<object>)Trees.at(dynData, SCALE))?.Select(n => n.ToNullableFloat());
                float? njs = (float?)Trees.at(dynData, NOTEJUMPSPEED);
                float? spawnoffset = (float?)Trees.at(dynData, SPAWNOFFSET);

                float? startX = position?.ElementAtOrDefault(0);
                float? startY = position?.ElementAtOrDefault(1);

                float? height = scale?.ElementAtOrDefault(1);

                // Actual wall stuff
                if (startX.HasValue || startY.HasValue || njs.HasValue || spawnoffset.HasValue)
                {
                    GetNoteJumpValues(njs, spawnoffset, out float _, out float _, out Vector3 _localMoveStartPos, out Vector3 _localMoveEndPos, out Vector3 _localJumpEndPos);

                    // Ripped from base game
                    Vector3 noteOffset = GetNoteOffset(obstacleData, startX, null);
                    noteOffset.y = startY.HasValue ? _verticalObstaclePosY + startY.GetValueOrDefault(0) * _noteLinesDistance : ((obstacleData.obstacleType == ObstacleType.Top)
                        ? (_topObstaclePosY + _jumpOffsetY) : _verticalObstaclePosY);
                    moveStartPos = _localMoveStartPos + noteOffset;
                    moveEndPos = _localMoveEndPos + noteOffset;
                    jumpEndPos = _localJumpEndPos + noteOffset;
                }
                if (height.HasValue) obstacleHeight = height.Value * _noteLinesDistance;
            }
        }
    }

    [NoodlePatch(typeof(BeatmapObjectSpawnMovementData))]
    [NoodlePatch("GetNoteSpawnMovementData")]
    internal class BeatmapObjectSpawnMovementDataGetNoteSpawnMovementData
    {
        private static void Postfix(NoteData noteData, ref Vector3 moveStartPos, ref Vector3 moveEndPos, ref Vector3 jumpEndPos, ref float jumpGravity)
        {
            if (noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float?> position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                float? flipLineIndex = (float?)Trees.at(dynData, "flipLineIndex");
                float? njs = (float?)Trees.at(dynData, NOTEJUMPSPEED);
                float? spawnoffset = (float?)Trees.at(dynData, SPAWNOFFSET);

                float? startRow = position?.ElementAtOrDefault(0);
                float? startHeight = position?.ElementAtOrDefault(1);

                Vector3 movestartposcopy = moveStartPos;
                Vector3 moveendposcopy = moveEndPos;
                Vector3 jumpendposcopy = jumpEndPos;

                if (position != null || flipLineIndex != null || njs.HasValue || spawnoffset.HasValue)
                {
                    GetNoteJumpValues(njs, spawnoffset, out float _, out float localJumpDistance, out Vector3 localMoveStartPos, out Vector3 localMoveEndPos, out Vector3 localJumpEndPos);

                    float localNoteJumpMovementSpeed = njs ?? _noteJumpMovementSpeed;

                    // NoteLineLayer.Base == noteData.startNoteLineLayer
                    // we avoid some math where the base game avoids spawning stacked notes together
                    Vector3 noteOffset = GetNoteOffset(noteData, startRow, (float)NoteLineLayer.Base);

                    float startLayerLineYPos = beatmapObjectSpawnMovementData.LineYPosForLineLayer(NoteLineLayer.Base);
                    float lineYPos = LineYPosForLineLayer(noteData, startHeight);
                    // Magic numbers below found with linear regression y=mx+b using existing HighestJumpPosYForLineLayer values
                    float highestJump = startHeight.HasValue ? ((0.875f * lineYPos) + 0.639583f) + _jumpOffsetY :
                        beatmapObjectSpawnMovementData.HighestJumpPosYForLineLayer(noteData.noteLineLayer);
                    jumpGravity = 2f * (highestJump - startLayerLineYPos) /
                        Mathf.Pow(localJumpDistance / localNoteJumpMovementSpeed * 0.5f, 2f);

                    jumpEndPos = localJumpEndPos + noteOffset;

                    // IsBasicNote() check is skipped so bombs can flip too
                    Vector3 noteOffset2 = GetNoteOffset(noteData, flipLineIndex ?? startRow, (float)NoteLineLayer.Base);
                    moveStartPos = localMoveStartPos + noteOffset2;
                    moveEndPos = localMoveEndPos + noteOffset2;
                }
            }
        }
    }
}