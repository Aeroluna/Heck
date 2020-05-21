using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static NoodleExtensions.HarmonyPatches.ObjectAnimationHelper;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(ObstacleController))]
    [NoodlePatch("Init")]
    internal class ObstacleControllerInit
    {
        private static void Postfix(ObstacleData obstacleData, ref float ____passedAvoidedMarkTime, ref float ____finishMovementTime)
        {
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;

                float? despawnTime = (float?)Trees.at(dynData, DESPAWNTIME);
                float? despawnDuration = (float?)Trees.at(dynData, DESPAWNDURATION);
                if (despawnTime.HasValue) ____passedAvoidedMarkTime = despawnTime.Value;
                if (despawnDuration.HasValue) ____finishMovementTime = ____passedAvoidedMarkTime + despawnDuration.Value;
            }
        }

        private static readonly MethodInfo _getCustomWidth = SymbolExtensions.GetMethodInfo(() => GetCustomWidth(0, null));
        private static readonly MethodInfo _getCustomLength = SymbolExtensions.GetMethodInfo(() => GetCustomLength(0, null));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundWidth = false;
            bool foundLength = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
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
            if (!foundWidth) Logger.Log("Failed to find get_width call, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            if (!foundLength) Logger.Log("Failed to find stloc.2, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
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
    [NoodlePatch("GetPosForTime")]
    internal class ObstacleControllerGetPosForTime
    {
        private static void Prefix(ObstacleController __instance, ref VectorState? __state, ref float time, ObstacleData ____obstacleData,
            ref Vector3 ____startPos, ref Vector3 ____midPos, ref Vector3 ____endPos, ref Quaternion ____worldRotation, ref Quaternion ____inverseWorldRotation)
        {
            if (____obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;

                Quaternion? rotation = GetWorldRotation(dynData, time);
                if (rotation.HasValue)
                {
                    ____worldRotation = rotation.Value;
                    ____inverseWorldRotation = Quaternion.Inverse(rotation.Value);
                }

                Quaternion? localRotation = GetLocalRotation(dynData, time);

                __instance.transform.localRotation = ____worldRotation * localRotation.GetValueOrDefault(Quaternion.identity);

                // fucky wucky
                List<PositionData> positionData = Trees.at(dynData, "varPosition");
                if (positionData != null)
                {
                    Vector3 startPos = ____startPos;
                    Vector3 midPos = ____midPos;
                    Vector3 endPos = ____endPos;

                    float timeCopy = time;
                    IEnumerable<PositionData> truncatedPosition = positionData
                        .Where(n => n.time < timeCopy);

                    float movementTime = 0;
                    PositionData activePositionData = null;
                    foreach (PositionData pos in truncatedPosition)
                    {
                        if (pos.time + pos.duration < time)
                        {
                            if (!pos.relative) movementTime += pos.duration;
                            ____startPos += (pos.endPosition * _noteLinesDistance);
                            ____midPos += (pos.endPosition * _noteLinesDistance);
                            ____endPos += (pos.endPosition * _noteLinesDistance);
                        }
                        else
                        {
                            if (!pos.relative) movementTime += time - pos.time;
                            activePositionData = pos;
                        }
                    }

                    __state = new VectorState(startPos, midPos, endPos, timeCopy, activePositionData);
                    time -= movementTime;
                }
            }
        }

        private struct VectorState
        {
            internal Vector3 startPos { get; }
            internal Vector3 midPos { get; }
            internal Vector3 endPos { get; }
            internal float time { get; }
            internal PositionData activePositionData { get; }

            internal VectorState(Vector3 startPos, Vector3 midPos, Vector3 endPos, float time, PositionData activePositionData)
            {
                this.startPos = startPos;
                this.midPos = midPos;
                this.endPos = endPos;
                this.time = time;
                this.activePositionData = activePositionData;
            }
        }

        private static void Postfix(ref Vector3 __result, VectorState? __state, ref Vector3 ____startPos, ref Vector3 ____midPos, ref Vector3 ____endPos)
        {
            // variable position kinda wacky
            if (__state.HasValue)
            {
                VectorState vectorState = __state.Value;
                ____startPos = vectorState.startPos;
                ____midPos = vectorState.midPos;
                ____endPos = vectorState.endPos;
                PositionData pos = vectorState.activePositionData;
                if (pos != null)
                {
                    __result += Vector3.Lerp(pos.startPosition, pos.endPosition,
                            Easings.Interpolate((vectorState.time - pos.time) / pos.duration, pos.easing)) * _noteLinesDistance;
                }
            }
        }
    }
}