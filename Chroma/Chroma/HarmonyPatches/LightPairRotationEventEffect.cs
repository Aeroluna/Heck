namespace Chroma.HarmonyPatches
{
    using System;
    using System.Reflection;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using IPA.Utilities;
    using UnityEngine;

    [HarmonyPatch(typeof(LightPairRotationEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal class LightPairRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        internal static BeatmapEventData LastLightPairRotationEventEffectData { get; private set; }

        // Laser rotation
#pragma warning disable SA1313
        private static void Prefix(BeatmapEventData beatmapEventData, BeatmapEventType ____eventL, BeatmapEventType ____eventR)
#pragma warning restore SA1313
        {
            if (beatmapEventData.type == ____eventL || beatmapEventData.type == ____eventR)
            {
                LastLightPairRotationEventEffectData = beatmapEventData;
            }
        }

        private static void Postfix()
        {
            LastLightPairRotationEventEffectData = null;
        }
    }

    [HarmonyPatch(typeof(LightPairRotationEventEffect))]
    [HarmonyPatch("UpdateRotationData")]
    internal class LightPairRotationEventEffectUpdateRotationData
    {
        private static MethodInfo _getPrivateFieldM = null;
        private static Type _rotationDataType = null;

        private static void GetRotationData()
        {
            // Thank you +1 Rabbit for providing this code
            // Since LightPairRotationEventEffect.RotationData is a private internal member, we need to get its type dynamically.
            _rotationDataType = Type.GetType("LightPairRotationEventEffect+RotationData,Main");

            // The reflection method to get the rotation data must have its generic method created dynamically, so as to use the dynamic type.
            MethodInfo reflectionGetField = typeof(ReflectionUtil).GetMethod("GetField");
            _getPrivateFieldM = reflectionGetField.MakeGenericMethod(_rotationDataType, typeof(LightPairRotationEventEffect));
        }

#pragma warning disable SA1313
        private static bool Prefix(LightPairRotationEventEffect __instance, BeatmapEventType ____eventL, float startRotationOffset, float direction)
#pragma warning restore SA1313
        {
            if (!ChromaBehaviour.LightingRegistered)
            {
                return true;
            }

            BeatmapEventData beatmapEventData = LightPairRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.LastLightPairRotationEventEffectData;

            string rotationName = beatmapEventData.type == ____eventL ? "_rotationDataL" : "_rotationDataR";

            if (_getPrivateFieldM == null)
            {
                GetRotationData();
            }

            var rotationData = _getPrivateFieldM.Invoke(null, new object[] { __instance, rotationName });

            if (beatmapEventData is CustomBeatmapEventData customData && rotationData != null)
            {
                dynamic dynData = customData.customData;

                bool? lockPosition = Trees.at(dynData, "_lockPosition");
                lockPosition = lockPosition.GetValueOrDefault(false);

                float? precisionSpeed = (float?)Trees.at(dynData, "_preciseSpeed");
                precisionSpeed = precisionSpeed.GetValueOrDefault(beatmapEventData.value);

                int? dir = (int?)Trees.at(dynData, "_direction");
                dir = dir.GetValueOrDefault(-1);

                if (dir == 1)
                {
                    direction = beatmapEventData.type == ____eventL ? 1 : -1;
                }
                else if (dir == 0)
                {
                    direction = beatmapEventData.type == ____eventL ? -1 : 1;
                }

                // Actual lasering
                Transform transform = (Transform)_rotationDataType.GetField("transform").GetValue(rotationData);
                Quaternion startRotation = (Quaternion)_rotationDataType.GetField("startRotation").GetValue(rotationData);
                Vector3 rotationVector = __instance.GetField<Vector3, LightPairRotationEventEffect>("_rotationVector");
                if (beatmapEventData.value == 0)
                {
                    _rotationDataType.GetField("enabled").SetValue(rotationData, false);
                    if (!lockPosition.Value)
                    {
                        transform.localRotation = startRotation;
                    }
                }
                else
                {
                    _rotationDataType.GetField("enabled").SetValue(rotationData, true);
                    _rotationDataType.GetField("rotationSpeed").SetValue(rotationData, precisionSpeed * 20f * direction);
                    if (!lockPosition.Value)
                    {
                        transform.localRotation = startRotation;
                        transform.Rotate(rotationVector, startRotationOffset, Space.Self);
                    }
                }

                return false;
            }

            return true;
        }
    }
}
