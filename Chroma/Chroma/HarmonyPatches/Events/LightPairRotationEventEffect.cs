namespace Chroma.HarmonyPatches
{
    using Heck;
    using UnityEngine;
    using static Chroma.ChromaCustomDataManager;

    [HeckPatch(typeof(LightPairRotationEventEffect))]
    [HeckPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class LightPairRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        internal static BeatmapEventData? LastLightPairRotationEventEffectData { get; private set; }

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

    [HeckPatch(typeof(LightPairRotationEventEffect))]
    [HeckPatch("UpdateRotationData")]
    internal static class LightPairRotationEventEffectUpdateRotationData
    {
        private static bool Prefix(
            BeatmapEventType ____eventL,
            float startRotationOffset,
            float direction,
            LightPairRotationEventEffect.RotationData ____rotationDataL,
            LightPairRotationEventEffect.RotationData ____rotationDataR,
            Vector3 ____rotationVector)
        {
            BeatmapEventData beatmapEventData = LightPairRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.LastLightPairRotationEventEffectData!;

            ChromaEventData? chromaData = TryGetEventData(beatmapEventData);
            if (chromaData == null)
            {
                return true;
            }

            bool isLeftEvent = beatmapEventData.type == ____eventL;

            LightPairRotationEventEffect.RotationData rotationData = isLeftEvent ? ____rotationDataL : ____rotationDataR;

            bool lockPosition = chromaData.LockPosition;
            float precisionSpeed = chromaData.Speed.GetValueOrDefault(beatmapEventData.value);
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
            Transform transform = rotationData.transform;
            Quaternion startRotation = rotationData.startRotation;
            float startRotationAngle = rotationData.startRotationAngle;
            Vector3 rotationVector = ____rotationVector;
            if (beatmapEventData.value == 0)
            {
                rotationData.enabled = false;
                if (!lockPosition)
                {
                    rotationData.rotationAngle = startRotationAngle;
                    transform.localRotation = startRotation * Quaternion.Euler(rotationVector * startRotationAngle);
                }
            }
            else if (beatmapEventData.value > 0)
            {
                rotationData.enabled = true;
                rotationData.rotationSpeed = precisionSpeed * 20f * direction;
                if (!lockPosition)
                {
                    float rotationAngle = startRotationOffset + startRotationAngle;
                    rotationData.rotationAngle = rotationAngle;
                    transform.localRotation = startRotation * Quaternion.Euler(rotationVector * rotationAngle);
                }
            }

            return false;
        }
    }
}
