using Heck;
using JetBrains.Annotations;
using UnityEngine;
using static Chroma.ChromaCustomDataManager;

namespace Chroma.HarmonyPatches.Events
{
    [HeckPatch(typeof(LightRotationEventEffect))]
    [HeckPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class LightRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        [UsedImplicitly]
        private static bool Prefix(
            BeatmapEventData beatmapEventData,
            LightRotationEventEffect __instance,
            BeatmapEventType ____event,
            Quaternion ____startRotation,
            ref float ____rotationSpeed,
            Vector3 ____rotationVector)
        {
            if (beatmapEventData.type != ____event)
            {
                return true;
            }

            ChromaEventData? chromaData = TryGetEventData(beatmapEventData);
            if (chromaData == null)
            {
                return true;
            }

            bool isLeftEvent = ____event == BeatmapEventType.Event12;

            bool lockPosition = chromaData.LockPosition;
            float precisionSpeed = chromaData.Speed.GetValueOrDefault(beatmapEventData.value);
            int? dir = chromaData.Direction;

            float direction = dir switch
            {
                0 => isLeftEvent ? -1 : 1,
                1 => isLeftEvent ? 1 : -1,
                _ => (Random.value > 0.5f) ? 1f : -1f
            };

            switch (beatmapEventData.value)
            {
                // Actual lasering
                case 0:
                {
                    __instance.enabled = false;
                    if (!lockPosition)
                    {
                        __instance.transform.localRotation = ____startRotation;
                    }

                    break;
                }

                case > 0:
                {
                    __instance.enabled = true;
                    ____rotationSpeed = precisionSpeed * 20f * direction;
                    if (!lockPosition)
                    {
                        __instance.transform.localRotation = ____startRotation;
                        __instance.transform.Rotate(____rotationVector, Random.Range(0f, 180f), Space.Self);
                    }

                    break;
                }
            }

            return false;
        }
    }
}
