using BS_Utils.Utilities;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using IPA.Utilities;
using System;
using System.Reflection;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(LightPairRotationEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal class LightPairRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        internal static BeatmapEventData lastLightPairRotationEventEffectData;

        //Laser rotation
        private static void Prefix(BeatmapEventData beatmapEventData, BeatmapEventType ____eventL, BeatmapEventType ____eventR)
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

    [HarmonyPatch(typeof(LightPairRotationEventEffect))]
    [HarmonyPatch("UpdateRotationData")]
    internal class LightPairRotationEventEffectUpdateRotationData
    {
        private static MethodInfo GetPrivateFieldM = null;
        private static Type RotationData = null;

        private static void GetRotationData()
        {
            // Thank you +1 Rabbit for providing this code
            // Since LightPairRotationEventEffect.RotationData is a private internal member, we need to get its type dynamically.
            RotationData = Type.GetType("LightPairRotationEventEffect+RotationData,Main");
            // The reflection method to get the rotation data must have its generic method created dynamically, so as to use the dynamic type.
            GetPrivateFieldM = typeof(BS_Utils.Utilities.ReflectionUtil).GetMethod("GetPrivateField", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(string) }, null);
            GetPrivateFieldM = GetPrivateFieldM.MakeGenericMethod(RotationData);
        }

        private static bool Prefix(LightPairRotationEventEffect __instance, BeatmapEventType ____eventL, float startRotationOffset, float direction)
        {
            if (!ChromaBehaviour.LightingRegistered) return true;

            BeatmapEventData beatmapEventData = LightPairRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.lastLightPairRotationEventEffectData;

            string rotationName = beatmapEventData.type == ____eventL ? "_rotationDataL" : "_rotationDataR";

            if (GetPrivateFieldM == null) GetRotationData();

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
                    Transform _transform = _rotationData.GetField<Transform>("transform", RotationData);
                    Quaternion _startRotation = _rotationData.GetField<Quaternion>("startRotation", RotationData);
                    Vector3 _rotationVector = __instance.GetField<Vector3, LightPairRotationEventEffect>("_rotationVector");
                    if (beatmapEventData.value == 0)
                    {
                        if (!lockPosition.Value)
                        {
                            _transform.localRotation = _startRotation;
                        }
                    }
                    else
                    {
                        if (!lockPosition.Value)
                        {
                            _transform.localRotation = _startRotation;
                            _transform.Rotate(_rotationVector, startRotationOffset, Space.Self);
                        }
                    }

                    if (precisionSpeed == 0)
                    {
                        _rotationData.SetField("enabled", false, RotationData);
                    }
                    else
                    {
                        _rotationData.SetField("enabled", true, RotationData);
                        _rotationData.SetField("rotationSpeed", precisionSpeed * 20f * direction, RotationData);
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.Log("INVALID _customData", Logger.Level.WARNING);
                Logger.Log(e);
            }

            return true;
        }
    }
}