using Heck;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace Chroma.HarmonyPatches.Events
{
    internal class LightRotationChromafier : IAffinity
    {
        private readonly DeserializedData _deserializedData;

        private LightRotationChromafier([Inject(Id = ChromaController.ID)] DeserializedData deserializedData)
        {
            _deserializedData = deserializedData;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(LightRotationEventEffect), nameof(LightRotationEventEffect.HandleBeatmapEvent))]
        private bool Prefix(
            BasicBeatmapEventData basicBeatmapEventData,
            LightRotationEventEffect __instance,
            BasicBeatmapEventType ____event,
            Quaternion ____startRotation,
            ref float ____rotationSpeed,
            Vector3 ____rotationVector)
        {
            if (!_deserializedData.Resolve(basicBeatmapEventData, out ChromaEventData? chromaData))
            {
                return true;
            }

            bool isLeftEvent = ____event == BasicBeatmapEventType.Event12;

            bool lockPosition = chromaData.LockPosition;
            float precisionSpeed = chromaData.Speed.GetValueOrDefault(basicBeatmapEventData.value);
            int? dir = chromaData.Direction;

            float direction = dir switch
            {
                0 => isLeftEvent ? -1 : 1,
                1 => isLeftEvent ? 1 : -1,
                _ => (Random.value > 0.5f) ? 1f : -1f
            };

            switch (basicBeatmapEventData.value)
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
