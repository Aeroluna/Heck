using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
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
        private static void Postfix(ObstacleController __instance, ObstacleData obstacleData, ref float ____passedAvoidedMarkTime, ref float ____finishMovementTime)
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

        private static readonly MethodInfo customWidth = SymbolExtensions.GetMethodInfo(() => GetCustomWidth(null, 0));
        private static readonly MethodInfo inverseQuaternion = SymbolExtensions.GetMethodInfo(() => Quaternion.Inverse(Quaternion.identity));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundRotation = false;
            bool foundWidth = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundWidth &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "get_width")
                {
                    foundWidth = true;
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Call, customWidth));
                    instructionList.Insert(i - 1, new CodeInstruction(OpCodes.Ldarg_1));
                }
            }
            if (!foundRotation) Logger.Log("Failed to find _worldRotation stfld, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            if (!foundWidth) Logger.Log("Failed to find get_width call, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }

        private static float GetCustomWidth(ObstacleData obstacleData, float @default)
        {
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float?> _scale = ((List<object>)Trees.at(dynData, SCALE))?.Select(n => n.ToNullableFloat());
                float? _width = _scale?.ElementAtOrDefault(0);
                if (_width.HasValue) return _width.Value;
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
                    Vector3 _startPos = ____startPos;
                    Vector3 _midPos = ____midPos;
                    Vector3 _endPos = ____endPos;

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

                    __state = new VectorState(_startPos, _midPos, _endPos, timeCopy, activePositionData);
                    time -= movementTime;
                }
            }
        }

        private struct VectorState
        {
            internal Vector3 _startPos { get; }
            internal Vector3 _midPos { get; }
            internal Vector3 _endPos { get; }
            internal float time { get; }
            internal PositionData activePositionData { get; }

            internal VectorState(Vector3 _startPos, Vector3 _midPos, Vector3 _endPos, float time, PositionData activePositionData)
            {
                this._startPos = _startPos;
                this._midPos = _midPos;
                this._endPos = _endPos;
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
                ____startPos = vectorState._startPos;
                ____midPos = vectorState._midPos;
                ____endPos = vectorState._endPos;
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