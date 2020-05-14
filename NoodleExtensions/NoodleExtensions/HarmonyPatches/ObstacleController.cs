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
        private static void Postfix(ObstacleController __instance, ObstacleData obstacleData, ref float ____passedAvoidedMarkTime, ref float ____finishMovementTime)
        {
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float> _localrot = ((List<object>)Trees.at(dynData, LOCALROTATION))?.Select(Convert.ToSingle);

                // oh my god im actually adding rotation
                if (_localrot != null)
                {
                    Vector3 vector = new Vector3(_localrot.ElementAt(0), _localrot.ElementAt(1), _localrot.ElementAt(2));
                    __instance.transform.Rotate(vector);
                }

                List<object> movement = Trees.at(dynData, "_movement");
                if (movement != null)
                {
                    List<MovementData> movementData = new List<MovementData>();
                    float dataTime = 0;
                    foreach (object n in movement)
                    {
                        IDictionary<string, object> dictData = n as IDictionary<string, object>;

                        float? doNothing = (float?)Trees.at(dictData, "_doNothingDuration");
                        if (doNothing.HasValue)
                        {
                            movementData.Add(new DoNothingData(dataTime, doNothing.Value));

                            dataTime += doNothing.Value;
                            continue;
                        }

                        float? despawnOffset = (float?)Trees.at(dictData, "_despawnOffset");
                        if (despawnOffset.HasValue)
                        {
                            ____passedAvoidedMarkTime = dataTime;
                            ____finishMovementTime = dataTime + despawnOffset.Value;
                            continue;
                        }

                        IEnumerable<float> startrot = ((List<object>)Trees.at(dictData, "_startRotation"))?.Select(Convert.ToSingle);
                        IEnumerable<float> endrot = ((List<object>)Trees.at(dictData, "_endRotation"))?.Select(Convert.ToSingle);
                        if (startrot != null || endrot != null)
                        {
                            float duration = (float)Trees.at(dictData, "_duration");
                            string easing = (string)Trees.at(dictData, "_easing");
                            movementData.Add(new RotationData(dataTime, duration, startrot, endrot, easing));

                            dataTime += duration;
                            continue;
                        }
                    }
                    dynData.movement = movementData;
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

    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("GetPosForTime")]
    internal class ObstacleControllerGetPosForTime
    {
        private static void Postfix(float time, ObstacleController __instance, ObstacleData ____obstacleData, ref Quaternion ____worldRotation,
            ref Quaternion ____inverseWorldRotation)
        {
            if (____obstacleData is CustomObstacleData customData) {
                dynamic dynData = customData.customData;
                List<MovementData> movement = Trees.at(dynData, "movement");
                if (movement == null) return;
                MovementData truncatedMovement = movement.Where(n => n.time < time).Last();
                if (truncatedMovement == null) return;
                if (truncatedMovement is RotationData rotationData)
                {
                    Quaternion rotation = Quaternion.Lerp(rotationData.startRotation, rotationData.endRotation, Easings.Interpolate((time - rotationData.time) / rotationData.duration, rotationData.easing));
                    ____worldRotation = rotation;
                    ____inverseWorldRotation = Quaternion.Inverse(rotation);
                    __instance.transform.localRotation = rotation;
                }
            }
        }
    }
}