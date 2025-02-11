using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using Heck.Animation;
using Heck.Deserialize;
using NoodleExtensions.Animation;
using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.Objects;

internal class ObstacleUpdateNoodlifier : IAffinity, IDisposable
{
    private static readonly FieldInfo _finishMovementTimeField = AccessTools.Field(
        typeof(ObstacleController),
        nameof(ObstacleController._finishMovementTime));

#if LATEST
    private static readonly FieldInfo _variableMovementDataProviderField = AccessTools.Field(
        typeof(ObstacleControllerBase),
        nameof(ObstacleControllerBase._variableMovementDataProvider));
#else
    private static readonly FieldInfo _move1DurationField = AccessTools.Field(
        typeof(ObstacleController),
        nameof(ObstacleController._move1Duration));
#endif

    private static readonly FieldInfo _obstacleDataField = AccessTools.Field(
        typeof(ObstacleController),
        nameof(ObstacleController._obstacleData));

    private readonly AnimationHelper _animationHelper;
    private readonly CutoutManager _cutoutManager;
    private readonly BeatmapCallbacksController _beatmapCallbacksController;
    private readonly DeserializedData _deserializedData;

    private readonly CodeInstruction _obstacleTimeAdjust;

    private ObstacleUpdateNoodlifier(
        [Inject(Id = ID)] DeserializedData deserializedData,
        AnimationHelper animationHelper,
        CutoutManager cutoutManager,
        BeatmapCallbacksController beatmapCallbacksController)
    {
        _deserializedData = deserializedData;
        _animationHelper = animationHelper;
        _cutoutManager = cutoutManager;
        _beatmapCallbacksController = beatmapCallbacksController;
        _obstacleTimeAdjust =
            InstanceTranspilers
                .EmitInstanceDelegate<ObstacleTimeAdjustDelegate>(ObstacleTimeAdjust);
    }

    private delegate float ObstacleTimeAdjustDelegate(
        float original,
        ObstacleData obstacleData,
#if LATEST
        IVariableMovementDataProvider variableMovementDataProvider,
#else
        float move1Duration,
#endif
        float finishMovementTime);

    public void Dispose()
    {
        InstanceTranspilers.DisposeDelegate(_obstacleTimeAdjust);
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.GetPosForTime))]
    private bool DefinitePosForTime(
        ref Vector3 __result,
        ObstacleData ____obstacleData,
        Vector3 ____startPos,
        Vector3 ____midPos,
#if LATEST
        IVariableMovementDataProvider ____variableMovementDataProvider,
#else
        float ____move1Duration,
        float ____move2Duration,
        float ____obstacleDuration,
#endif
        float time)
    {
        if (!_deserializedData.Resolve(____obstacleData, out NoodleObstacleData? noodleData))
        {
            return true;
        }

#if LATEST
        float moveDuration = ____variableMovementDataProvider.moveDuration;
        float jumpDuration = ____variableMovementDataProvider.jumpDuration;
        float obstacleDuration = ____obstacleData.duration;
#else
        float moveDuration = ____move1Duration;
        float jumpDuration = ____move2Duration;
        float obstacleDuration = ____obstacleDuration;
#endif

        float jumpTime = Mathf.Clamp((time - moveDuration) / (jumpDuration + obstacleDuration), 0, 1);
        _animationHelper.GetDefinitePositionOffset(
            noodleData.AnimationObject,
            noodleData.Track,
            jumpTime,
            out Vector3? position);

        if (!position.HasValue)
        {
            return true;
        }

        Vector3 noteOffset = noodleData.InternalNoteOffset;
        Vector3 definitePosition = position.Value + noteOffset;
        if (time < moveDuration)
        {
            __result = Vector3.LerpUnclamped(____startPos, ____midPos, time / moveDuration);
            __result += definitePosition - ____midPos;
        }
        else
        {
            __result = definitePosition;
        }

        return false;
    }

    private float ObstacleTimeAdjust(
        float original,
        ObstacleData obstacleData,
#if LATEST
        IVariableMovementDataProvider variableMovementDataProvider,
#else
        float move1Duration,
#endif
        float finishMovementTime)
    {
#if LATEST
        float move1Duration = variableMovementDataProvider.moveDuration;
#endif

        if (!(original > move1Duration) || !_deserializedData.Resolve(obstacleData, out NoodleObstacleData? noodleData))
        {
            return original;
        }

        float? time = noodleData.GetTimeProperty();
        if (time.HasValue)
        {
            return (time.Value * (finishMovementTime - move1Duration)) + move1Duration;
        }

        return original;
    }

#if LATEST
    [AffinityPostfix]
    [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.GetObstacleLength))]
    private void UseCustomLength(ObstacleData ____obstacleData, ref float __result)
    {
        if (_deserializedData.Resolve(____obstacleData, out NoodleObstacleData? noodleData) &&
            noodleData.Length != null)
        {
            __result = noodleData.Length.Value * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance;
        }
    }
#endif

    [AffinityPrefix]
    [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.ManualUpdate))]
    private void Prefix(
        ObstacleController __instance,
        ObstacleData ____obstacleData,
        float ____startTimeOffset,
#if LATEST
        IVariableMovementDataProvider ____variableMovementDataProvider,
        ref ObstacleSpawnData ____obstacleSpawnData,
#else
        ref Vector3 ____startPos,
        ref Vector3 ____midPos,
        ref Vector3 ____endPos,
        float ____move1Duration,
        float ____move2Duration,
        float ____obstacleDuration,
        ref Quaternion ____inverseWorldRotation,
#endif
        ref Quaternion ____worldRotation,
        ref Bounds ____bounds)
    {
        if (!_deserializedData.Resolve(____obstacleData, out NoodleObstacleData? noodleData))
        {
            return;
        }

        if (noodleData.InternalDoUnhide)
        {
            __instance.Hide(false);
        }

        IReadOnlyList<Track>? tracks = noodleData.Track;
        NoodleObjectData.AnimationObjectData? animationObject = noodleData.AnimationObject;
        if (tracks == null && animationObject == null)
        {
            return;
        }

#if LATEST
        float moveDuration = ____variableMovementDataProvider.moveDuration;
        float jumpDuration = ____variableMovementDataProvider.jumpDuration;
        float obstacleDuration = ____obstacleData.duration;
#else
        float moveDuration = ____move1Duration;
        float jumpDuration = ____move2Duration;
        float obstacleDuration = ____obstacleDuration;
#endif

        float? time = noodleData.GetTimeProperty();
        float normalTime;
        if (time.HasValue)
        {
            normalTime = time.Value;
        }
        else
        {
            float elapsedTime = _beatmapCallbacksController.songTime - ____startTimeOffset;
            normalTime = (elapsedTime - moveDuration) / (jumpDuration + obstacleDuration);
        }

        _animationHelper.GetObjectOffset(
            animationObject,
            tracks,
            normalTime,
            out Vector3? positionOffset,
            out Quaternion? rotationOffset,
            out Vector3? scaleOffset,
            out Quaternion? localRotationOffset,
            out float? dissolve,
            out _,
            out float? cuttable);

        if (positionOffset.HasValue)
        {
            Vector3 offset = positionOffset.Value;

#if LATEST
            Vector3 moveOffset = noodleData.InternalStartPos;
            ____obstacleSpawnData = new ObstacleSpawnData(
                moveOffset + offset,
                ____obstacleSpawnData.obstacleWidth,
                ____obstacleSpawnData.obstacleHeight);
#else
            Vector3 startPos = noodleData.InternalStartPos;
            Vector3 midPos = noodleData.InternalMidPos;
            Vector3 endPos = noodleData.InternalEndPos;
            ____startPos = startPos + offset;
            ____midPos = midPos + offset;
            ____endPos = endPos + offset;
#endif
        }

        Transform transform = __instance.transform;

        if (rotationOffset.HasValue || localRotationOffset.HasValue)
        {
            Quaternion worldRotation = noodleData.InternalWorldRotation;
            Quaternion localRotation = noodleData.InternalLocalRotation;

            Quaternion worldRotationQuatnerion = worldRotation;
            if (rotationOffset.HasValue)
            {
                worldRotationQuatnerion *= rotationOffset.Value;
                ____worldRotation = worldRotationQuatnerion;
#if !LATEST
                ____inverseWorldRotation = Quaternion.Inverse(worldRotationQuatnerion);
#endif
            }

            worldRotationQuatnerion *= localRotation;

            if (localRotationOffset.HasValue)
            {
                worldRotationQuatnerion *= localRotationOffset.Value;
            }

            transform.localRotation = worldRotationQuatnerion;
        }

        if (cuttable.HasValue)
        {
            if (cuttable.Value >= 1)
            {
                ____bounds.size = Vector3.zero;
            }
            else
            {
                Vector3 boundsSize = noodleData.InternalBoundsSize;
                ____bounds.size = boundsSize;
            }
        }

        if (scaleOffset.HasValue)
        {
            transform.localScale = Vector3.Scale(noodleData.InternalScale, scaleOffset.Value);
        }

        if (dissolve.HasValue)
        {
            _cutoutManager.ObstacleCutoutEffects[__instance].SetCutout(dissolve.Value);
        }
    }

    [AffinityTranspiler]
    [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.ManualUpdate))]
    private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            /*
             * -- float num = this._audioTimeSyncController.songTime - this._startTimeOffset;
             * ++ float num = ObstacletimeAdjust(this._audioTimeSyncController.songTime - this._startTimeOffset, this._obstacleData, this._move1Duration, this._finishMovementTime);
             */
            .MatchForward(false, new CodeMatch(OpCodes.Stloc_0))
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, _obstacleDataField),
                new CodeInstruction(OpCodes.Ldarg_0),
#if LATEST
                new CodeInstruction(OpCodes.Ldfld, _variableMovementDataProviderField),
#else
                new CodeInstruction(OpCodes.Ldfld, _move1DurationField),
#endif
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, _finishMovementTimeField),
                _obstacleTimeAdjust)
            .InstructionEnumeration();
    }
}
