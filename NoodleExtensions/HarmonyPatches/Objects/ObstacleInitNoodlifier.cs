using System;
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
    private static readonly FieldInfo _inverseWorldRotationField =
        AccessTools.Field(typeof(ObstacleController), "_inverseWorldRotation");

    private static readonly MethodInfo _invertQuaternion = AccessTools.Method(
        typeof(Quaternion),
        nameof(Quaternion.Inverse));

    private static readonly FieldInfo _lengthField = AccessTools.Field(typeof(ObstacleController), "_length");

    private static readonly MethodInfo _widthGetter = AccessTools.PropertyGetter(
        typeof(ObstacleData),
        nameof(ObstacleData.width));

    private static readonly FieldInfo _worldRotationField = AccessTools.Field(
        typeof(ObstacleController),
        "_worldRotation");

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
            InstanceTranspilers.EmitInstanceDelegate<Func<ObstacleData, float, Quaternion>>(GetWorldRotation);
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
        return noodleData?.Width ?? @default;
    }

    private Quaternion GetWorldRotation(ObstacleData obstacleData, float @default)
    {
        Quaternion worldRotation = Quaternion.Euler(0, @default, 0);

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
        Vector3 ____startPos,
        Vector3 ____midPos,
        Vector3 ____endPos,
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

        if (noodleData is { Uninteractable: true })
        {
            ____bounds.size = Vector3.zero;
        }
        else
        {
            _obstacleTracker.AddActive(__instance);
        }

        noodleData.InternalStartPos = ____startPos;
        noodleData.InternalMidPos = ____midPos;
        noodleData.InternalEndPos = ____endPos;
        noodleData.InternalLocalRotation = localRotation;
        noodleData.InternalBoundsSize = ____bounds.size;

        Vector3 noteOffset = ____endPos;
        noteOffset.z = 0;
        noodleData.InternalNoteOffset = noteOffset;
    }

    [AffinityTranspiler]
    [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.Init))]
    private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)

            // world rotation
            /*
             * -- this._worldRotation = Quaternion.Euler(0f, worldRotation, 0f);
             * ++ this._worldRotation = GetWorldRotation(obstacleData, worldRotation);
             */
            .MatchForward(false, new CodeMatch(OpCodes.Stfld, _worldRotationField))
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2),
                _getWorldRotation)
            .RemoveInstructionsWithOffsets(-4, -1)

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
            .RemoveInstructionsWithOffsets(-5, -1)

            // width
            /*
             * -- this._width = (float)obstacleData.width * singleLineWidth;
             * ++ this._width = GetCustomWidth((float)obstacleData.width * singleLineWidth, obstacleData);
             */
            .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _widthGetter))
            .Advance(2)
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
