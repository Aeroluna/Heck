namespace Chroma.HarmonyPatches
{
    using System;
    using IPA.Utilities;
    using UnityEngine;
    using static ChromaEventDataManager;

    [ChromaPatch(typeof(LightPairRotationEventEffect))]
    [ChromaPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class LightPairRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        internal static BeatmapEventData LastLightPairRotationEventEffectData { get; private set; }

        // Laser rotation
        private static void Prefix(BeatmapEventData beatmapEventData, BeatmapEventType ____eventL, BeatmapEventType ____eventR)
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

    [ChromaPatch(typeof(LightPairRotationEventEffect))]
    [ChromaPatch("UpdateRotationData")]
    internal static class LightPairRotationEventEffectUpdateRotationData
    {
        private static Type _rotationDataType = null;

        private static void GetRotationData()
        {
            // Since LightPairRotationEventEffect.RotationData is a private internal member, we need to get its type dynamically.
            _rotationDataType = Type.GetType("LightPairRotationEventEffect+RotationData,Main");
        }

        private static bool Prefix(LightPairRotationEventEffect __instance, BeatmapEventType ____eventL, float startRotationOffset, float direction, object ____rotationDataL, object ____rotationDataR)
        {
            BeatmapEventData beatmapEventData = LightPairRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.LastLightPairRotationEventEffectData;

            bool isLeftEvent = beatmapEventData.type == ____eventL;

            if (_rotationDataType == null)
            {
                GetRotationData();
            }

            object rotationData = isLeftEvent ? ____rotationDataL : ____rotationDataR;

            ChromaLaserSpeedEventData chromaData = (ChromaLaserSpeedEventData)ChromaEventDatas[beatmapEventData];

            bool lockPosition = chromaData.LockPosition;
            float precisionSpeed = chromaData.PreciseSpeed;
            int? dir = chromaData.Direction;

            switch (dir)
            {
                case 0:
                    direction = isLeftEvent ? -1 : 1;
                    break;

                case 1:
                    direction = isLeftEvent ? 1 : -1;
                    break;
            }

            // Actual lasering
            Transform transform = (Transform)_rotationDataType.GetField("transform").GetValue(rotationData);
            Quaternion startRotation = (Quaternion)_rotationDataType.GetField("startRotation").GetValue(rotationData);
            float startRotationAngle = (float)_rotationDataType.GetField("startRotationAngle").GetValue(rotationData);
            Vector3 rotationVector = __instance.GetField<Vector3, LightPairRotationEventEffect>("_rotationVector");
            if (beatmapEventData.value == 0)
            {
                _rotationDataType.GetField("enabled").SetValue(rotationData, false);
                if (!lockPosition)
                {
                    _rotationDataType.GetField("rotationAngle").SetValue(rotationData, startRotationAngle);
                    transform.localRotation = startRotation * Quaternion.Euler(rotationVector * startRotationAngle);
                }
            }
            else if (beatmapEventData.value > 0)
            {
                _rotationDataType.GetField("enabled").SetValue(rotationData, true);
                _rotationDataType.GetField("rotationSpeed").SetValue(rotationData, precisionSpeed * 20f * direction);
                if (!lockPosition)
                {
                    float rotationAngle = startRotationOffset + startRotationAngle;
                    _rotationDataType.GetField("rotationAngle").SetValue(rotationData, rotationAngle);
                    transform.localRotation = startRotation * Quaternion.Euler(rotationVector * rotationAngle);
                }
            }

            return false;
        }
    }
}
