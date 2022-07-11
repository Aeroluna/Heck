using Heck;
using IPA.Utilities;
using JetBrains.Annotations;
using NoodleExtensions.HarmonyPatches.SmallFixes;
using UnityEngine;
using Zenject;

namespace NoodleExtensions.Managers
{
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

        [UsedImplicitly]
        private SpawnDataManager(
            InitializedSpawnMovementData initializedSpawnMovementData,
            BeatmapObjectSpawnController.InitData initData,
            [Inject(Id = NoodleController.ID)] DeserializedData deserializedData)
        {
            _initData = initData;
            _movementData = initializedSpawnMovementData.MovementData;
            _deserializedData = deserializedData;
        }

        internal static Vector2 Get2DNoteOffset(float lineIndex, int noteLinesCount, float lineLayer)
        {
            float distance = -(noteLinesCount - 1f) * 0.5f;
            return new Vector2((distance + lineIndex) * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance, LineYPosForLineLayer(lineLayer));
        }

        internal bool GetObstacleSpawnData(ObstacleData obstacleData, ref BeatmapObjectSpawnMovementData.ObstacleSpawnData result)
        {
            if (!_deserializedData.Resolve(obstacleData, out NoodleObstacleData? noodleData))
            {
                return true;
            }

            float? njs = noodleData.NJS;
            float? spawnoffset = noodleData.SpawnOffset;

            float lineIndex = noodleData.StartX + (_movementData.noteLinesCount / 2) ?? obstacleData.lineIndex;
            float lineLayer = noodleData.StartY ?? (float)obstacleData.lineLayer;

            Vector3 obstacleOffset = GetObstacleOffset(lineIndex, lineLayer);
            obstacleOffset.y += _movementData.jumpOffsetY;

            float? height = noodleData.Height;
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
                StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance);

            return false;
        }

        internal bool GetJumpingNoteSpawnData(NoteData noteData, ref BeatmapObjectSpawnMovementData.NoteSpawnData result)
        {
            if (!_deserializedData.Resolve(noteData, out NoodleBaseNoteData? noodleData))
            {
                return true;
            }

            float? njs = noodleData.NJS;
            float? spawnoffset = noodleData.SpawnOffset;

            bool gravityOverride = noodleData.DisableGravity;

            float offset = _movementData.noteLinesCount / 2f;
            float? flipLineIndex = noodleData.InternalFlipLineIndex;
            float lineIndex = noodleData.StartX + offset ?? noteData.lineIndex;
            float lineLayer = noodleData.StartY ?? (float)noteData.noteLineLayer;
            float startlinelayer = noodleData.InternalStartNoteLineLayer ?? (float)noteData.beforeJumpNoteLineLayer;

            Vector3 noteOffset = GetNoteOffset(lineIndex, startlinelayer);
            GetNoteJumpValues(
                njs,
                spawnoffset,
                out float jumpDuration,
                out float jumpDistance,
                out Vector3 moveStartPos,
                out Vector3 moveEndPos,
                out Vector3 jumpEndPos);

            NoteJumpGravityForLineLayer(
                lineLayer,
                startlinelayer,
                jumpDistance,
                njs,
                out float jumpGravity,
                out float noGravity);

            Vector3 noteOffset2 = GetNoteOffset(
                flipLineIndex ?? lineIndex,
                gravityOverride ? lineLayer : startlinelayer);

            result = new BeatmapObjectSpawnMovementData.NoteSpawnData(
                moveStartPos + noteOffset2,
                moveEndPos + noteOffset2,
                jumpEndPos + noteOffset,
                gravityOverride ? noGravity : jumpGravity,
                _movementData.moveDuration,
                jumpDuration);

            return false;
        }

        internal bool GetSliderSpawnData(SliderData sliderData, ref BeatmapObjectSpawnMovementData.SliderSpawnData result)
        {
            if (!_deserializedData.Resolve(sliderData, out NoodleSliderData? noodleData))
            {
                return true;
            }

            float? njs = noodleData.NJS;
            float? spawnoffset = noodleData.SpawnOffset;

            bool gravityOverride = noodleData.DisableGravity;

            float offset = _movementData.noteLinesCount / 2f;
            float headLineIndex = noodleData.StartX + offset ?? sliderData.headLineIndex;
            float headLineLayer = noodleData.StartY ?? (float)sliderData.headLineLayer;
            float headStartlinelayer = noodleData.InternalStartNoteLineLayer + offset ?? (float)sliderData.headBeforeJumpLineLayer;
            float tailLineIndex = noodleData.TailStartX + offset ?? sliderData.tailLineIndex;
            float tailLineLayer = noodleData.TailStartY ?? (float)sliderData.tailLineLayer;
            float tailStartlinelayer = noodleData.InternalTailStartNoteLineLayer + offset ?? (float)sliderData.tailBeforeJumpLineLayer;

            Vector3 headOffset = GetNoteOffset(headLineIndex, headStartlinelayer);
            Vector3 tailOffset = GetNoteOffset(tailLineIndex, tailStartlinelayer);
            GetNoteJumpValues(
                njs,
                spawnoffset,
                out float jumpDuration,
                out float jumpDistance,
                out Vector3 moveStartPos,
                out Vector3 moveEndPos,
                out Vector3 jumpEndPos);

            NoteJumpGravityForLineLayer(
                headLineLayer,
                headStartlinelayer,
                jumpDistance,
                njs,
                out float headJumpGravity,
                out float headNoGravity);

            NoteJumpGravityForLineLayer(
                tailLineLayer,
                tailStartlinelayer,
                jumpDistance,
                njs,
                out float tailJumpGravity,
                out float tailNoGravity);

            result = new BeatmapObjectSpawnMovementData.SliderSpawnData(
                moveStartPos + headOffset,
                moveEndPos + headOffset,
                jumpEndPos + headOffset,
                gravityOverride ? headNoGravity : headJumpGravity,
                moveStartPos + tailOffset,
                moveEndPos + tailOffset,
                jumpEndPos + tailOffset,
                gravityOverride ? tailNoGravity : tailJumpGravity,
                _movementData.moveDuration,
                jumpDuration);

            return false;
        }

        internal float GetSpawnAheadTime(float? inputNjs, float? inputOffset)
        {
            return _movementData.moveDuration + (GetJumpDuration(inputNjs, inputOffset) * 0.5f);
        }

        private static float LineYPosForLineLayer(float height)
        {
            return StaticBeatmapObjectSpawnMovementData.kBaseLinesYPos
                   + (height * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance); // offset by 0.25
        }

        private Vector3 GetNoteOffset(float lineIndex, float lineLayer)
        {
            Vector2 coords = Get2DNoteOffset(lineIndex, _movementData.noteLinesCount, lineLayer);
            return (_rightVecAccessor(ref _movementData) * coords.x)
                   + new Vector3(0, coords.y, 0);
        }

        private Vector3 GetObstacleOffset(float lineIndex, float lineLayer)
        {
            Vector3 result = GetNoteOffset(lineIndex, lineLayer);
            result.y += StaticBeatmapObjectSpawnMovementData.kObstacleVerticalOffset;
            return result;
        }

        private void NoteJumpGravityForLineLayer(
            float lineLayer,
            float startLineLayer,
            float jumpDistance,
            float? njs,
            out float gravity,
            out float noGravity)
        {
            float lineYPos = LineYPosForLineLayer(lineLayer);
            float startLayerLineYPos = LineYPosForLineLayer(startLineLayer);

            // HighestJumpPosYForLineLayer
            // Magic numbers below found with linear regression y=mx+b using existing HighestJumpPosYForLineLayer values
            float highestJump = (0.875f * lineYPos) + 0.639583f + _movementData.jumpOffsetY;

            // NoteJumpGravityForLineLayer
            float num = jumpDistance / (njs ?? _movementData.noteJumpMovementSpeed) * 0.5f;
            num = 2 / (num * num);
            float GetJumpGravity(float gravityLineYPos) => (highestJump - gravityLineYPos) * num;
            gravity = GetJumpGravity(startLayerLineYPos);
            noGravity = GetJumpGravity(lineYPos);
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
