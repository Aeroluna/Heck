namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using IPA.Utilities;
    using NoodleExtensions.Animation;
    using UnityEngine;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
    using static NoodleExtensions.NoodleObjectDataManager;
    using static NoodleExtensions.Plugin;

    [NoodlePatch(typeof(ObstacleController))]
    [NoodlePatch("Init")]
    internal static class ObstacleControllerInit
    {
        internal static readonly List<ObstacleController> _activeObstacles = new List<ObstacleController>();

        private static readonly MethodInfo _getCustomWidth = SymbolExtensions.GetMethodInfo(() => GetCustomWidth(0, null));
        private static readonly MethodInfo _getWorldRotation = SymbolExtensions.GetMethodInfo(() => GetWorldRotation(null, 0));
        private static readonly MethodInfo _getCustomLength = SymbolExtensions.GetMethodInfo(() => GetCustomLength(0, null));
        private static readonly MethodInfo _invertQuaternion = SymbolExtensions.GetMethodInfo(() => InvertQuaternion(Quaternion.identity));

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

        private static void Postfix(ObstacleController __instance, Quaternion ____worldRotation, ObstacleData obstacleData, Vector3 ____startPos, Vector3 ____midPos, Vector3 ____endPos, ref Bounds ____bounds)
        {
            if (__instance is MultiplayerConnectedPlayerObstacleController)
            {
                return;
            }

            NoodleObstacleData noodleData = (NoodleObstacleData)NoodleObjectDatas[obstacleData];

            Quaternion? localRotationQuaternion = noodleData.LocalRotationQuaternion;

            Transform transform = __instance.transform;

            Quaternion localRotation = _quaternionIdentity;
            if (localRotationQuaternion.HasValue)
            {
                localRotation = localRotationQuaternion.Value;
                transform.localRotation = ____worldRotation * localRotation;
            }

            transform.localScale = Vector3.one; // This is a fix for animation due to obstacles being recycled

            Track track = noodleData.Track;
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

            bool? cuttable = noodleData.Cuttable;
            if (cuttable.HasValue && !cuttable.Value)
            {
                ____bounds.size = _vectorZero;
            }
            else
            {
                _activeObstacles.Add(__instance);
            }

            noodleData.StartPos = ____startPos;
            noodleData.MidPos = ____midPos;
            noodleData.EndPos = ____endPos;
            noodleData.LocalRotation = localRotation;
            noodleData.BoundsSize = ____bounds.size;
        }

        private static Quaternion InvertQuaternion(Quaternion quaternion)
        {
            return Quaternion.Euler(-quaternion.eulerAngles);
        }

        private static Quaternion GetWorldRotation(ObstacleData obstacleData, float @default)
        {
            Quaternion worldRotation = Quaternion.Euler(0, @default, 0);

            if (NoodleObjectDatas.TryGetValue(obstacleData, out NoodleObjectData noodleObjectData))
            {
                NoodleObstacleData noodleData = (NoodleObstacleData)noodleObjectData;

                Quaternion? worldRotationQuaternion = noodleData.WorldRotationQuaternion;
                if (worldRotationQuaternion.HasValue)
                {
                    worldRotation = worldRotationQuaternion.Value;
                }

                noodleData.WorldRotation = worldRotation;
            }

            return worldRotation;
        }

        private static float GetCustomWidth(float @default, ObstacleData obstacleData)
        {
            if (NoodleObjectDatas.TryGetValue(obstacleData, out NoodleObjectData noodleObjectData))
            {
                NoodleObstacleData noodleData = (NoodleObstacleData)noodleObjectData;
                float? width = noodleData.Width;
                if (width.HasValue)
                {
                    return width.Value;
                }
            }

            return @default;
        }

        private static float GetCustomLength(float @default, ObstacleData obstacleData)
        {
            if (NoodleObjectDatas.TryGetValue(obstacleData, out NoodleObjectData noodleObjectData))
            {
                NoodleObstacleData noodleData = (NoodleObstacleData)noodleObjectData;
                float? length = noodleData.Length;
                if (length.HasValue)
                {
                    return length.Value * NoteLinesDistance;
                }
            }

            return @default;
        }
    }

    [NoodlePatch(typeof(ObstacleController))]
    [NoodlePatch("ManualUpdate")]
    internal static class ObstacleControllerManualUpdate
    {
        private static readonly FieldAccessor<ObstacleDissolve, CutoutAnimateEffect>.Accessor _obstacleCutoutAnimateEffectAccessor = FieldAccessor<ObstacleDissolve, CutoutAnimateEffect>.GetAccessor("_cutoutAnimateEffect");

        private static readonly FieldInfo _obstacleDataField = AccessTools.Field(typeof(ObstacleController), "_obstacleData");
        private static readonly FieldInfo _move1DurationField = AccessTools.Field(typeof(ObstacleController), "_move1Duration");
        private static readonly FieldInfo _finishMovementTime = AccessTools.Field(typeof(ObstacleController), "_finishMovementTime");
        private static readonly MethodInfo _obstacleTimeAdjust = SymbolExtensions.GetMethodInfo(() => ObstacleTimeAdjust(0, null, 0, 0));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundTime = false;
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
            }

            if (!foundTime)
            {
                NoodleLogger.Log("Failed to find stloc.0!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static float ObstacleTimeAdjust(float original, ObstacleData obstacleData, float move1Duration, float finishMovementTime)
        {
            if (original > move1Duration && NoodleObjectDatas.TryGetValue(obstacleData, out NoodleObjectData noodleData))
            {
                float? time = (float?)AnimationHelper.TryGetPropertyAsObject(noodleData.Track, TIME);
                if (time.HasValue)
                {
                    return (time.Value * (finishMovementTime - move1Duration)) + move1Duration;
                }
            }

            return original;
        }

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
            ref Quaternion ____inverseWorldRotation,
            ref Bounds ____bounds)
        {
            if (__instance is MultiplayerConnectedPlayerObstacleController)
            {
                return;
            }

            NoodleObstacleData noodleData = (NoodleObstacleData)NoodleObjectDatas[____obstacleData];

            Track track = noodleData.Track;
            NoodleObjectData.AnimationObjectData animationObject = noodleData.AnimationObject;
            if (track != null || animationObject != null)
            {
                // idk i just copied base game time
                float elapsedTime = ____audioTimeSyncController.songTime - ____startTimeOffset;
                float normalTime = (elapsedTime - ____move1Duration) / (____move2Duration + ____obstacleDuration);

                AnimationHelper.GetObjectOffset(animationObject, track, normalTime, out Vector3? positionOffset, out Quaternion? rotationOffset, out Vector3? scaleOffset, out Quaternion? localRotationOffset, out float? dissolve, out float? _, out float? cuttable);

                if (positionOffset.HasValue)
                {
                    Vector3 startPos = noodleData.StartPos;
                    Vector3 midPos = noodleData.MidPos;
                    Vector3 endPos = noodleData.EndPos;

                    Vector3 offset = positionOffset.Value;
                    ____startPos = startPos + offset;
                    ____midPos = midPos + offset;
                    ____endPos = endPos + offset;
                }

                Transform transform = __instance.transform;

                if (rotationOffset.HasValue || localRotationOffset.HasValue)
                {
                    Quaternion worldRotation = noodleData.WorldRotation;
                    Quaternion localRotation = noodleData.LocalRotation;

                    Quaternion worldRotationQuatnerion = worldRotation;
                    if (rotationOffset.HasValue)
                    {
                        worldRotationQuatnerion *= rotationOffset.Value;
                        Quaternion inverseWorldRotation = Quaternion.Euler(-worldRotationQuatnerion.eulerAngles);
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

                bool cuttableEnabled = true;
                if (cuttable.HasValue)
                {
                    cuttableEnabled = cuttable.Value >= 1;
                    if (cuttableEnabled)
                    {
                        if (____bounds.size != _vectorZero)
                        {
                            ____bounds.size = _vectorZero;
                        }
                    }
                    else
                    {
                        Vector3 boundsSize = noodleData.BoundsSize;
                        if (____bounds.size != boundsSize)
                        {
                            ____bounds.size = boundsSize;
                        }
                    }
                }

                if (scaleOffset.HasValue)
                {
                    transform.localScale = scaleOffset.Value;
                }

                if (dissolve.HasValue)
                {
                    CutoutAnimateEffect cutoutAnimateEffect = noodleData.CutoutAnimateEffect;
                    if (cutoutAnimateEffect == null)
                    {
                        ObstacleDissolve obstacleDissolve = __instance.gameObject.GetComponent<ObstacleDissolve>();
                        cutoutAnimateEffect = _obstacleCutoutAnimateEffectAccessor(ref obstacleDissolve);
                        noodleData.CutoutAnimateEffect = cutoutAnimateEffect;
                    }

                    cutoutAnimateEffect.SetCutout(1 - dissolve.Value);
                }
            }
        }
    }

    [NoodlePatch(typeof(ObstacleController))]
    [NoodlePatch("GetPosForTime")]
    internal static class ObstacleControllerGetPosForTime
    {
        private static bool Prefix(
            ref Vector3 __result,
            ObstacleData ____obstacleData,
            Vector3 ____startPos,
            Vector3 ____midPos,
            float ____move1Duration,
            float ____move2Duration,
            float ____obstacleDuration,
            float time)
        {
            if (!NoodleObjectDatas.TryGetValue(____obstacleData, out NoodleObjectData noodleObjectData))
            {
                return true;
            }

            NoodleObstacleData noodleData = (NoodleObstacleData)noodleObjectData;

            float jumpTime = Mathf.Clamp((time - ____move1Duration) / (____move2Duration + ____obstacleDuration), 0, 1);
            AnimationHelper.GetDefinitePositionOffset(noodleData.AnimationObject, noodleData.Track, jumpTime, out Vector3? position);

            if (position.HasValue)
            {
                Vector3 noteOffset = noodleData.NoteOffset;
                Vector3 definitePosition = position.Value + noteOffset;
                definitePosition.x += noodleData.XOffset;
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

            return true;
        }
    }
}
