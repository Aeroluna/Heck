namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using IPA.Utilities;
    using NoodleExtensions.Animation;
    using UnityEngine;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
    using static NoodleExtensions.Plugin;

    [NoodlePatch(typeof(ObstacleController))]
    [NoodlePatch("Init")]
    internal static class ObstacleControllerInit
    {
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

            if (!foundRotation)
            {
                NoodleLogger.Log("Failed to find _worldRotation stfld!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundWidth)
            {
                NoodleLogger.Log("Failed to find get_width call!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundLength)
            {
                NoodleLogger.Log("Failed to find stloc.2!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

#pragma warning disable SA1313
        private static void Postfix(ObstacleController __instance, Quaternion ____worldRotation, ObstacleData obstacleData, Vector3 ____startPos, Vector3 ____midPos, Vector3 ____endPos)
#pragma warning restore SA1313
        {
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float> localrot = ((List<object>)Trees.at(dynData, LOCALROTATION))?.Select(n => Convert.ToSingle(n));

                Transform transform = __instance.transform;

                Quaternion localRotation = _quaternionIdentity;
                if (localrot != null)
                {
                    localRotation = Quaternion.Euler(localrot.ElementAt(0), localrot.ElementAt(1), localrot.ElementAt(2));
                    transform.localRotation = ____worldRotation * localRotation;
                }

                transform.localScale = Vector3.one; // This is a fix for animation due to obstacles being recycled

                Track track = AnimationHelper.GetTrack(dynData);
                if (track != null && ParentObject.Controller != null)
                {
                    ParentObject parentObject = ParentObject.Controller.GetParentObjectTrack(track);
                    if (parentObject != null)
                    {
                        parentObject.ParentToObject(transform);
                    }
                    else
                    {
                        ParentObject.ResetTransformParent(transform);
                    }
                }
                else
                {
                    ParentObject.ResetTransformParent(transform);
                }

                dynData.startPos = ____startPos;
                dynData.midPos = ____midPos;
                dynData.endPos = ____endPos;
                dynData.localRotation = localRotation;
            }

            __instance.Update();
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
                        IEnumerable<float> rot = list.Select(n => Convert.ToSingle(n));
                        worldRotation = Quaternion.Euler(rot.ElementAt(0), rot.ElementAt(1), rot.ElementAt(2));
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
                if (width.HasValue)
                {
                    return width.Value;
                }
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
                if (length.HasValue)
                {
                    return length.Value * NoteLinesDistance;
                }
            }

            return @default;
        }
    }

    [NoodlePatch(typeof(ObstacleController))]
    [NoodlePatch("Update")]
    internal static class ObstacleControllerUpdate
    {
        private static readonly FieldAccessor<ObstacleDissolve, CutoutAnimateEffect>.Accessor _obstacleCutoutAnimateEffectAccessor = FieldAccessor<ObstacleDissolve, CutoutAnimateEffect>.GetAccessor("_cutoutAnimateEffect");

        private static readonly FieldInfo _obstacleDataField = AccessTools.Field(typeof(ObstacleController), "_obstacleData");
        private static readonly FieldInfo _move1DurationField = AccessTools.Field(typeof(ObstacleController), "_move1Duration");
        private static readonly FieldInfo _finishMovementTime = AccessTools.Field(typeof(ObstacleController), "_finishMovementTime");
        private static readonly MethodInfo _obstacleTimeAdjust = SymbolExtensions.GetMethodInfo(() => ObstacleTimeAdjust(0, null, 0, 0));
        private static readonly MethodInfo _setLocalPosition = typeof(Transform).GetProperty("localPosition").GetSetMethod();

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundTime = false;
            bool foundPosition = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundTime &&
                    instructionList[i].opcode == OpCodes.Stloc_0)
                {
                    foundTime = true;
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Ldfld, _obstacleDataField));
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 3, new CodeInstruction(OpCodes.Ldfld, _move1DurationField));
                    instructionList.Insert(i + 4, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 5, new CodeInstruction(OpCodes.Ldfld, _finishMovementTime));
                    instructionList.Insert(i + 6, new CodeInstruction(OpCodes.Call, _obstacleTimeAdjust));
                }

                if (!foundPosition &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "set_position")
                {
                    foundPosition = true;
                    instructionList[i].operand = _setLocalPosition;
                }
            }

            if (!foundTime)
            {
                NoodleLogger.Log("Failed to find stloc.0!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundPosition)
            {
                NoodleLogger.Log("Failed to find callvirt to set_position!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static float ObstacleTimeAdjust(float original, ObstacleData obstacleData, float move1Duration, float finishMovementTime)
        {
            if (original > move1Duration)
            {
                dynamic dynData = ((CustomObstacleData)obstacleData).customData;
                Track track = Trees.at(dynData, "track");
                float? time = AnimationHelper.TryGetProperty(track, TIME);
                if (time.HasValue)
                {
                    return (Mathf.Clamp(time.Value, 0, 1) * (finishMovementTime - move1Duration)) + move1Duration;
                }
            }

            return original;
        }

#pragma warning disable SA1313
        private static void Prefix(
            ObstacleController __instance,
            ObstacleData ____obstacleData,
            AudioTimeSyncController ____audioTimeSyncController,
            float ____startTimeOffset,
            ref Vector3 ____startPos,
            ref Vector3 ____midPos,
            ref Vector3 ____endPos,
            float ____move1Duration,
            float ____move2Duration,
            float ____obstacleDuration,
            ref Quaternion ____worldRotation,
            ref Quaternion ____inverseWorldRotation)
#pragma warning restore SA1313
        {
            if (____obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;

                Track track = Trees.at(dynData, "track");
                dynamic animationObject = Trees.at(dynData, "_animation");
                if (track != null || animationObject != null)
                {
                    // idk i just copied base game time
                    float elapsedTime = ____audioTimeSyncController.songTime - ____startTimeOffset;
                    float normalTime = (elapsedTime - ____move1Duration) / (____move2Duration + ____obstacleDuration);

                    AnimationHelper.GetObjectOffset(animationObject, track, normalTime, out Vector3? positionOffset, out Quaternion? rotationOffset, out Vector3? scaleOffset, out Quaternion? localRotationOffset, out float? dissolve, out float? _, out float? _);

                    if (positionOffset.HasValue)
                    {
                        Vector3 startPos = Trees.at(dynData, "startPos");
                        Vector3 midPos = Trees.at(dynData, "midPos");
                        Vector3 endPos = Trees.at(dynData, "endPos");

                        Vector3 offset = positionOffset.Value;
                        ____startPos = startPos + offset;
                        ____midPos = midPos + offset;
                        ____endPos = endPos + offset;
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
                            ____worldRotation = worldRotationQuatnerion;
                            ____inverseWorldRotation = inverseWorldRotation;
                        }

                        worldRotationQuatnerion *= localRotation;

                        if (localRotationOffset.HasValue)
                        {
                            worldRotationQuatnerion *= localRotationOffset.Value;
                        }

                        transform.localRotation = worldRotationQuatnerion;
                    }

                    if (scaleOffset.HasValue)
                    {
                        transform.localScale = scaleOffset.Value;
                    }

                    if (dissolve.HasValue)
                    {
                        CutoutAnimateEffect cutoutAnimateEffect = Trees.at(dynData, "cutoutAnimateEffect");
                        if (cutoutAnimateEffect == null)
                        {
                            ObstacleDissolve obstacleDissolve = __instance.gameObject.GetComponent<ObstacleDissolve>();
                            cutoutAnimateEffect = _obstacleCutoutAnimateEffectAccessor(ref obstacleDissolve);
                            dynData.cutoutAnimateEffect = cutoutAnimateEffect;
                        }

                        cutoutAnimateEffect.SetCutout(1 - dissolve.Value);
                    }
                }
            }
        }
    }

    [NoodlePatch(typeof(ObstacleController))]
    [NoodlePatch("GetPosForTime")]
    internal static class ObstacleControllerGetPosForTime
    {
#pragma warning disable SA1313
        private static bool Prefix(
            ref Vector3 __result,
            ObstacleData ____obstacleData,
            Vector3 ____startPos,
            Vector3 ____midPos,
            float ____move1Duration,
            float ____move2Duration,
            float ____obstacleDuration,
            float time)
#pragma warning restore SA1313
        {
            if (____obstacleData is CustomObstacleData customObstacleData)
            {
                dynamic dynData = customObstacleData.customData;
                dynamic animationObject = Trees.at(dynData, "_animation");
                Track track = Trees.at(dynData, "track");

                float jumpTime = Mathf.Clamp((time - ____move1Duration) / (____move2Duration + ____obstacleDuration), 0, 1);
                AnimationHelper.GetDefinitePositionOffset(animationObject, track, jumpTime, out Vector3? position);

                if (position.HasValue)
                {
                    Vector3 noteOffset = Trees.at(dynData, "noteOffset");
                    Vector3 definitePosition = position.Value + noteOffset;
                    definitePosition.x += Trees.at(dynData, "xOffset");
                    if (time < ____move1Duration)
                    {
                        __result = Vector3.LerpUnclamped(____startPos, ____midPos, time / ____move1Duration);
                        __result += definitePosition - ____midPos;
                    }
                    else
                    {
                        __result = definitePosition;
                    }

                    return false;
                }
            }

            return true;
        }
    }
}
