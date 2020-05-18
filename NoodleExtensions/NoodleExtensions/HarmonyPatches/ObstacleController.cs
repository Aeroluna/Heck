using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(ObstacleController))]
    [NoodlePatch("Init")]
    internal class ObstacleControllerInit
    {
        private static void Postfix(ObstacleController __instance, ObstacleData obstacleData, Quaternion ____worldRotation, ref float ____passedAvoidedMarkTime, ref float ____finishMovementTime)
        {
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float> _localrot = ((List<object>)Trees.at(dynData, LOCALROTATION))?.Select(Convert.ToSingle);

                // oh my god im actually adding rotation
                Quaternion? localRotation = null;
                if (_localrot != null)
                {
                    Vector3 vector = new Vector3(_localrot.ElementAt(0), _localrot.ElementAt(1), _localrot.ElementAt(2));
                    localRotation = Quaternion.Euler(vector);
                    __instance.transform.Rotate(vector);
                }

                float? despawnTime = (float?)Trees.at(dynData, DESPAWNTIME);
                float? despawnDuration = (float?)Trees.at(dynData, DESPAWNDURATION);
                if (despawnTime.HasValue) ____passedAvoidedMarkTime = despawnTime.Value;
                if (despawnDuration.HasValue) ____finishMovementTime = ____passedAvoidedMarkTime + despawnDuration.Value;

                List<object> varPosition = Trees.at(dynData, VARIABLEPOSITION);
                if (varPosition != null)
                {
                    List<PositionData> positionData = new List<PositionData>();
                    foreach (object n in varPosition)
                    {
                        IDictionary<string, object> dictData = n as IDictionary<string, object>;

                        IEnumerable<float> startpos = ((List<object>)Trees.at(dictData, VARIABLESTARTPOS))?.Select(Convert.ToSingle);
                        IEnumerable<float> endpos = ((List<object>)Trees.at(dictData, VARIABLEENDPOS))?.Select(Convert.ToSingle);

                        float time = (float)Trees.at(dictData, VARIABLETIME);
                        float duration = (float)Trees.at(dictData, VARIABLEDURATION);
                        string easing = (string)Trees.at(dictData, VARIABLEEASING);
                        positionData.Add(new PositionData(time, duration, startpos, endpos, easing));
                    }
                    dynData.varPosition = positionData;
                }

                RotationData.savedRotation = ____worldRotation;

                List<object> varRotation = Trees.at(dynData, VARIABLEROTATION);
                if (varRotation != null)
                {
                    List<RotationData> rotationData = new List<RotationData>();
                    foreach (object n in varRotation)
                    {
                        IDictionary<string, object> dictData = n as IDictionary<string, object>;

                        IEnumerable<float> startrot = ((List<object>)Trees.at(dictData, VARIABLESTARTROT))?.Select(Convert.ToSingle);
                        IEnumerable<float> endrot = ((List<object>)Trees.at(dictData, VARIABLEENDROT))?.Select(Convert.ToSingle);

                        float time = (float)Trees.at(dictData, VARIABLETIME);
                        float duration = (float)Trees.at(dictData, VARIABLEDURATION);
                        string easing = (string)Trees.at(dictData, VARIABLEEASING);
                        rotationData.Add(new RotationData(time, duration, startrot, endrot, easing));
                    }
                    dynData.varRotation = rotationData;
                }

                RotationData.savedRotation = ____worldRotation * localRotation.GetValueOrDefault(Quaternion.identity);

                List<object> varLocalRotation = Trees.at(dynData, VARIABLELOCALROTATION);
                if (varLocalRotation != null)
                {
                    List<RotationData> rotationData = new List<RotationData>();
                    foreach (object n in varLocalRotation)
                    {
                        IDictionary<string, object> dictData = n as IDictionary<string, object>;

                        IEnumerable<float> startrot = ((List<object>)Trees.at(dictData, VARIABLESTARTROT))?.Select(Convert.ToSingle);
                        IEnumerable<float> endrot = ((List<object>)Trees.at(dictData, VARIABLEENDROT))?.Select(Convert.ToSingle);

                        float time = (float)Trees.at(dictData, VARIABLETIME);
                        float duration = (float)Trees.at(dictData, VARIABLEDURATION);
                        string easing = (string)Trees.at(dictData, VARIABLEEASING);
                        rotationData.Add(new RotationData(time, duration, startrot, endrot, easing));
                    }
                    dynData.varLocalRotation = rotationData;
                }
            }
        }

        private static readonly MethodInfo customWidth = SymbolExtensions.GetMethodInfo(() => GetCustomWidth(null, 0));
        private static readonly MethodInfo worldRotation = SymbolExtensions.GetMethodInfo(() => GetWorldRotation(null, 0));
        private static readonly MethodInfo inverseQuaternion = SymbolExtensions.GetMethodInfo(() => Quaternion.Inverse(Quaternion.identity));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundRotation = false;
            bool foundWidth = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundRotation &&
                    instructionList[i].opcode == OpCodes.Stfld &&
                    ((FieldInfo)instructionList[i].operand).Name == "_worldRotation")
                {
                    foundRotation = true;

                    instructionList[i - 1] = new CodeInstruction(OpCodes.Call, worldRotation);
                    instructionList[i - 4] = new CodeInstruction(OpCodes.Ldarg_1);
                    instructionList.RemoveAt(i - 2);

                    instructionList.RemoveRange(i + 1, 2);
                    instructionList[i + 1] = new CodeInstruction(OpCodes.Ldarg_0);
                    instructionList[i + 2] = new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ObstacleController), "_worldRotation"));
                    instructionList[i + 3] = new CodeInstruction(OpCodes.Call, inverseQuaternion);
                }
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

        private static Quaternion GetWorldRotation(ObstacleData obstacleData, float @default)
        {
            Quaternion _worldRotation = Quaternion.Euler(0, @default, 0);
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                dynamic _rotation = Trees.at(dynData, ROTATION);

                if (_rotation != null)
                {
                    if (_rotation is List<object> list)
                    {
                        IEnumerable<float> _rot = (list)?.Select(Convert.ToSingle);
                        _worldRotation = Quaternion.Euler(_rot.ElementAt(0), _rot.ElementAt(1), _rot.ElementAt(2));
                    }
                    else _worldRotation = Quaternion.Euler(0, (float)_rotation, 0);
                }
            }
            return _worldRotation;
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
        private static void Prefix(ref VectorState? __state, ref float time, ObstacleData ____obstacleData, ref Vector3 ____startPos, ref Vector3 ____midPos, ref Vector3 ____endPos)
        {
            if (____obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;

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
                            movementTime += pos.duration;
                            ____startPos += pos.endPosition;
                            ____midPos += pos.endPosition;
                            ____endPos += pos.endPosition;
                        }
                        else
                        {
                            movementTime += time - pos.time;
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

        private static void Postfix(ref Vector3 __result, VectorState? __state, float time, ObstacleController __instance, ObstacleData ____obstacleData, ref Quaternion ____worldRotation,
            ref Quaternion ____inverseWorldRotation, ref Vector3 ____startPos, ref Vector3 ____midPos, ref Vector3 ____endPos)
        {
            if (____obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;

                if (__state.HasValue)
                {
                    VectorState vectorState = __state.Value;
                    ____startPos = vectorState._startPos;
                    ____midPos = vectorState._midPos;
                    ____endPos = vectorState._endPos;
                    time = vectorState.time;
                    PositionData pos = vectorState.activePositionData;
                    if (pos != null)
                    {
                        __result += Vector3.Lerp(pos.startPosition, pos.endPosition,
                                Easings.Interpolate((time - pos.time) / pos.duration, pos.easing));
                    }
                }

                List<RotationData> rotationData = Trees.at(dynData, "varRotation");
                if (rotationData != null)
                {
                    RotationData truncatedRotation = rotationData
                        .Where(n => n.time < time)
                        .Where(n => n.time + n.duration > time)
                        .LastOrDefault();
                    if (truncatedRotation != null)
                    {
                        Quaternion rotation = Quaternion.Lerp(truncatedRotation.startRotation, truncatedRotation.endRotation,
                            Easings.Interpolate((time - truncatedRotation.time) / truncatedRotation.duration, truncatedRotation.easing));
                        ____worldRotation = rotation;
                        ____inverseWorldRotation = Quaternion.Inverse(rotation);
                    }
                }

                Quaternion? localRotation = null;
                List<RotationData> localRotationData = Trees.at(dynData, "varLocalRotation");
                if (localRotationData != null)
                {
                    RotationData truncatedRotation = localRotationData
                        .Where(n => n.time < time)
                        .Where(n => n.time + n.duration > time)
                        .LastOrDefault();
                    if (truncatedRotation != null)
                        localRotation = Quaternion.Lerp(truncatedRotation.startRotation, truncatedRotation.endRotation,
                            Easings.Interpolate((time - truncatedRotation.time) / truncatedRotation.duration, truncatedRotation.easing));
                }

                __instance.transform.localRotation = ____worldRotation * localRotation.GetValueOrDefault(Quaternion.identity);
            }
        }
    }
}