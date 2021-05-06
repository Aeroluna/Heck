namespace Chroma.HarmonyPatches
{
    using Heck;
    using UnityEngine;
    using static ChromaEventDataManager;

    [HeckPatch(typeof(LightRotationEventEffect))]
    [HeckPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class LightRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        private static bool Prefix(
            BeatmapEventData beatmapEventData,
            LightRotationEventEffect __instance,
            BeatmapEventType ____event,
            Quaternion ____startRotation,
            ref float ____rotationSpeed,
            Vector3 ____rotationVector)
        {
            if (beatmapEventData.type == ____event)
            {
                ChromaLaserSpeedEventData chromaData = TryGetEventData<ChromaLaserSpeedEventData>(beatmapEventData);
                if (chromaData == null)
                {
                    return true;
                }

                bool isLeftEvent = ____event == BeatmapEventType.Event12;

                bool lockPosition = chromaData.LockPosition;
                float precisionSpeed = chromaData.PreciseSpeed;
                int? dir = chromaData.Direction;

                float direction = (Random.value > 0.5f) ? 1f : -1f;
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
                if (beatmapEventData.value == 0)
                {
                    __instance.enabled = false;
                    if (!lockPosition)
                    {
                        __instance.transform.localRotation = ____startRotation;
                    }
                }
                else if (beatmapEventData.value > 0)
                {
                    __instance.enabled = true;
                    ____rotationSpeed = precisionSpeed * 20f * direction;
                    if (!lockPosition)
                    {
                        __instance.transform.localRotation = ____startRotation;
                        __instance.transform.Rotate(____rotationVector, Random.Range(0f, 180f), Space.Self);
                    }
                }

                return false;
            }

            return true;
        }
    }
}
