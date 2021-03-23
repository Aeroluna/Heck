﻿namespace NoodleExtensions.HarmonyPatches
{
    using UnityEngine;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
    using static NoodleExtensions.NoodleObjectDataManager;

    [NoodlePatch(typeof(BeatmapObjectSpawnMovementData))]
    [NoodlePatch("GetObstacleSpawnData")]
    internal static class BeatmapObjectSpawnMovementDataGetObstacleSpawnData
    {
        private static void Postfix(Vector3 ____centerPos, ObstacleData obstacleData, ref BeatmapObjectSpawnMovementData.ObstacleSpawnData __result)
        {
            if (!NoodleObjectDatas.TryGetValue(obstacleData, out NoodleObjectData noodleObjectData))
            {
                return;
            }

            NoodleObstacleData noodleData = (NoodleObstacleData)noodleObjectData;

            float? njs = noodleData.NJS;
            float? spawnoffset = noodleData.SpawnOffset;

            float? startX = noodleData.StartX;
            float? startY = noodleData.StartY;

            float? height = noodleData.Height;

            Vector3? finalNoteOffset = null;

            Vector3 moveStartPos = __result.moveStartPos;
            Vector3 moveEndPos = __result.moveEndPos;
            Vector3 jumpEndPos = __result.jumpEndPos;
            float obstacleHeight = __result.obstacleHeight;
            GetNoteJumpValues(njs, spawnoffset, out float jumpDuration, out float _, out Vector3 localMoveStartPos, out Vector3 localMoveEndPos, out Vector3 localJumpEndPos);

            // Actual wall stuff
            if (startX.HasValue || startY.HasValue || njs.HasValue || spawnoffset.HasValue)
            {
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

            noodleData.NoteOffset = ____centerPos + finalNoteOffset.Value;
            float? width = noodleData.Width;
            noodleData.XOffset = ((width.GetValueOrDefault(obstacleData.lineIndex) / 2f) - 0.5f) * NoteLinesDistance;
        }
    }

    [NoodlePatch(typeof(BeatmapObjectSpawnMovementData))]
    [NoodlePatch("GetJumpingNoteSpawnData")]
    internal static class BeatmapObjectSpawnMovementDataGetJumpingNoteSpawnData
    {
        private static void Postfix(BeatmapObjectSpawnMovementData __instance, Vector3 ____centerPos, float ____jumpDuration, NoteData noteData, ref BeatmapObjectSpawnMovementData.NoteSpawnData __result)
        {
            if (!NoodleObjectDatas.TryGetValue(noteData, out NoodleObjectData noodleObjectData))
            {
                return;
            }

            NoodleNoteData noodleData = (NoodleNoteData)noodleObjectData;

            float? flipLineIndex = noodleData.FlipLineIndexInternal;
            float? njs = noodleData.NJS;
            float? spawnoffset = noodleData.SpawnOffset;
            float? startlinelayer = noodleData.StartNoteLineLayerInternal;

            bool gravityOverride = noodleData.DisableGravity;

            float? startRow = noodleData.StartX;
            float? startHeight = noodleData.StartY;

            float jumpDuration = ____jumpDuration;

            ////Vector3 moveStartPos = __result.moveStartPos;
            ////Vector3 moveEndPos = __result.moveEndPos;
            ////Vector3 jumpEndPos = __result.jumpEndPos;
            float jumpGravity = __result.jumpGravity;

            Vector3 noteOffset = GetNoteOffset(noteData, startRow, startlinelayer ?? (float)noteData.startNoteLineLayer);

            if (startRow.HasValue || startHeight.HasValue || flipLineIndex.HasValue || njs.HasValue || spawnoffset.HasValue || startlinelayer.HasValue || gravityOverride)
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

                Vector3 jumpEndPos = localJumpEndPos + noteOffset;

                // IsBasicNote() check is skipped so bombs can flip too
                Vector3 noteOffset2 = GetNoteOffset(noteData, flipLineIndex ?? startRow, gravityOverride ? startHeight : startlinelayer ?? (float)noteData.startNoteLineLayer);
                Vector3 moveStartPos = localMoveStartPos + noteOffset2;
                Vector3 moveEndPos = localMoveEndPos + noteOffset2;

                __result = new BeatmapObjectSpawnMovementData.NoteSpawnData(moveStartPos, moveEndPos, jumpEndPos, jumpGravity, __result.moveDuration, jumpDuration);
            }

            // DEFINITE POSITION IS WEIRD, OK?
            // ty reaxt
            float startVerticalVelocity = jumpGravity * jumpDuration * 0.5f;
            float num = jumpDuration * 0.5f;
            float yOffset = (startVerticalVelocity * num) - (jumpGravity * num * num * 0.5f);
            noodleData.NoteOffset = ____centerPos + noteOffset + new Vector3(0, yOffset, 0);
        }
    }
}
