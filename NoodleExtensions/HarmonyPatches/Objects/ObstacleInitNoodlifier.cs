﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using Heck.Deserialize;
using NoodleExtensions.HarmonyPatches.ObjectProcessing;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.Objects;

internal class ObstacleInitNoodlifier : IAffinity, IDisposable
{
#if LATEST
    private static readonly FieldInfo _widthField = AccessTools.Field(
        typeof(ObstacleController),
        nameof(ObstacleController._width));
#else
    private static readonly MethodInfo _widthGetter = AccessTools.PropertyGetter(
        typeof(ObstacleData),
        nameof(ObstacleData.width));

    private static readonly FieldInfo _inverseWorldRotationField =
        AccessTools.Field(
            typeof(ObstacleController),
            nameof(ObstacleController._inverseWorldRotation));

    private static readonly MethodInfo _invertQuaternion = AccessTools.Method(
        typeof(Quaternion),
        nameof(Quaternion.Inverse));
#endif

    private static readonly FieldInfo _lengthField = AccessTools.Field(
        typeof(ObstacleController),
        nameof(ObstacleController._length));

    private static readonly FieldInfo _worldRotationField = AccessTools.Field(
        typeof(ObstacleController),
        nameof(ObstacleController._worldRotation));

    private readonly DeserializedData _deserializedData;
    private readonly CodeInstruction _getCustomLength;
    private readonly CodeInstruction _getCustomWidth;

    private readonly CodeInstruction _getWorldRotation;
    private readonly ManagedActiveObstacleTracker _obstacleTracker;

    private ObstacleInitNoodlifier(
        [Inject(Id = NoodleController.ID)] DeserializedData deserializedData,
        ManagedActiveObstacleTracker obstacleTracker)
    {
        _deserializedData = deserializedData;
        _obstacleTracker = obstacleTracker;

        _getWorldRotation =
            InstanceTranspilers.EmitInstanceDelegate<Func<Quaternion, ObstacleData, Quaternion>>(GetWorldRotation);
        _getCustomWidth = InstanceTranspilers.EmitInstanceDelegate<Func<float, ObstacleData, float>>(GetCustomWidth);
        _getCustomLength = InstanceTranspilers.EmitInstanceDelegate<Func<float, ObstacleData, float>>(GetCustomLength);
    }

    public void Dispose()
    {
        InstanceTranspilers.DisposeDelegate(_getWorldRotation);
        InstanceTranspilers.DisposeDelegate(_getCustomWidth);
        InstanceTranspilers.DisposeDelegate(_getCustomLength);
    }

    private float GetCustomLength(float @default, ObstacleData obstacleData)
    {
        _deserializedData.Resolve(obstacleData, out NoodleObstacleData? noodleData);
        return noodleData?.Length * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance ?? @default;
    }

    private float GetCustomWidth(float @default, ObstacleData obstacleData)
    {
        _deserializedData.Resolve(obstacleData, out NoodleObstacleData? noodleData);
#if LATEST
        float? width = noodleData?.Width * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance;
#else
        float? width = noodleData?.Width;
#endif
        return width ?? @default;
    }

    private Quaternion GetWorldRotation(
        Quaternion worldRotation,
        ObstacleData obstacleData)
    {
        if (!_deserializedData.Resolve(obstacleData, out NoodleObstacleData? noodleData))
        {
            return worldRotation;
        }

        Quaternion? worldRotationQuaternion = noodleData.WorldRotationQuaternion;
        if (worldRotationQuaternion.HasValue)
        {
            worldRotation = worldRotationQuaternion.Value;
        }

        noodleData.InternalWorldRotation = worldRotation;

        return worldRotation;
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.Init))]
    private void Postfix(
        ObstacleController __instance,
        Quaternion ____worldRotation,
        ObstacleData obstacleData,
#if LATEST
        IVariableMovementDataProvider ____variableMovementDataProvider,
        ObstacleSpawnData obstacleSpawnData,
#else
        Vector3 ____startPos,
        Vector3 ____midPos,
        Vector3 ____endPos,
#endif
        ref Bounds ____bounds)
    {
        if (!_deserializedData.Resolve(obstacleData, out NoodleObstacleData? noodleData))
        {
            return;
        }

        Quaternion? localRotationQuaternion = noodleData.LocalRotationQuaternion;

        Transform transform = __instance.transform;

        Quaternion localRotation = Quaternion.identity;
        if (localRotationQuaternion.HasValue)
        {
            localRotation = localRotationQuaternion.Value;
            transform.localRotation = ____worldRotation * localRotation;
        }

        Vector3 scale = (noodleData.ScaleX != null || noodleData.ScaleY != null || noodleData.ScaleZ != null)
            ? new Vector3(noodleData.ScaleX ?? 1, noodleData.ScaleY ?? 1, noodleData.ScaleZ ?? 1)
            : Vector3.one;
        transform.localScale = scale;
        noodleData.InternalScale = scale;

        if (noodleData is { Uninteractable: true })
        {
            ____bounds.size = Vector3.zero;
        }
        else
        {
            _obstacleTracker.AddActive(__instance);
        }

        noodleData.InternalLocalRotation = localRotation;
        noodleData.InternalBoundsSize = ____bounds.size;

#if LATEST
        noodleData.InternalStartPos = obstacleSpawnData.moveOffset;
        Vector3 noteOffset = ____variableMovementDataProvider.jumpEndPosition + obstacleSpawnData.moveOffset;
        noteOffset.z = 0;
        noodleData.InternalNoteOffset = noteOffset;
#else
        noodleData.InternalStartPos = ____startPos;
        noodleData.InternalMidPos = ____midPos;
        noodleData.InternalEndPos = ____endPos;
        Vector3 noteOffset = ____endPos;
        noteOffset.z = 0;
        noodleData.InternalNoteOffset = noteOffset;
#endif
    }

    [AffinityTranspiler]
    [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.Init))]
    private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)

            // world rotation
            /*
             * -- this._worldRotation = Quaternion.Euler(0f, worldRotation, 0f);
             * ++ this._worldRotation = GetWorldRotation(Quaternion.Euler(0f, worldRotation, 0f), obstacleData);
             */
            .MatchForward(false, new CodeMatch(OpCodes.Stfld, _worldRotationField))
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_1),
                _getWorldRotation)

#if !LATEST
            // inverse world rotation
            /*
             * -- this._inverseWorldRotation = Quaternion.Euler(0f, -worldRotation, 0f);
             * ++ this._inverseWorldRotation = Quaternion.Inverse(this._worldRotation);
             */
            .MatchForward(false, new CodeMatch(OpCodes.Stfld, _inverseWorldRotationField))
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, _worldRotationField),
                new CodeInstruction(OpCodes.Call, _invertQuaternion))
#if V1_39_1
            .RemoveInstructionsWithOffsets(-7, -1)
#else
            .RemoveInstructionsWithOffsets(-5, -1)
#endif
#endif

            // width
            /*
             * -- this._width = (float)obstacleData.width * singleLineWidth;
             * ++ this._width = GetCustomWidth((float)obstacleData.width * singleLineWidth, obstacleData);
             */
#if LATEST
            .MatchForward(false, new CodeMatch(OpCodes.Stfld, _widthField))
#else
            .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _widthGetter))
            .Advance(2)
#endif
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_1),
                _getCustomWidth)

            // length
            /*
             * this._length = num * obstacleData.duration;
             * ++ this._length = GetCustomLength(num * obstacleData.duration, obstacleData);
             */
            .MatchForward(false, new CodeMatch(OpCodes.Stfld, _lengthField))
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_1),
                _getCustomLength)
            .InstructionEnumeration();
    }
}
