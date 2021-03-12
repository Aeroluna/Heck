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
    internal static class BeatmapObjectSpawnMovementDataInit
    {
        private static void Postfix(BeatmapObjectSpawnMovementData __instance)
        {
            InitBeatmapObjectSpawnController(__instance);
        }
    }

    [NoodlePatch(typeof(BeatmapObjectSpawnMovementData))]
    [NoodlePatch("GetObstacleSpawnData")]
    internal static class BeatmapObjectSpawnMovementDataGetObstacleSpawnData
    {
        private static void Postfix(Vector3 ____centerPos, ObstacleData obstacleData, ref BeatmapObjectSpawnMovementData.ObstacleSpawnData __result)
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

                Vector3 moveStartPos = __result.moveStartPos;
                Vector3 moveEndPos = __result.moveEndPos;
                Vector3 jumpEndPos = __result.jumpEndPos;
                float obstacleHeight = __result.obstacleHeight;
                GetNoteJumpValues(njs, spawnoffset, out float jumpDuration, out float _, out Vector3 _, out Vector3 _, out Vector3 _);

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

                __result = new BeatmapObjectSpawnMovementData.ObstacleSpawnData(moveStartPos, moveEndPos, jumpEndPos, obstacleHeight, __result.moveDuration, jumpDuration, __result.noteLinesDistance);

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
    [NoodlePatch("GetJumpingNoteSpawnData")]
    internal static class BeatmapObjectSpawnMovementDataGetJumpingNoteSpawnData
    {
        private static void Postfix(BeatmapObjectSpawnMovementData __instance, Vector3 ____centerPos, float ____jumpDuration, NoteData noteData, ref BeatmapObjectSpawnMovementData.NoteSpawnData __result)
        {
            if (noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float?> position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                float? flipLineIndex = (float?)Trees.at(dynData, "flipLineIndex");
                float? njs = (float?)Trees.at(dynData, NOTEJUMPSPEED);
                float? spawnoffset = (float?)Trees.at(dynData, NOTESPAWNOFFSET);
                float? startlinelayer = (float?)Trees.at(dynData, "startNoteLineLayer");

                bool gravityOverride = (bool?)Trees.at(dynData, NOTEGRAVITYDISABLE) ?? false;

                float? startRow = position?.ElementAtOrDefault(0);
                float? startHeight = position?.ElementAtOrDefault(1);

                float jumpDuration = ____jumpDuration;

                Vector3 moveStartPos = __result.moveStartPos;
                Vector3 moveEndPos = __result.moveEndPos;
                Vector3 jumpEndPos = __result.jumpEndPos;
                float jumpGravity = __result.jumpGravity;

                Vector3 noteOffset = GetNoteOffset(noteData, startRow, startlinelayer ?? (float)noteData.startNoteLineLayer);

                if (position != null || flipLineIndex != null || njs.HasValue || spawnoffset.HasValue || startlinelayer.HasValue || gravityOverride)
                {
                    GetNoteJumpValues(njs, spawnoffset, out float localJumpDuration, out float localJumpDistance, out Vector3 localMoveStartPos, out Vector3 localMoveEndPos, out Vector3 localJumpEndPos);
                    jumpDuration = localJumpDuration;

                    float localNoteJumpMovementSpeed = njs ?? NoteJumpMovementSpeed;

                    float startLayerLineYPos = LineYPosForLineLayer(noteData, startlinelayer ?? (float)noteData.startNoteLineLayer);
                    float lineYPos = LineYPosForLineLayer(noteData, startHeight);

                    // Magic numbers below found with linear regression y=mx+b using existing HighestJumpPosYForLineLayer values
                    float highestJump = startHeight.HasValue ? (0.875f * lineYPos) + 0.639583f + JumpOffsetY :
                        __instance.HighestJumpPosYForLineLayer(noteData.noteLineLayer);
                    jumpGravity = 2f * (highestJump - (gravityOverride ? lineYPos : startLayerLineYPos)) /
                        Mathf.Pow(localJumpDistance / localNoteJumpMovementSpeed * 0.5f, 2f);

                    jumpEndPos = localJumpEndPos + noteOffset;

                    // IsBasicNote() check is skipped so bombs can flip too
                    Vector3 noteOffset2 = GetNoteOffset(noteData, flipLineIndex ?? startRow, gravityOverride ? startHeight : startlinelayer ?? (float)noteData.startNoteLineLayer);
                    moveStartPos = localMoveStartPos + noteOffset2;
                    moveEndPos = localMoveEndPos + noteOffset2;

                    __result = new BeatmapObjectSpawnMovementData.NoteSpawnData(moveStartPos, moveEndPos, jumpEndPos, jumpGravity, __result.moveDuration, jumpDuration);
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
