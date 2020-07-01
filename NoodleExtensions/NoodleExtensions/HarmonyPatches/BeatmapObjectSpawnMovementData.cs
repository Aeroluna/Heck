namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using UnityEngine;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
    using static NoodleExtensions.Plugin;

    [HarmonyPatch(typeof(BeatmapObjectSpawnMovementData))]
    [HarmonyPatch("Init")]
    internal class BeatmapObjectSpawnMovementDataInit
    {
#pragma warning disable SA1313
        private static void Postfix(BeatmapObjectSpawnMovementData __instance)
#pragma warning restore SA1313
        {
            InitBeatmapObjectSpawnController(__instance);
        }
    }

    [NoodlePatch(typeof(BeatmapObjectSpawnMovementData))]
    [NoodlePatch("GetObstacleSpawnMovementData")]
    internal class BeatmapObjectSpawnMovementDataGetObstacleSpawnMovementData
    {
#pragma warning disable SA1313
        private static void Postfix(Vector3 ____centerPos, ObstacleData obstacleData, ref Vector3 moveStartPos, ref Vector3 moveEndPos, ref Vector3 jumpEndPos, ref float obstacleHeight)
#pragma warning restore SA1313
        {
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float?> position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                IEnumerable<float?> scale = ((List<object>)Trees.at(dynData, SCALE))?.Select(n => n.ToNullableFloat());
                float? njs = (float?)Trees.at(dynData, NOTEJUMPSPEED);
                float? spawnoffset = (float?)Trees.at(dynData, NOTESPAWNOFFSET);

                float? startX = position?.ElementAtOrDefault(0);
                float? startY = position?.ElementAtOrDefault(1);

                float? height = scale?.ElementAtOrDefault(1);

                Vector3? finalNoteOffset = null;

                // Actual wall stuff
                if (startX.HasValue || startY.HasValue || njs.HasValue || spawnoffset.HasValue)
                {
                    GetNoteJumpValues(njs, spawnoffset, out float _, out float _, out Vector3 localMoveStartPos, out Vector3 localMoveEndPos, out Vector3 localJumpEndPos);

                    // Ripped from base game
                    Vector3 noteOffset = GetNoteOffset(obstacleData, startX, null);
                    noteOffset.y = startY.HasValue ? VerticalObstaclePosY + (startY.GetValueOrDefault(0) * NoteLinesDistance) : ((obstacleData.obstacleType == ObstacleType.Top)
                        ? (TopObstaclePosY + JumpOffsetY) : VerticalObstaclePosY);

                    finalNoteOffset = noteOffset;

                    moveStartPos = localMoveStartPos + noteOffset;
                    moveEndPos = localMoveEndPos + noteOffset;
                    jumpEndPos = localJumpEndPos + noteOffset;
                }

                if (height.HasValue)
                {
                    obstacleHeight = height.Value * NoteLinesDistance;
                }

                if (!finalNoteOffset.HasValue)
                {
                    Vector3 noteOffset = GetNoteOffset(obstacleData, startX, null);
                    noteOffset.y = (obstacleData.obstacleType == ObstacleType.Top) ? (TopObstaclePosY + JumpOffsetY) : VerticalObstaclePosY;
                    finalNoteOffset = noteOffset;
                }

                dynData.noteOffset = ____centerPos + finalNoteOffset.Value;
                float? width = scale?.ElementAtOrDefault(0);
                dynData.xOffset = ((width.GetValueOrDefault(obstacleData.lineIndex) / 2f) - 0.5f) * NoteLinesDistance;
            }
        }
    }

    [NoodlePatch(typeof(BeatmapObjectSpawnMovementData))]
    [NoodlePatch("GetNoteSpawnMovementData")]
    internal class BeatmapObjectSpawnMovementDataGetNoteSpawnMovementData
    {
#pragma warning disable SA1313
        private static void Postfix(BeatmapObjectSpawnMovementData __instance, Vector3 ____centerPos, float ____jumpDuration, NoteData noteData, ref Vector3 moveStartPos, ref Vector3 moveEndPos, ref Vector3 jumpEndPos, ref float jumpGravity)
#pragma warning restore SA1313
        {
            if (noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float?> position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                float? flipLineIndex = (float?)Trees.at(dynData, "flipLineIndex");
                float? njs = (float?)Trees.at(dynData, NOTEJUMPSPEED);
                float? spawnoffset = (float?)Trees.at(dynData, NOTESPAWNOFFSET);
                float startlinelayer = (float?)Trees.at(dynData, "startNoteLineLayer") ?? (float)NoteLineLayer.Base;

                float? startRow = position?.ElementAtOrDefault(0);
                float? startHeight = position?.ElementAtOrDefault(1);

                float jumpDuration = ____jumpDuration;

                Vector3 noteOffset = GetNoteOffset(noteData, startRow, startlinelayer);

                if (position != null || flipLineIndex != null || njs.HasValue || spawnoffset.HasValue)
                {
                    GetNoteJumpValues(njs, spawnoffset, out float localJumpDuration, out float localJumpDistance, out Vector3 localMoveStartPos, out Vector3 localMoveEndPos, out Vector3 localJumpEndPos);
                    jumpDuration = localJumpDuration;

                    float localNoteJumpMovementSpeed = njs ?? NoteJumpMovementSpeed;

                    float startLayerLineYPos = LineYPosForLineLayer(noteData, startlinelayer);
                    float lineYPos = LineYPosForLineLayer(noteData, startHeight);

                    // Magic numbers below found with linear regression y=mx+b using existing HighestJumpPosYForLineLayer values
                    float highestJump = startHeight.HasValue ? ((0.875f * lineYPos) + 0.639583f) + JumpOffsetY :
                        __instance.HighestJumpPosYForLineLayer(noteData.noteLineLayer);
                    jumpGravity = 2f * (highestJump - startLayerLineYPos) /
                        Mathf.Pow(localJumpDistance / localNoteJumpMovementSpeed * 0.5f, 2f);

                    jumpEndPos = localJumpEndPos + noteOffset;

                    // IsBasicNote() check is skipped so bombs can flip too
                    Vector3 noteOffset2 = GetNoteOffset(noteData, flipLineIndex ?? startRow, startlinelayer);
                    moveStartPos = localMoveStartPos + noteOffset2;
                    moveEndPos = localMoveEndPos + noteOffset2;
                }

                // DEFINITE POSITION IS WEIRD, OK?
                // ty reaxt
                float startVerticalVelocity = jumpGravity * jumpDuration * 0.5f;
                float num = jumpDuration * 0.5f;
                float yOffset = (startVerticalVelocity * num) - (jumpGravity * num * num * 0.5f);
                dynData.noteOffset = ____centerPos + noteOffset + new Vector3(0, yOffset, 0);
            }
        }
    }
}
