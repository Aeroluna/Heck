using Heck;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace Chroma.HarmonyPatches.Events
{
    internal class LightRotationChromafier : IAffinity
    {
        private readonly CustomData _customData;

        private LightRotationChromafier([Inject(Id = ChromaController.ID)] CustomData customData)
        {
            _customData = customData;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(LightRotationEventEffect), nameof(LightRotationEventEffect.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger))]
        private bool Prefix(
            BeatmapEventData beatmapEventData,
            LightRotationEventEffect __instance,
            BeatmapEventType ____event,
            Quaternion ____startRotation,
            ref float ____rotationSpeed,
            Vector3 ____rotationVector)
        {
            if (beatmapEventData.type != ____event || !_customData.Resolve(beatmapEventData, out ChromaEventData? chromaData))
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
