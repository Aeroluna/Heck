using Heck;
using JetBrains.Annotations;
using NoodleExtensions.HarmonyPatches.SmallFixes;
using UnityEngine;
using Zenject;

namespace NoodleExtensions.Managers
{
    internal class SpawnDataManager
    {
        private readonly BeatmapObjectSpawnController.InitData _initData;
        private readonly DeserializedData _deserializedData;
        private readonly BeatmapObjectSpawnMovementData _movementData;

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

            bool? v2 = noodleData.V2;
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
                    _movementData._obstacleTopPosY - obstacleOffset.y);
            }

            GetNoteJumpValues(
                v2,
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

            bool? v2 = noodleData.V2;
            float? njs = noodleData.NJS;
            float? spawnoffset = noodleData.SpawnOffset;

            bool gravityOverride = noodleData.DisableGravity;

            float offset = _movementData.noteLinesCount / 2f;
            float? flipLineIndex = noodleData.InternalFlipLineIndex;
            float lineIndex = noodleData.StartX + offset ?? noteData.lineIndex;
            float lineLayer = noodleData.StartY ?? (float)noteData.noteLineLayer;
            float startlinelayer = noodleData.InternalStartNoteLineLayer;

            Vector3 noteOffset = GetNoteOffset(lineIndex, startlinelayer);
            GetNoteJumpValues(
                v2,
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

            bool? v2 = noodleData.V2;
            float? njs = noodleData.NJS;
            float? spawnoffset = noodleData.SpawnOffset;

            bool gravityOverride = noodleData.DisableGravity;

            float offset = _movementData.noteLinesCount / 2f;
            float headLineIndex = noodleData.StartX + offset ?? sliderData.headLineIndex;
            float headLineLayer = noodleData.StartY ?? (float)sliderData.headLineLayer;
            float headStartlinelayer = noodleData.InternalStartNoteLineLayer;
            float tailLineIndex = noodleData.TailStartX + offset ?? sliderData.tailLineIndex;
            float tailLineLayer = noodleData.TailStartY ?? (float)sliderData.tailLineLayer;
            float tailStartlinelayer = noodleData.InternalTailStartNoteLineLayer;

            Vector3 headOffset = GetNoteOffset(headLineIndex, gravityOverride ? headLineLayer : headStartlinelayer);
            Vector3 tailOffset = GetNoteOffset(tailLineIndex, gravityOverride ? tailLineLayer : tailStartlinelayer);
            GetNoteJumpValues(
                v2,
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

        internal float GetSpawnAheadTime(bool? v2, float? inputNjs, float? inputOffset)
        {
            return _movementData.moveDuration + (GetJumpDuration(v2, inputNjs, inputOffset) * 0.5f);
        }

        private static float LineYPosForLineLayer(float height)
        {
            return StaticBeatmapObjectSpawnMovementData.kBaseLinesYPos
                   + (height * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance); // offset by 0.25
        }

        private Vector3 GetNoteOffset(float lineIndex, float lineLayer)
        {
            Vector2 coords = Get2DNoteOffset(lineIndex, _movementData.noteLinesCount, lineLayer);
            return (_movementData._rightVec * coords.x)
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

        // We have to use our own implementation of this function in case we're on a v2 beatmap, to fix many old modcharts.
        // If we are on a v2 beatmap, we'll use our own implementation of CalculateHalfJumpDurationInBeats.
        // Otherwise, we'll use the built in CalculateHalfJumpDurationInBeats.
        private float CalculateHalfJumpDurationInBeats(
            bool? v2,
            float startHalfJumpDurationInBeats,
            float maxHalfJumpDistance,
            float noteJumpMovementSpeed,
            float oneBeatDuration,
            float noteJumpStartBeatOffset)
        {
            if (!v2.HasValue || !v2.Value)
            {
                return CoreMathUtils.CalculateHalfJumpDurationInBeats(
                    startHalfJumpDurationInBeats,
                    maxHalfJumpDistance,
                    noteJumpMovementSpeed,
                    oneBeatDuration,
                    noteJumpStartBeatOffset);
            }

            float num = startHalfJumpDurationInBeats;
            float num2 = noteJumpMovementSpeed * oneBeatDuration;
            float num3 = num2 * num;
            maxHalfJumpDistance -= 0.001f;
            while (num3 > maxHalfJumpDistance)
            {
                num /= 2f;
                num3 = num2 * num;
            }

            num += noteJumpStartBeatOffset;
            if (num < 1f)
            {
                num = 1f;
            }

            return num;
        }

        private float GetJumpDuration(
            bool? v2,
            float? inputNjs,
            float? inputOffset)
        {
            if (!inputNjs.HasValue && !inputOffset.HasValue && _initData.noteJumpValueType == BeatmapObjectSpawnMovementData.NoteJumpValueType.JumpDuration)
            {
                return _movementData.jumpDuration;
            }

            float oneBeatDuration = _initData.beatsPerMinute.OneBeatDuration();
            float halfJumpDurationInBeats = CalculateHalfJumpDurationInBeats(
                v2,
                _movementData._startHalfJumpDurationInBeats,
                _movementData._maxHalfJumpDistance,
                inputNjs ?? _movementData.noteJumpMovementSpeed,
                oneBeatDuration,
                inputOffset ?? _movementData._noteJumpStartBeatOffset);
            return oneBeatDuration * halfJumpDurationInBeats * 2f;
        }

        private void GetNoteJumpValues(
            bool? v2,
            float? inputNjs,
            float? inputOffset,
            out float jumpDuration,
            out float jumpDistance,
            out Vector3 moveStartPos,
            out Vector3 moveEndPos,
            out Vector3 jumpEndPos)
        {
            jumpDuration = GetJumpDuration(v2, inputNjs, inputOffset);

            Vector3 centerPos = _movementData.centerPos;
            Vector3 forwardVec = _movementData._forwardVec;

            jumpDistance = (inputNjs ?? _movementData.noteJumpMovementSpeed) * jumpDuration;
            moveEndPos = centerPos + (forwardVec * (jumpDistance * 0.5f));
            jumpEndPos = centerPos - (forwardVec * (jumpDistance * 0.5f));
            moveStartPos = centerPos + (forwardVec * (_movementData._moveDistance + (jumpDistance * 0.5f)));
        }
    }
}
