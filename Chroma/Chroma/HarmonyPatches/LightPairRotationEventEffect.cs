using BS_Utils.Utilities;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(LightPairRotationEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal class LightPairRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        internal static BeatmapEventData lastLightPairRotationEventEffectData;

        //Laser rotation
        private static void Prefix(ref BeatmapEventData beatmapEventData, ref BeatmapEventType ____eventL, ref BeatmapEventType ____eventR)
        {
            if (beatmapEventData.type == ____eventL || beatmapEventData.type == ____eventR)
            {
                lastLightPairRotationEventEffectData = beatmapEventData;
            }
        }

        private static void Postfix()
        {
            lastLightPairRotationEventEffectData = null;
        }
    }

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(LightPairRotationEventEffect))]
    [HarmonyPatch("UpdateRotationData")]
    internal class LightPairRotationEventEffectUpdateRotationData
    {
        //Laser rotation
        private static bool Prefix(LightPairRotationEventEffect __instance, ref BeatmapEventType ____eventL, float startRotationOffset, float direction)
        {
            if (!ChromaBehaviour.LightingRegistered) return true;

            BeatmapEventData beatmapEventData = LightPairRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.lastLightPairRotationEventEffectData;

            // Thank you +1 Rabbit for providing this code
            // Since LightPairRotationEventEffect.RotationData is a private internal member, we need to get its type dynamically.
            Type RotationData = Type.GetType("LightPairRotationEventEffect+RotationData,Main");
            // The reflection method to get the rotation data must have its generic method created dynamically, so as to use the dynamic type.
            MethodInfo GetPrivateFieldM = typeof(ReflectionUtil).GetMethod("GetPrivateField", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(string) }, null);
            GetPrivateFieldM = GetPrivateFieldM.MakeGenericMethod(RotationData);

            string rotationName = beatmapEventData.type == ____eventL ? "_rotationDataL" : "_rotationDataR";

            var _rotationData = GetPrivateFieldM.Invoke(null, new object[] { __instance, rotationName });

            try
            {
                if (beatmapEventData is CustomBeatmapEventData customData && _rotationData != null)
                {
                    dynamic dynData = customData.customData;

                    bool? lockPosition = Trees.at(dynData, "_lockPosition");
                    lockPosition = lockPosition.GetValueOrDefault(false);

                    float? precisionSpeed = (float?)Trees.at(dynData, "_preciseSpeed");
                    precisionSpeed = precisionSpeed.GetValueOrDefault(beatmapEventData.value);

                    int? dir = (int?)Trees.at(dynData, "_direction");
                    dir = dir.GetValueOrDefault(-1);

                    if (dir == 1) direction = beatmapEventData.type == ____eventL ? 1 : -1;
                    else if (dir == 0) direction = beatmapEventData.type == ____eventL ? -1 : 1;

                    //Actual lasering
                    Transform _transform = _rotationData.GetField<Transform>("transform");
                    Quaternion _startRotation = _rotationData.GetField<Quaternion>("startRotation");
                    Vector3 _rotationVector = __instance.GetPrivateField<Vector3>("_rotationVector");
                    if (precisionSpeed == 0)
                    {
                        _rotationData.SetPrivateField("enabled", false);
                        if (!lockPosition.Value)
                        {
                            _transform.localRotation = _startRotation;
                        }
                    }
                    else
                    {
                        _rotationData.SetPrivateField("enabled", true);
                        _rotationData.SetPrivateField("rotationSpeed", precisionSpeed * 20f * direction);
                        if (!lockPosition.Value)
                        {
                            _transform.localRotation = _startRotation;
                            _transform.Rotate(_rotationVector, startRotationOffset, Space.Self);
                        }
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            return true;
        }
    }
}