namespace NoodleExtensions.HarmonyPatches
{
    using IPA.Utilities;
    using UnityEngine;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;

    internal static class SpawnDataHelper
    {
        internal static Vector3 GetNoteOffset(BeatmapObjectData beatmapObjectData, float? startRow, float? startHeight)
        {
            float distance = (-(NoteLinesCount - 1f) * 0.5f) + (startRow.HasValue ? NoteLinesCount / 2f : 0); // Add last part to simulate https://github.com/spookyGh0st/beatwalls/#wall
            float lineIndex = startRow.GetValueOrDefault(beatmapObjectData.lineIndex);
            distance = (distance + lineIndex) * NoteLinesDistance;

            return (RightVec * distance)
                + new Vector3(0, LineYPosForLineLayer(beatmapObjectData, startHeight), 0);
        }

        internal static float LineYPosForLineLayer(BeatmapObjectData beatmapObjectData, float? height)
        {
            float ypos = BaseLinesYPos;
            if (height.HasValue)
            {
                ypos = (height.Value * NoteLinesDistance) + BaseLinesYPos; // offset by 0.25
            }
            else if (beatmapObjectData is NoteData noteData)
            {
                ypos = BeatmapObjectSpawnMovementData.LineYPosForLineLayer(noteData.noteLineLayer);
            }

            return ypos;
        }

        internal static void GetNoteJumpValues(
            float? inputNoteJumpMovementSpeed,
            float? inputNoteJumpStartBeatOffset,
            out float localJumpDuration,
            out float localJumpDistance,
            out Vector3 localMoveStartPos,
            out Vector3 localMoveEndPos,
            out Vector3 localJumpEndPos)
        {
            float localNoteJumpMovementSpeed = inputNoteJumpMovementSpeed ?? NoteJumpMovementSpeed;
            float localNoteJumpStartBeatOffset = inputNoteJumpStartBeatOffset ?? NoteJumpStartBeatOffset;
            float num = 60f / StartBPM;
            float num2 = StartHalfJumpDurationInBeats;
            while (localNoteJumpMovementSpeed * num * num2 > MaxHalfJumpDistance)
            {
                num2 /= 2f;
            }

            num2 += localNoteJumpStartBeatOffset;
            if (num2 < 1f)
            {
                num2 = 1f;
            }

            localJumpDuration = num * num2 * 2f;
            localJumpDistance = localNoteJumpMovementSpeed * localJumpDuration;
            localMoveStartPos = CenterPos + (ForwardVec * (MoveDistance + (localJumpDistance * 0.5f)));
            localMoveEndPos = CenterPos + (ForwardVec * localJumpDistance * 0.5f);
            localJumpEndPos = CenterPos - (ForwardVec * localJumpDistance * 0.5f);
        }

        internal static void InitBeatmapObjectSpawnController(BeatmapObjectSpawnMovementData beatmapObjectSpawnMovementData)
        {
            BeatmapObjectSpawnMovementData = beatmapObjectSpawnMovementData;
        }

        internal static class BeatmapObjectSpawnMovementDataVariables
        {
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _startBPMAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_startBPM");
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _topObstaclePosYAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_topObstaclePosY");
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _jumpOffsetYAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_jumpOffsetY");
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _verticalObstaclePosYAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_verticalObstaclePosY");
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _moveDistanceAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_moveDistance");
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _noteJumpMovementSpeedAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_noteJumpMovementSpeed");
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _noteJumpStartBeatOffsetAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_noteJumpStartBeatOffset");
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _noteLinesDistanceAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_noteLinesDistance");
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _baseLinesYPosAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_baseLinesYPos");
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _noteLinesCountAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_noteLinesCount");
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, Vector3>.Accessor _centerPosAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, Vector3>.GetAccessor("_centerPos");
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, Vector3>.Accessor _forwardVecAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, Vector3>.GetAccessor("_forwardVec");
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, Vector3>.Accessor _rightVecAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, Vector3>.GetAccessor("_rightVec");
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _startHalfJumpDurationInBeatsAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_startHalfJumpDurationInBeats");
            private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _maxHalfJumpDistanceAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_maxHalfJumpDistance");
            private static BeatmapObjectSpawnMovementData _beatmapObjectSpawnMovementData;

            internal static BeatmapObjectSpawnMovementData BeatmapObjectSpawnMovementData { get => _beatmapObjectSpawnMovementData; set => _beatmapObjectSpawnMovementData = value; }

            internal static float StartBPM => _startBPMAccessor(ref _beatmapObjectSpawnMovementData);

            internal static float TopObstaclePosY => _topObstaclePosYAccessor(ref _beatmapObjectSpawnMovementData);

            internal static float JumpOffsetY => _jumpOffsetYAccessor(ref _beatmapObjectSpawnMovementData);

            internal static float VerticalObstaclePosY => _verticalObstaclePosYAccessor(ref _beatmapObjectSpawnMovementData);

            internal static float MoveDistance => _moveDistanceAccessor(ref _beatmapObjectSpawnMovementData);

            internal static float NoteJumpMovementSpeed => _noteJumpMovementSpeedAccessor(ref _beatmapObjectSpawnMovementData);

            internal static float NoteJumpStartBeatOffset => _noteJumpStartBeatOffsetAccessor(ref _beatmapObjectSpawnMovementData);

            internal static float NoteLinesDistance => _noteLinesDistanceAccessor(ref _beatmapObjectSpawnMovementData);

            internal static float BaseLinesYPos => _baseLinesYPosAccessor(ref _beatmapObjectSpawnMovementData);

            internal static float NoteLinesCount => _noteLinesCountAccessor(ref _beatmapObjectSpawnMovementData);

            internal static Vector3 CenterPos => _centerPosAccessor(ref _beatmapObjectSpawnMovementData);

            internal static Vector3 ForwardVec => _forwardVecAccessor(ref _beatmapObjectSpawnMovementData);

            internal static Vector3 RightVec => _rightVecAccessor(ref _beatmapObjectSpawnMovementData);

            internal static float StartHalfJumpDurationInBeats => _startHalfJumpDurationInBeatsAccessor(ref _beatmapObjectSpawnMovementData);

            internal static float MaxHalfJumpDistance => _maxHalfJumpDistanceAccessor(ref _beatmapObjectSpawnMovementData);
        }
    }
}
