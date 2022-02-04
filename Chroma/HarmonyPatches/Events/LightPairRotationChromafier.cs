using Heck;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace Chroma.HarmonyPatches.Events
{
    internal class LightPairRotationChromafier : IAffinity
    {
        private readonly CustomData _customData;

        private BeatmapEventData? _lastData;

        private LightPairRotationChromafier([Inject(Id = ChromaController.ID)] CustomData customData)
        {
            _customData = customData;
        }

        // Laser rotation
        [AffinityPrefix]
        [AffinityPatch(typeof(LightPairRotationEventEffect), nameof(LightPairRotationEventEffect.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger))]
        private void LoadEventData(BeatmapEventData beatmapEventData, BeatmapEventType ____eventL, BeatmapEventType ____eventR)
        {
            if (beatmapEventData.type == ____eventL || beatmapEventData.type == ____eventR)
            {
                _lastData = beatmapEventData;
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(LightPairRotationEventEffect), nameof(LightPairRotationEventEffect.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger))]
        private void ResetEVentData()
        {
            _lastData = null;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(LightPairRotationEventEffect), nameof(LightPairRotationEventEffect.UpdateRotationData))]
        private bool Prefix(
            BeatmapEventType ____eventL,
            float startRotationOffset,
            float direction,
            LightPairRotationEventEffect.RotationData ____rotationDataL,
            LightPairRotationEventEffect.RotationData ____rotationDataR,
            Vector3 ____rotationVector)
        {
            if (_lastData == null || !_customData.Resolve(_lastData, out ChromaEventData? chromaData))
            {
                return true;
            }

            bool isLeftEvent = _lastData.type == ____eventL;

            LightPairRotationEventEffect.RotationData rotationData = isLeftEvent ? ____rotationDataL : ____rotationDataR;

            bool lockPosition = chromaData.LockPosition;
            float precisionSpeed = chromaData.Speed.GetValueOrDefault(_lastData.value);
            int? dir = chromaData.Direction;

            direction = dir switch
            {
                0 => isLeftEvent ? -1 : 1,
                1 => isLeftEvent ? 1 : -1,
                _ => direction
            };

            // Actual lasering
            Transform transform = rotationData.transform;
            Quaternion startRotation = rotationData.startRotation;
            float startRotationAngle = rotationData.startRotationAngle;
            switch (_lastData.value)
            {
                case 0:
                {
                    rotationData.enabled = false;
                    if (!lockPosition)
                    {
                        rotationData.rotationAngle = startRotationAngle;
                        transform.localRotation = startRotation * Quaternion.Euler(____rotationVector * startRotationAngle);
                    }

                    break;
                }

                case > 0:
                {
                    rotationData.enabled = true;
                    rotationData.rotationSpeed = precisionSpeed * 20f * direction;
                    if (!lockPosition)
                    {
                        float rotationAngle = startRotationOffset + startRotationAngle;
                        rotationData.rotationAngle = rotationAngle;
                        transform.localRotation = startRotation * Quaternion.Euler(____rotationVector * rotationAngle);
                    }

                    break;
                }
            }

            return false;
        }
    }
}
