using Heck.Deserialize;
using JetBrains.Annotations;
using NoodleExtensions.HarmonyPatches.SmallFixes;
using UnityEngine;
using Zenject;
#if LATEST
using _NoteSpawnData = NoteSpawnData;
using _ObstacleSpawnData = ObstacleSpawnData;
using _SliderSpawnData = SliderSpawnData;
#else
using _NoteSpawnData = BeatmapObjectSpawnMovementData.NoteSpawnData;
using _ObstacleSpawnData = BeatmapObjectSpawnMovementData.ObstacleSpawnData;
using _SliderSpawnData = BeatmapObjectSpawnMovementData.SliderSpawnData;
#endif

namespace NoodleExtensions.Managers;

internal class SpawnDataManager
{
    private readonly DeserializedData _deserializedData;
    private readonly BeatmapObjectSpawnController.InitData _initData;
    private readonly BeatmapObjectSpawnMovementData _movementData;

#if LATEST
    private readonly VariableMovementDataProvider _variableMovementDataProvider;
#endif

    [UsedImplicitly]
    private SpawnDataManager(
        InitializedSpawnMovementData initializedSpawnMovementData,
        BeatmapObjectSpawnController.InitData initData,
#if LATEST
        VariableMovementDataProvider variableMovementDataProvider,
#endif
        [Inject(Id = NoodleController.ID)] DeserializedData deserializedData)
    {
        _movementData = initializedSpawnMovementData.MovementData;
        _initData = initData;
        _deserializedData = deserializedData;

#if LATEST
        _variableMovementDataProvider = variableMovementDataProvider;
#endif
    }

    internal static Vector2 Get2DNoteOffset(float lineIndex, int noteLinesCount, float lineLayer)
    {
        float distance = -(noteLinesCount - 1f) * 0.5f;
        return new Vector2(
            (distance + lineIndex) * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance,
            LineYPosForLineLayer(lineLayer));
    }

    internal float GetSpawnAheadTime(float? inputNjs, float? inputOffset)
    {
#if LATEST
        const float moveDuration = VariableMovementDataProvider.kMoveDuration;
#else
        float moveDuration = _movementData.moveDuration;
#endif
        return moveDuration + (GetJumpDuration(inputNjs, inputOffset) * 0.5f);
    }

    internal bool GetJumpingNoteSpawnData(NoteData noteData, ref _NoteSpawnData result)
    {
        if (!_deserializedData.Resolve(noteData, out NoodleBaseNoteData? noodleData))
        {
            return true;
        }

        bool gravityOverride = noodleData.DisableGravity;

        float offset = _movementData.noteLinesCount / 2f;
        float? flipLineIndex = noodleData.InternalFlipLineIndex;
        float lineIndex = noodleData.StartX + offset ?? noteData.lineIndex;
        float lineLayer = noodleData.StartY ?? (float)noteData.noteLineLayer;
        float startLineLayer = noodleData.InternalStartNoteLineLayer;

        Vector3 noteOffset = GetNoteOffset(lineIndex, startLineLayer);
#if LATEST
        Vector3 noteOffset2 = (noteData.colorType != ColorType.None)
            ? GetNoteOffset(
                flipLineIndex ?? lineIndex,
                gravityOverride ? lineLayer : startLineLayer)
            : noteOffset;

        float gravity = GetGravityBase(lineLayer, gravityOverride ? lineLayer : startLineLayer);

        result = new NoteSpawnData(
            noteOffset2,
            noteOffset2,
            noteOffset,
            gravity);
#else
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
#endif

        return false;
    }

    internal bool GetObstacleSpawnData(
        ObstacleData obstacleData,
        ref _ObstacleSpawnData result)
    {
        if (!_deserializedData.Resolve(obstacleData, out NoodleObstacleData? noodleData))
        {
            return true;
        }

        float lineIndex = noodleData.StartX + (_movementData.noteLinesCount / 2) ?? obstacleData.lineIndex;
        float lineLayer = noodleData.StartY ?? (float)obstacleData.lineLayer;

        Vector3 obstacleOffset = GetObstacleOffset(lineIndex, lineLayer);
        obstacleOffset.y += _movementData._jumpOffsetYProvider.jumpOffsetY;

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

#if LATEST
        float width = noodleData.Width ?? obstacleData.width;
        width *= StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance;
        obstacleOffset.x += (width - StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance) * 0.5f;
        result = new ObstacleSpawnData(
            obstacleOffset,
            width,
            obstacleHeight);
#else
        float? njs = noodleData.Njs;
        float? spawnoffset = noodleData.SpawnOffset;

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
#endif

        return false;
    }

    internal bool GetSliderSpawnData(SliderData sliderData, ref _SliderSpawnData result)
    {
        if (!_deserializedData.Resolve(sliderData, out NoodleSliderData? noodleData))
        {
            return true;
        }

        bool gravityOverride = noodleData.DisableGravity;

        float offset = _movementData.noteLinesCount / 2f;
        float headLineIndex = noodleData.StartX + offset ?? sliderData.headLineIndex;
        float headLineLayer = noodleData.StartY ?? (float)sliderData.headLineLayer;
        float headStartLineLayer = noodleData.InternalStartNoteLineLayer;
        float tailLineIndex = noodleData.TailStartX + offset ?? sliderData.tailLineIndex;
        float tailLineLayer = noodleData.TailStartY ?? (float)sliderData.tailLineLayer;
        float tailStartLineLayer = noodleData.InternalTailStartNoteLineLayer;

        Vector3 headOffset = GetNoteOffset(headLineIndex, gravityOverride ? headLineLayer : headStartLineLayer);
        Vector3 tailOffset = GetNoteOffset(tailLineIndex, gravityOverride ? tailLineLayer : tailStartLineLayer);

#if LATEST
        float headGravity = GetGravityBase(headLineLayer, gravityOverride ? headLineLayer : headStartLineLayer);
        float tailGravity = GetGravityBase(tailLineLayer, gravityOverride ? tailLineLayer : tailStartLineLayer);

        result = new SliderSpawnData(
            headOffset,
            headGravity,
            tailOffset,
            tailGravity);
#else
        float? njs = noodleData.Njs;
        float? spawnoffset = noodleData.SpawnOffset;

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
            headStartLineLayer,
            jumpDistance,
            njs,
            out float headJumpGravity,
            out float headNoGravity);

        NoteJumpGravityForLineLayer(
            tailLineLayer,
            tailStartLineLayer,
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
#endif

        return false;
    }

    private static float LineYPosForLineLayer(float height)
    {
        return StaticBeatmapObjectSpawnMovementData.kBaseLinesYPos +
               (height * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance); // offset by 0.25
    }

    private Vector3 GetNoteOffset(float lineIndex, float lineLayer)
    {
        Vector2 coords = Get2DNoteOffset(lineIndex, _movementData.noteLinesCount, lineLayer);
        return (_movementData._rightVec * coords.x) + new Vector3(0, coords.y, 0);
    }

    private Vector3 GetObstacleOffset(float lineIndex, float lineLayer)
    {
        Vector3 result = GetNoteOffset(lineIndex, lineLayer);
        result.y += StaticBeatmapObjectSpawnMovementData.kObstacleVerticalOffset;
        return result;
    }

    private float HighestJumpPosYForLineLayer(float lineLayer)
    {
        // Magic numbers below found with linear regression y=mx+b using existing HighestJumpPosYForLineLayer values
        return (0.875f * lineLayer) + 0.639583f + _movementData._jumpOffsetYProvider.jumpOffsetY;
    }

    private float GetJumpDuration(
        float? inputNjs,
        float? inputOffset)
    {
#if LATEST
        if (!inputNjs.HasValue &&
            !inputOffset.HasValue)
        {
            return _variableMovementDataProvider.jumpDuration;
        }

        float njs = inputNjs ?? _variableMovementDataProvider.noteJumpSpeed;
        float spawnOffset = inputOffset ?? _initData.noteJumpValue;
        BeatmapObjectSpawnMovementData.NoteJumpValueType noteJumpValueType = _initData.noteJumpValueType;
        if (noteJumpValueType == BeatmapObjectSpawnMovementData.NoteJumpValueType.JumpDuration)
        {
            return spawnOffset * 2f;
        }

        float oneBeatDuration = _initData.beatsPerMinute.OneBeatDuration();
        float halfJumpDurationInBeats = CoreMathUtils.CalculateHalfJumpDurationInBeats(
            _movementData._startHalfJumpDurationInBeats,
            _movementData._maxHalfJumpDistance,
            njs,
            oneBeatDuration,
            spawnOffset);

        return oneBeatDuration * halfJumpDurationInBeats * 2;
#else
        if (!inputNjs.HasValue &&
            !inputOffset.HasValue &&
            _initData.noteJumpValueType == BeatmapObjectSpawnMovementData.NoteJumpValueType.JumpDuration)
        {
            return _movementData.jumpDuration;
        }

        float oneBeatDuration = _initData.beatsPerMinute.OneBeatDuration();
        float halfJumpDurationInBeats = CoreMathUtils.CalculateHalfJumpDurationInBeats(
            _movementData._startHalfJumpDurationInBeats,
            _movementData._maxHalfJumpDistance,
            inputNjs ?? _movementData.noteJumpMovementSpeed,
            oneBeatDuration,
            inputOffset ?? _movementData._noteJumpStartBeatOffset);
        return oneBeatDuration * halfJumpDurationInBeats * 2;
#endif
    }

#if LATEST
    private float GetGravityBase(float noteLineLayer, float beforeJumpLineLayer)
    {
        return HighestJumpPosYForLineLayer(noteLineLayer) - LineYPosForLineLayer(beforeJumpLineLayer);
    }
#else
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
        Vector3 forwardVec = _movementData._forwardVec;

        jumpDistance = (inputNjs ?? _movementData.noteJumpMovementSpeed) * jumpDuration;
        moveEndPos = centerPos + (forwardVec * (jumpDistance * 0.5f));
        jumpEndPos = centerPos - (forwardVec * (jumpDistance * 0.5f));
        moveStartPos = centerPos + (forwardVec * (_movementData._moveDistance + (jumpDistance * 0.5f)));
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

        float highestJump = HighestJumpPosYForLineLayer(lineYPos);

        // NoteJumpGravityForLineLayer
        float num = (jumpDistance / (njs ?? _movementData.noteJumpMovementSpeed)) * 0.5f;
        num = 2 / (num * num);
        gravity = GetJumpGravity(startLayerLineYPos);
        noGravity = GetJumpGravity(lineYPos);
        return;

        float GetJumpGravity(float gravityLineYPos)
        {
            return (highestJump - gravityLineYPos) * num;
        }
    }
#endif
}
