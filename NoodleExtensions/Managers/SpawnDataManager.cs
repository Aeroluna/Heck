using Heck;
using IPA.Utilities;
using JetBrains.Annotations;
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
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, BeatmapObjectSpawnMovementData.NoteJumpValueType>.Accessor _noteJumpValueTypeAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, BeatmapObjectSpawnMovementData.NoteJumpValueType>.GetAccessor("_noteJumpValueType");
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, Vector3>.Accessor _forwardVecAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, Vector3>.GetAccessor("_forwardVec");
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, Vector3>.Accessor _rightVecAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, Vector3>.GetAccessor("_rightVec");
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _moveDistanceAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_moveDistance");
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _baseLinesYPosAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_baseLinesYPos");
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _jumpOffsetYAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_jumpOffsetY");
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _verticalObstaclePosYAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_verticalObstaclePosY");
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _topObstaclePosYAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_topObstaclePosY");
        private static readonly FieldAccessor<BeatmapObjectSpawnMovementData, float>.Accessor _obstacleTopPosYAccessor = FieldAccessor<BeatmapObjectSpawnMovementData, float>.GetAccessor("_obstacleTopPosY");

        private readonly IBeatmapObjectSpawnController _spawnController;
        private readonly CustomData _customData;
        private BeatmapObjectSpawnMovementData _movementData;

        private SpawnDataManager(
            IBeatmapObjectSpawnController beatmapObjectSpawnController,
            [Inject(Id = NoodleController.ID)] CustomData customData)
        {
            _spawnController = beatmapObjectSpawnController;
            _movementData = beatmapObjectSpawnController.beatmapObjectSpawnMovementData;
            _customData = customData;
        }

        internal bool GetObstacleSpawnData(ObstacleData obstacleData, ref BeatmapObjectSpawnMovementData.ObstacleSpawnData result)
        {
            if (!_customData.Resolve(obstacleData, out NoodleObstacleData? noodleData))
            {
                return true;
            }

            float? njs = noodleData.NJS;
            float? spawnoffset = noodleData.SpawnOffset;

            float? startX = noodleData.StartX;
            float? startY = noodleData.StartY;

            float? height = noodleData.Height;

            Vector3 noteOffset = GetNoteOffset(startX, startY, obstacleData.lineIndex, NoteLineLayer.Base);

            if (startY.HasValue)
            {
                noteOffset.y = _verticalObstaclePosYAccessor(ref _movementData) + (startY.Value * _movementData.noteLinesDistance);
            }
            else
            {
                noteOffset.y = obstacleData.obstacleType == ObstacleType.Top
                    ? (_topObstaclePosYAccessor(ref _movementData) + _jumpOffsetYAccessor(ref _movementData))
                    : _verticalObstaclePosYAccessor(ref _movementData);
            }

            float obstacleHeight;
            if (height.HasValue)
            {
                obstacleHeight = height.Value * _movementData.noteLinesDistance;
            }
            else
            {
                // _topObstaclePosY =/= _obstacleTopPosY
                obstacleHeight = _obstacleTopPosYAccessor(ref _movementData) - noteOffset.y;
            }

            GetNoteJumpValues(
                njs,
                spawnoffset,
                out float jumpDuration,
                out _,
                out Vector3 localMoveStartPos,
                out Vector3 localMoveEndPos,
                out Vector3 localJumpEndPos);
            Vector3 moveStartPos = localMoveStartPos + noteOffset;
            Vector3 moveEndPos = localMoveEndPos + noteOffset;
            Vector3 jumpEndPos = localJumpEndPos + noteOffset;

            result = new BeatmapObjectSpawnMovementData.ObstacleSpawnData(
                moveStartPos,
                moveEndPos,
                jumpEndPos,
                obstacleHeight,
                _movementData.moveDuration,
                jumpDuration,
                _movementData.noteLinesDistance);

            noodleData.NoteOffset = _movementData.centerPos + noteOffset;
            float? width = noodleData.Width;
            noodleData.XOffset = ((width.GetValueOrDefault(obstacleData.lineIndex) / 2f) - 0.5f) * _movementData.noteLinesDistance;

            return false;
        }

        internal bool GetJumpingNoteSpawnData(NoteData noteData, ref BeatmapObjectSpawnMovementData.NoteSpawnData result)
        {
            if (!_customData.Resolve(noteData, out NoodleNoteData? noodleData))
            {
                return true;
            }

            float? flipLineIndex = noodleData.FlipLineIndexInternal;
            float njs = noodleData.NJS ?? _movementData.noteJumpMovementSpeed;
            float? spawnoffset = noodleData.SpawnOffset;
            float? startlinelayer = noodleData.StartNoteLineLayerInternal;

            bool gravityOverride = noodleData.DisableGravity;

            float? startX = noodleData.StartX;
            float? startY = noodleData.StartY;

            float jumpOffsetY = _jumpOffsetYAccessor(ref _movementData);

            Vector3 noteOffset = GetNoteOffset(startX, startlinelayer, noteData.lineIndex, noteData.beforeJumpNoteLineLayer);
            GetNoteJumpValues(
                njs,
                spawnoffset,
                out float jumpDuration,
                out float jumpDistance,
                out Vector3 localMoveStartPos,
                out Vector3 localMoveEndPos,
                out Vector3 localJumpEndPos);

            float lineYPos = LineYPosForLineLayer(startY, noteData.noteLineLayer);
            float startLayerLineYPos = LineYPosForLineLayer(startlinelayer, noteData.beforeJumpNoteLineLayer);

            // HighestJumpPosYForLineLayer
            // Magic numbers below found with linear regression y=mx+b using existing HighestJumpPosYForLineLayer values
            float highestJump = startY.HasValue ? (0.875f * lineYPos) + 0.639583f + jumpOffsetY :
                _movementData.HighestJumpPosYForLineLayer(noteData.noteLineLayer);

            // NoteJumpGravityForLineLayer
            float num = jumpDistance / njs * 0.5f;
            num = 2 / (num * num);

            float GetJumpGravity(float gravityLineYPos) => (highestJump - gravityLineYPos) * num;
            float jumpGravity = GetJumpGravity(startLayerLineYPos);

            Vector3 jumpEndPos = localJumpEndPos + noteOffset;
            Vector3 moveStartPos;
            Vector3 moveEndPos;

            // note duration???
            if (noteData.duration == 0f)
            {
                Vector3 noteOffset2 = GetNoteOffset(
                    flipLineIndex ?? startX,
                    gravityOverride ? startY : startlinelayer,
                    noteData.flipLineIndex,
                    gravityOverride ? noteData.noteLineLayer : noteData.beforeJumpNoteLineLayer);
                moveStartPos = localMoveStartPos + noteOffset2;
                moveEndPos = localMoveEndPos + noteOffset2;
            }
            else
            {
                moveStartPos = localMoveStartPos + noteOffset;
                moveEndPos = localMoveEndPos + noteOffset;
            }

            result = new BeatmapObjectSpawnMovementData.NoteSpawnData(
                moveStartPos,
                moveEndPos,
                jumpEndPos,
                gravityOverride ? GetJumpGravity(lineYPos) : jumpGravity,
                _movementData.moveDuration,
                jumpDuration);

            // DEFINITE POSITION IS WEIRD, OK?
            // fuck
            float num2 = jumpDuration * 0.5f;
            float startVerticalVelocity = jumpGravity * num2;
            float yOffset = (startVerticalVelocity * num2) - (jumpGravity * num2 * num2 * 0.5f);
            noodleData.NoteOffset = _movementData.centerPos + noteOffset + new Vector3(0, yOffset, 0);

            return false;
        }

        internal float GetSpawnAheadTime(float? inputNjs, float? inputOffset, float bpm)
        {
            return _movementData.moveDuration + (GetJumpDuration(inputNjs, inputOffset, bpm) * 0.5f);
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

        private float LineYPosForLineLayer(float? height, NoteLineLayer noteLineLayer)
        {
            if (height.HasValue)
            {
                return _baseLinesYPosAccessor(ref _movementData) + (height.Value * _movementData.noteLinesDistance); // offset by 0.25
            }

            return _movementData.LineYPosForLineLayer(noteLineLayer);
        }

        private float GetJumpDuration(
            float? inputNjs,
            float? inputOffset,
            float bpm)
        {
            float noteJumpMovementSpeed = inputNjs ?? _movementData.noteJumpMovementSpeed;
            float noteJumpStartBeatOffset = inputOffset ?? _noteJumpStartBeatOffsetAccessor(ref _movementData);
            float oneBeatDuration = bpm.OneBeatDuration();
            if (_noteJumpValueTypeAccessor(ref _movementData) != BeatmapObjectSpawnMovementData.NoteJumpValueType.BeatOffset)
            {
                return _movementData.jumpDuration;
            }

            float halfJumpDurationInBeats = CoreMathUtils.CalculateHalfJumpDurationInBeats(
                _startHalfJumpDurationInBeatsAccessor(ref _movementData),
                _maxHalfJumpDistanceAccessor(ref _movementData),
                noteJumpMovementSpeed,
                oneBeatDuration,
                noteJumpStartBeatOffset);
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
            float noteJumpMovementSpeed = inputNjs ?? _movementData.noteJumpMovementSpeed;
            jumpDuration = GetJumpDuration(noteJumpMovementSpeed, inputOffset, _spawnController.currentBpm);

            Vector3 centerPos = _movementData.centerPos;
            Vector3 forwardVec = _forwardVecAccessor(ref _movementData);

            jumpDistance = noteJumpMovementSpeed * jumpDuration;
            moveEndPos = centerPos + (forwardVec * (jumpDistance * 0.5f));
            jumpEndPos = centerPos - (forwardVec * (jumpDistance * 0.5f));
            moveStartPos = centerPos + (forwardVec * (_moveDistanceAccessor(ref _movementData) + (jumpDistance * 0.5f)));
        }
    }
}
