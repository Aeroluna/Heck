#if LATEST
using Heck.Deserialize;
using JetBrains.Annotations;
using NoodleExtensions.HarmonyPatches.SmallFixes;
using UnityEngine;
using Zenject;

namespace NoodleExtensions.Managers;

// doesnt actually properly support the njs changing event but idc
internal class NoodleMovementDataProvider : IVariableMovementDataProvider
{
    private readonly IVariableMovementDataProvider _original;
    private readonly DeserializedData _deserializedData;
    private readonly BeatmapObjectSpawnMovementData _movementData;
    private readonly float _noteJumpStartBeatOffset;
    private readonly float _oneBeatDuration;
    private readonly BeatmapObjectSpawnMovementData.NoteJumpValueType _noteJumpValueType;

    private float? _jumpDistanceOverride;
    private float? _jumpDurationOverride;
    private float? _halfJumpDurationOverride;
    private float? _spawnAheadTimeOverride;
    private float? _noteJumpSpeedOverride;
    private Vector3? _moveStartPositionOverride;
    private Vector3? _moveEndPositionOverride;
    private Vector3? _jumpEndPositionOverride;

    [UsedImplicitly]
    private NoodleMovementDataProvider(
        IVariableMovementDataProvider original,
        BeatmapObjectSpawnController.InitData initData,
        InitializedSpawnMovementData initializedSpawnMovementData,
        [Inject(Id = NoodleController.ID)] DeserializedData deserializedData)
    {
        _original = original;
        _deserializedData = deserializedData;
        _movementData = initializedSpawnMovementData.MovementData;
        _noteJumpStartBeatOffset = initData.noteJumpValue;
        _oneBeatDuration = initData.beatsPerMinute.OneBeatDuration();
        _noteJumpValueType = initData.noteJumpValueType;
    }

    public bool wasUpdatedThisFrame => _original.wasUpdatedThisFrame;

    public float jumpDistance => _jumpDistanceOverride ?? _original.jumpDistance;

    public float jumpDuration => _jumpDurationOverride ?? _original.jumpDuration;

    public float halfJumpDuration => _halfJumpDurationOverride ?? _original.halfJumpDuration;

    public float moveDuration => _original.moveDuration;

    public float spawnAheadTime => _spawnAheadTimeOverride ?? _original.spawnAheadTime;

    public float waitingDuration => 0;

    public float noteJumpSpeed => _noteJumpSpeedOverride ?? _original.noteJumpSpeed;

    public Vector3 moveStartPosition => _moveStartPositionOverride ?? _original.moveStartPosition;

    public Vector3 moveEndPosition => _moveEndPositionOverride ?? _original.moveEndPosition;

    public Vector3 jumpEndPosition => _jumpEndPositionOverride ?? _original.jumpEndPosition;

    public void Init(
        float startHalfJumpDurationInBeats,
        float maxHalfJumpDistance,
        float noteJumpMovementSpeed,
        float minRelativeNoteJumpSpeed,
        float bpm,
        BeatmapObjectSpawnMovementData.NoteJumpValueType noteJumpValueType,
        float noteJumpValue,
        Vector3 centerPosition,
        Vector3 forwardVector)
    {
        throw new System.NotImplementedException();
    }

    public float CalculateCurrentNoteJumpGravity(float gravityBase)
    {
        float halfJumpDur = halfJumpDuration;
        return 2f * gravityBase / (halfJumpDur * halfJumpDur);
    }

    public float JumpPosYForLineLayerAtDistanceFromPlayerWithoutJumpOffset(float highestJumpPosY, float distanceFromPlayer)
    {
        float num = ((jumpDistance * 0.5f) - distanceFromPlayer) / noteJumpSpeed;
        float num2 = NoteJumpGravityForLineLayerWithoutJumpOffset(highestJumpPosY, 0);
        float num3 = num2 * jumpDuration * 0.5f;
        return LineYPosForLineLayer(0) + (num3 * num) - (num2 * num * num * 0.5f);
    }

    internal void InitObject(BeatmapObjectData beatmapObjectData)
    {
        _jumpDistanceOverride = null;
        _jumpDurationOverride = null;
        _halfJumpDurationOverride = null;
        _spawnAheadTimeOverride = null;
        _noteJumpSpeedOverride = null;
        _moveStartPositionOverride = null;
        _moveEndPositionOverride = null;
        _jumpEndPositionOverride = null;

        if (!_deserializedData.Resolve(beatmapObjectData, out NoodleObjectData? noodleData))
        {
            return;
        }

        if (noodleData.Njs == null &&
            noodleData.SpawnOffset == null)
        {
            return;
        }

        if (noodleData.Njs != null)
        {
            _noteJumpSpeedOverride = noodleData.Njs;
        }

        float njs = _noteJumpSpeedOverride ?? _original.noteJumpSpeed;
        float spawnOffset = noodleData.SpawnOffset ?? _noteJumpStartBeatOffset;
        switch (_noteJumpValueType)
        {
            case BeatmapObjectSpawnMovementData.NoteJumpValueType.JumpDuration:
                _jumpDurationOverride = spawnOffset * 2f;
                _halfJumpDurationOverride = spawnOffset;
                break;

            case BeatmapObjectSpawnMovementData.NoteJumpValueType.BeatOffset:
            {
                float halfJumpDurationInBeats = CoreMathUtils.CalculateHalfJumpDurationInBeats(
                    _movementData._startHalfJumpDurationInBeats,
                    _movementData._maxHalfJumpDistance,
                    njs,
                    _oneBeatDuration,
                    spawnOffset);

                float halfJump = _oneBeatDuration * halfJumpDurationInBeats;
                _halfJumpDurationOverride = halfJump;
                _jumpDurationOverride = halfJump * 2;
                break;
            }
        }

        _spawnAheadTimeOverride = VariableMovementDataProvider.kMoveDuration + halfJumpDuration;

        float jumpDist = njs * jumpDuration;
        _jumpDistanceOverride = jumpDist;
        float halfJumpDistance = jumpDist * 0.5f;
        Vector3 center = _movementData.centerPos;
        Vector3 forward = Vector3.forward;

        // kInitMoveDistance is no longer multiplied by moveDuration in 1.40,
        // but that breaks a bunch of maps so let's do it anyway
        float moveDistance = VariableMovementDataProvider.kInitMoveDistance * moveDuration;
        _moveStartPositionOverride =
            center + (forward * (moveDistance + halfJumpDistance));
        _moveEndPositionOverride = center + (forward * halfJumpDistance);
        _jumpEndPositionOverride = center - (forward * halfJumpDistance);
    }

    private static float LineYPosForLineLayer(float height)
    {
        return StaticBeatmapObjectSpawnMovementData.kBaseLinesYPos +
               (height * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance); // offset by 0.25
    }

    private float NoteJumpGravityForLineLayerWithoutJumpOffset(float highestJumpPosY, float beforeJumpLineLayer)
    {
        float num = jumpDistance / noteJumpSpeed * 0.5f;
        return 2f * (highestJumpPosY - LineYPosForLineLayer(beforeJumpLineLayer)) / (num * num);
    }

    [UsedImplicitly]
    internal class Pool : MemoryPool<BeatmapObjectData, NoodleMovementDataProvider>
    {
        protected override void Reinitialize(
            BeatmapObjectData beatmapObjectData,
            NoodleMovementDataProvider noodleMovementDataProvider)
        {
            noodleMovementDataProvider.InitObject(beatmapObjectData);
        }
    }
}
#endif
