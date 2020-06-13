using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using IPA.Utilities;
using NoodleExtensions.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(ObstacleController))]
    [NoodlePatch("Init")]
    internal class ObstacleControllerInit
    {
        private static void Postfix(ObstacleController __instance, ObstacleData obstacleData, Vector3 startPos, Vector3 midPos, Vector3 endPos)
        {
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float> localrot = ((List<object>)Trees.at(dynData, LOCALROTATION))?.Select(n => Convert.ToSingle(n));
                Quaternion localRotation = _quaternionIdentity;
                if (localrot != null)
                {
                    localRotation = Quaternion.Euler(localrot.ElementAt(0), localrot.ElementAt(1), localrot.ElementAt(2));
                    __instance.transform.rotation *= localRotation;
                }

                dynData.startPos = startPos;
                dynData.midPos = midPos;
                dynData.endPos = endPos;
                dynData.localRotation = localRotation;
            }
        }

        private static readonly MethodInfo _getCustomWidth = SymbolExtensions.GetMethodInfo(() => GetCustomWidth(0, null));
        private static readonly MethodInfo _getWorldRotation = SymbolExtensions.GetMethodInfo(() => GetWorldRotation(null, 0));
        private static readonly MethodInfo _getCustomLength = SymbolExtensions.GetMethodInfo(() => GetCustomLength(0, null));
        private static readonly MethodInfo _invertQuaternion = SymbolExtensions.GetMethodInfo(() => Quaternion.Inverse(Quaternion.identity));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundRotation = false;
            bool foundWidth = false;
            bool foundLength = false;
            int instructrionListCount = instructionList.Count;
            for (int i = 0; i < instructrionListCount; i++)
            {
                if (!foundRotation &&
                       instructionList[i].opcode == OpCodes.Stfld &&
                       ((FieldInfo)instructionList[i].operand).Name == "_worldRotation")
                {
                    foundRotation = true;

                    instructionList[i - 1] = new CodeInstruction(OpCodes.Call, _getWorldRotation);
                    instructionList[i - 4] = new CodeInstruction(OpCodes.Ldarg_1);
                    instructionList.RemoveAt(i - 2);

                    instructionList.RemoveRange(i + 1, 2);
                    instructionList[i + 1] = new CodeInstruction(OpCodes.Ldarg_0);
                    instructionList[i + 2] = new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ObstacleController), "_worldRotation"));
                    instructionList[i + 3] = new CodeInstruction(OpCodes.Call, _invertQuaternion);
                }
                if (!foundWidth &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "get_width")
                {
                    foundWidth = true;
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_1));
                    instructionList.Insert(i + 3, new CodeInstruction(OpCodes.Call, _getCustomWidth));
                }
                if (!foundLength &&
                    instructionList[i].opcode == OpCodes.Stloc_2)
                {
                    foundLength = true;
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldarg_1));
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, _getCustomLength));
                }
            }
            if (!foundRotation) Logger.Log("Failed to find _worldRotation stfld, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            if (!foundWidth) Logger.Log("Failed to find get_width call, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            if (!foundLength) Logger.Log("Failed to find stloc.2, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }

        private static Quaternion GetWorldRotation(ObstacleData obstacleData, float @default)
        {
            Quaternion worldRotation = Quaternion.Euler(0, @default, 0);
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                dynamic rotation = Trees.at(dynData, ROTATION);

                if (rotation != null)
                {
                    if (rotation is List<object> list)
                    {
                        IEnumerable<float> _rot = list.Select(n => Convert.ToSingle(n));
                        worldRotation = Quaternion.Euler(_rot.ElementAt(0), _rot.ElementAt(1), _rot.ElementAt(2));
                    }
                    else
                    {
                        worldRotation = Quaternion.Euler(0, (float)rotation, 0);
                    }
                }
                dynData.worldRotation = worldRotation;
            }
            return worldRotation;
        }

        private static float GetCustomWidth(float @default, ObstacleData obstacleData)
        {
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float?> scale = ((List<object>)Trees.at(dynData, SCALE))?.Select(n => n.ToNullableFloat());
                float? width = scale?.ElementAtOrDefault(0);
                if (width.HasValue) return width.Value;
            }
            return @default;
        }

        private static float GetCustomLength(float @default, ObstacleData obstacleData)
        {
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float?> scale = ((List<object>)Trees.at(dynData, SCALE))?.Select(n => n.ToNullableFloat());
                float? length = scale?.ElementAtOrDefault(2);
                if (length.HasValue) return length.Value * _noteLinesDistance;
            }
            return @default;
        }
    }

    [NoodlePatch(typeof(ObstacleController))]
    [NoodlePatch("Update")]
    internal class ObstacleControllerUpdate
    {
        private static readonly FieldAccessor<ObstacleController, Vector3>.Accessor _startPosAccessor = FieldAccessor<ObstacleController, Vector3>.GetAccessor("_startPos");
        private static readonly FieldAccessor<ObstacleController, Vector3>.Accessor _midPosAccessor = FieldAccessor<ObstacleController, Vector3>.GetAccessor("_midPos");
        private static readonly FieldAccessor<ObstacleController, Vector3>.Accessor _endPosAccessor = FieldAccessor<ObstacleController, Vector3>.GetAccessor("_endPos");

        private static readonly FieldAccessor<ObstacleController, AudioTimeSyncController>.Accessor _audioTimeSyncControllerAccessor = FieldAccessor<ObstacleController, AudioTimeSyncController>.GetAccessor("_audioTimeSyncController");
        private static readonly FieldAccessor<ObstacleController, float>.Accessor _jumpDurationAccessor = FieldAccessor<ObstacleController, float>.GetAccessor("_move2Duration");
        private static readonly FieldAccessor<ObstacleController, float>.Accessor _startTimeOffsetAccessor = FieldAccessor<ObstacleController, float>.GetAccessor("_startTimeOffset");

        private static readonly FieldAccessor<ObstacleController, Quaternion>.Accessor _worldRotationAccessor = FieldAccessor<ObstacleController, Quaternion>.GetAccessor("_worldRotation");
        private static readonly FieldAccessor<ObstacleController, Quaternion>.Accessor _inverseWorldRotationAccessor = FieldAccessor<ObstacleController, Quaternion>.GetAccessor("_inverseWorldRotation");

        private static readonly FieldAccessor<ObstacleDissolve, CutoutAnimateEffect>.Accessor _obstacleCutoutAnimateEffectAccessor = FieldAccessor<ObstacleDissolve, CutoutAnimateEffect>.GetAccessor("_cutoutAnimateEffect");

        private static void Prefix(ObstacleController __instance, ObstacleData ____obstacleData)
        {
            if (____obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;

                Track track = Trees.at(dynData, "track");
                if (track != null)
                {

                    // idk i just copied base game time
                    float jumpDuration = _jumpDurationAccessor(ref __instance);
                    float elapsedTime = _audioTimeSyncControllerAccessor(ref __instance).songTime - _startTimeOffsetAccessor(ref __instance);
                    float normalTime = elapsedTime / jumpDuration;

                    dynamic animationObject = Trees.at(dynData, "_animation");
                    AnimationHelper.GetObjectOffset(animationObject, track, normalTime, out Vector3? positionOffset, out Quaternion? rotationOffset, out Vector3? scaleOffset, out Quaternion? localRotationOffset, out float? dissolve, out float? _);

                    if (positionOffset.HasValue)
                    {
                        Vector3 startPos = Trees.at(dynData, "startPos");
                        Vector3 midPos = Trees.at(dynData, "midPos");
                        Vector3 endPos = Trees.at(dynData, "endPos");

                        Vector3 offset = positionOffset.Value;
                        _startPosAccessor(ref __instance) = startPos + offset;
                        _midPosAccessor(ref __instance) = midPos + offset;
                        _endPosAccessor(ref __instance) = endPos + offset;
                    }

                    Transform transform = __instance.transform;

                    if (rotationOffset.HasValue || localRotationOffset.HasValue)
                    {
                        Quaternion worldRotation = Trees.at(dynData, "worldRotation");
                        Quaternion localRotation = Trees.at(dynData, "localRotation");

                        Quaternion worldRotationQuatnerion = worldRotation;
                        if (rotationOffset.HasValue)
                        {
                            worldRotationQuatnerion *= rotationOffset.Value;
                            Quaternion inverseWorldRotation = Quaternion.Inverse(worldRotationQuatnerion);
                            _worldRotationAccessor(ref __instance) = worldRotationQuatnerion;
                            _inverseWorldRotationAccessor(ref __instance) = inverseWorldRotation;
                        }

                        worldRotationQuatnerion *= localRotation;

                        if (localRotationOffset.HasValue) worldRotationQuatnerion *= localRotationOffset.Value;

                        transform.rotation = worldRotationQuatnerion;
                    }

                    if (scaleOffset.HasValue) transform.localScale = scaleOffset.Value;

                    if (dissolve.HasValue)
                    {
                        CutoutAnimateEffect cutoutAnimateEffect = Trees.at(dynData, "cutoutAnimateEffect");
                        if (cutoutAnimateEffect == null)
                        {
                            ObstacleDissolve obstacleDissolve = __instance.gameObject.GetComponent<ObstacleDissolve>();
                            cutoutAnimateEffect = _obstacleCutoutAnimateEffectAccessor(ref obstacleDissolve);
                            dynData.cutoutAnimateEffect = cutoutAnimateEffect;
                        }
                        cutoutAnimateEffect.SetCutout(dissolve.Value);
                    }
                }
            }
        }
    }
}