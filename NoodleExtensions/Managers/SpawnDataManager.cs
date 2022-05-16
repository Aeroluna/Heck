using Heck;
using IPA.Utilities;
using JetBrains.Annotations;
using NoodleExtensions.HarmonyPatches.SmallFixes;
using UnityEngine;
using Zenject;

namespace NoodleExtensions.Managers
{
    [UsedImplicitly]
    internal class SpawnDataManager
    {
        // these are the fields that dont have a property
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _startHalfJumpDurationInBeatsAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_startHalfJumpDurationInBeats");
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _maxHalfJumpDistanceAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_maxHalfJumpDistance");
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _noteJumpStartBeatOffsetAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_noteJumpStartBeatOffset");
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, Vector3>.Accessor _forwardVecAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, Vector3>.GetAccessor("_forwardVec");
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, Vector3>.Accessor _rightVecAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, Vector3>.GetAccessor("_rightVec");
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _moveDistanceAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_moveDistance");
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _obstacleTopPosYAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_obstacleTopPosY");

        private readonly BeatmapObjectSpawnController.InitData _initData;
        private readonly DeserializedData _deserializedData;
        private BeatmapObjectSpawnMovementData _movementData;

        private SpawnDataManager(
            InitializedSpawnMovementData initializedSpawnMovementData,
            BeatmapObjectSpawnController.InitData initData,
            [Inject(Id = NoodleController.ID)] DeserializedData deserializedData)
        {
            _initData = initData;
            _movementData = initializedSpawnMovementData.MovementData;
            _deserializedData = deserializedData;
        }

        internal bool GetObstacleSpawnData(ObstacleData obstacleData, ref BeatmapObjectSpawnMovementData.ObstacleSpawnData result)
        {
            if (!_deserializedData.Resolve(obstacleData, out NoodleObstacleData? noodleData))
            {
                return true;
            }

            float? njs = noodleData.NJS;
            float? spawnoffset = noodleData.SpawnOffset;

            float? startX = noodleData.StartX;
            float? startY = noodleData.StartY;

            float? height = noodleData.Height;

            Vector3 obstacleOffset = GetObstacleOffset(startX, startY, obstacleData.lineIndex, obstacleData.lineLayer);
            obstacleOffset.y += _movementData.jumpOffsetY;

            // original code has this line, not sure how important it is
            ////obstacleOffset.y = Mathf.Max(obstacleOffset.y, this._verticalObstaclePosY);

            float obstacleHeight;
            if (height.HasValue)
            {
                obstacleHeight = height.Value * StaticBeatmapObjectSpawnMovementData.layerHeight;
            }
            else
            {
                // _topObstaclePosY =/= _obstacleTopPosY
                obstacleHeight = Mathf.Min(
                    obstacleData.height * StaticBeatmapObjectSpawnMovementData.layerHeight,
                    _obstacleTopPosYAccessor(ref _movementData) - obstacleOffset.y);
            }

            GetNoteJumpValues(
                njs,
                spawnoffset,
                out float jumpDuration,
                out _,
                out Vector3 moveStartPos,
                out Vector3 moveEndPos,
                out Vector3 jumpEndPos);

            result = new BeatmapObjectSpawnMovementData.ObstacleSpawnData(
                moveStartPos + obstacleOffset,
                moveEndPos + obstacleOffset,
                jumpEndPos + obstacleOffset,
                obstacleHeight,
                _movementData.moveDuration,
                jumpDuration,
                _movementData.noteLinesDistance);

            // for definite position
            noodleData.InternalNoteOffset = _movementData.centerPos + obstacleOffset;
            float? width = noodleData.Width;
            noodleData.InternalXOffset = ((width.GetValueOrDefault(obstacleData.lineIndex) / 2f) - 0.5f) * _movementData.noteLinesDistance;

            return false;
        }

        internal bool GetJumpingNoteSpawnData(NoteData noteData, ref BeatmapObjectSpawnMovementData.NoteSpawnData result)
        {
            if (!_deserializedData.Resolve(noteData, out NoodleNoteData? noodleData))
            {
                return true;
            }

            float? flipLineIndex = noodleData.InternalFlipLineIndex;
            float? njs = noodleData.NJS;
            float? spawnoffset = noodleData.SpawnOffset;
            float? startlinelayer = noodleData.InternalStartNoteLineLayer;

            bool gravityOverride = noodleData.DisableGravity;

            float? startX = noodleData.StartX;
            float? startY = noodleData.StartY;

            Vector3 noteOffset = GetNoteOffset(startX, startlinelayer, noteData.lineIndex, noteData.beforeJumpNoteLineLayer);
            GetNoteJumpValues(
                njs,
                spawnoffset,
                out float jumpDuration,
                out float jumpDistance,
                out Vector3 moveStartPos,
                out Vector3 moveEndPos,
                out Vector3 jumpEndPos);

            float lineYPos = LineYPosForLineLayer(startY, noteData.noteLineLayer);
            float startLayerLineYPos = LineYPosForLineLayer(startlinelayer, noteData.beforeJumpNoteLineLayer);

            // HighestJumpPosYForLineLayer
            // Magic numbers below found with linear regression y=mx+b using existing HighestJumpPosYForLineLayer values
            float highestJump = startY.HasValue ? (0.875f * lineYPos) + 0.639583f + _movementData.jumpOffsetY :
                _movementData.HighestJumpPosYForLineLayer(noteData.noteLineLayer);

            // NoteJumpGravityForLineLayer
            float num = jumpDistance / (njs ?? _movementData.noteJumpMovementSpeed) * 0.5f;
            num = 2 / (num * num);

            float GetJumpGravity(float gravityLineYPos) => (highestJump - gravityLineYPos) * num;
            float jumpGravity = GetJumpGravity(startLayerLineYPos);

            Vector3 noteOffset2 = GetNoteOffset(
                flipLineIndex ?? startX,
                gravityOverride ? startY : startlinelayer,
                noteData.flipLineIndex,
                gravityOverride ? noteData.noteLineLayer : noteData.beforeJumpNoteLineLayer);

            result = new BeatmapObjectSpawnMovementData.NoteSpawnData(
                moveStartPos + noteOffset2,
                moveEndPos + noteOffset2,
                jumpEndPos + noteOffset,
                gravityOverride ? GetJumpGravity(lineYPos) : jumpGravity,
                _movementData.moveDuration,
                jumpDuration);

            // DEFINITE POSITION IS WEIRD, OK?
            // fuck
            float num2 = jumpDuration * 0.5f;
            float startVerticalVelocity = jumpGravity * num2;
            float yOffset = (startVerticalVelocity * num2) - (jumpGravity * num2 * num2 * 0.5f);
            noodleData.InternalNoteOffset = _movementData.centerPos + noteOffset + new Vector3(0, yOffset, 0);

            return false;
        }

        internal float GetSpawnAheadTime(float? inputNjs, float? inputOffset)
        {
            return _movementData.moveDuration + (GetJumpDuration(inputNjs, inputOffset) * 0.5f);
        }

        private Vector3 GetNoteOffset(float? startX, float? startY, int noteLineIndex, NoteLineLayer noteLineLayer)
        {
            int noteLinesCount = _movementData.noteLinesCount;

            // Add last part to simulate https://github.com/spookyGh0st/beatwalls/#wall
            float distance = (-(noteLinesCount - 1f) * 0.5f) + (startX.HasValue ? noteLinesCount / 2f : 0);
            float lineIndex = startX.GetValueOrDefault(noteLineIndex);
            distance = (distance + lineIndex) * _movementData.noteLinesDistance;

            return (_rightVecAccessor(ref _movementData) * distance)
                   + new Vector3(0, LineYPosForLineLayer(startY, noteLineLayer), 0);
        }

        private Vector3 GetObstacleOffset(float? startX, float? startY, int noteLineIndex, NoteLineLayer noteLineLayer)
        {
            Vector3 result = GetNoteOffset(startX, startY, noteLineIndex, noteLineLayer);
            result.y -= 0.15f;
            return result;
        }

        private float LineYPosForLineLayer(float? height, NoteLineLayer noteLineLayer)
        {
            if (height.HasValue)
            {
                return StaticBeatmapObjectSpawnMovementData.kBaseLinesYPos
                       + (height.Value * _movementData.noteLinesDistance); // offset by 0.25
            }

            return StaticBeatmapObjectSpawnMovementData.LineYPosForLineLayer(noteLineLayer);
        }

        private float GetJumpDuration(
            float? inputNjs,
            float? inputOffset)
        {
            if (!inputNjs.HasValue && !inputOffset.HasValue && _initData.noteJumpValueType == BeatmapObjectSpawnMovementData.NoteJumpValueType.JumpDuration)
            {
                return _movementData.jumpDuration;
            }

            float oneBeatDuration = _initData.beatsPerMinute.OneBeatDuration();
            float halfJumpDurationInBeats = CoreMathUtils.CalculateHalfJumpDurationInBeats(
                _startHalfJumpDurationInBeatsAccessor(ref _movementData),
                _maxHalfJumpDistanceAccessor(ref _movementData),
                inputNjs ?? _movementData.noteJumpMovementSpeed,
                oneBeatDuration,
                inputOffset ?? _noteJumpStartBeatOffsetAccessor(ref _movementData));
            return oneBeatDuration * halfJumpDurationInBeats * 2f;
        }

        private void GetNoteJumpValues(
            float? inputNjs,
            float? inputOffset,
            out float jumpDuration,
            out float jumpDistance,
            out Vector3 moveStartPos,
            out Vector3 moveEndPos,
            out Vector3 jumpEndPos)
        {
            jumpDuration = GetJumpDuration(inputNjs, inputOffset);

            Vector3 centerPos = _movementData.centerPos;
            Vector3 forwardVec = _forwardVecAccessor(ref _movementData);

            jumpDistance = (inputNjs ?? _movementData.noteJumpMovementSpeed) * jumpDuration;
            moveEndPos = centerPos + (forwardVec * (jumpDistance * 0.5f));
            jumpEndPos = centerPos - (forwardVec * (jumpDistance * 0.5f));
            moveStartPos = centerPos + (forwardVec * (_moveDistanceAccessor(ref _movementData) + (jumpDistance * 0.5f)));
        }
    }
}
