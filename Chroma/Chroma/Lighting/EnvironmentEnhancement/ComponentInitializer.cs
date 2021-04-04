namespace Chroma
{
    using System.Collections.Generic;
    using System.Linq;
    using Chroma.Colorizer;
    using IPA.Utilities;
    using UnityEngine;

    internal static class ComponentInitializer
    {
        private static readonly FieldAccessor<TrackLaneRingsManager, TrackLaneRing[]>.Accessor _ringsAccessor = FieldAccessor<TrackLaneRingsManager, TrackLaneRing[]>.GetAccessor("_rings");
        private static readonly FieldAccessor<Spectrogram, BasicSpectrogramData>.Accessor _spectrogramDataAccessor = FieldAccessor<Spectrogram, BasicSpectrogramData>.GetAccessor("_spectrogramData");
        private static readonly FieldAccessor<LightRotationEventEffect, IBeatmapObjectCallbackController>.Accessor _lightCallbackControllerAccessor = FieldAccessor<LightRotationEventEffect, IBeatmapObjectCallbackController>.GetAccessor("_beatmapObjectCallbackController");
        private static readonly FieldAccessor<LightPairRotationEventEffect, IBeatmapObjectCallbackController>.Accessor _lightPairCallbackControllerAccessor = FieldAccessor<LightPairRotationEventEffect, IBeatmapObjectCallbackController>.GetAccessor("_beatmapObjectCallbackController");
        private static readonly FieldAccessor<LightPairRotationEventEffect, Transform>.Accessor _transformLAccessor = FieldAccessor<LightPairRotationEventEffect, Transform>.GetAccessor("_transformL");
        private static readonly FieldAccessor<LightPairRotationEventEffect, Transform>.Accessor _transformRAccessor = FieldAccessor<LightPairRotationEventEffect, Transform>.GetAccessor("_transformR");

        internal static void InitializeComponents(Transform root, Transform original)
        {
            LightWithIdMonoBehaviour lightWithIdMonoBehaviour = root.GetComponent<LightWithIdMonoBehaviour>();
            if (lightWithIdMonoBehaviour != null)
            {
                LightColorizer.RegisterLight(lightWithIdMonoBehaviour);
            }

            LightWithIds lightWithIds = root.GetComponent<LightWithIds>();
            if (lightWithIds != null)
            {
                LightColorizer.RegisterLight(lightWithIds);
            }

            TrackLaneRing trackLaneRing = root.GetComponent<TrackLaneRing>();
            if (trackLaneRing != null)
            {
                trackLaneRing.Init(Vector3.zero, root.position);

                TrackLaneRing originalRing = original.GetComponent<TrackLaneRing>();
                foreach (TrackLaneRingsManager manager in HarmonyPatches.TrackLaneRingsManagerAwake.RingManagers)
                {
                    TrackLaneRingsManager managerRef = manager;
                    TrackLaneRing[] rings = _ringsAccessor(ref managerRef);
                    if (rings.Contains(originalRing))
                    {
                        // ToList() to add and then back ToArray()
                        List<TrackLaneRing> ringsList = rings.ToList();
                        ringsList.Add(trackLaneRing);
                        _ringsAccessor(ref managerRef) = ringsList.ToArray();

                        if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                        {
                            ChromaLogger.Log($"Initialized TrackLaneRing");
                        }

                        break;
                    }
                }
            }

            Spectrogram spectrogram = root.GetComponent<Spectrogram>();
            if (spectrogram != null)
            {
                Spectrogram originalSpectrogram = original.GetComponent<Spectrogram>();

                _spectrogramDataAccessor(ref spectrogram) = _spectrogramDataAccessor(ref originalSpectrogram);

                if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                {
                    ChromaLogger.Log($"Initialized Spectrogram");
                }
            }

            LightRotationEventEffect lightRotationEvent = root.GetComponent<LightRotationEventEffect>();
            if (lightRotationEvent != null)
            {
                LightRotationEventEffect originalLightRotationEvent = original.GetComponent<LightRotationEventEffect>();

                _lightCallbackControllerAccessor(ref lightRotationEvent) = _lightCallbackControllerAccessor(ref originalLightRotationEvent);

                if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                {
                    ChromaLogger.Log($"Initialized LightRotationEventEffect");
                }
            }

            LightPairRotationEventEffect lightPairRotationEvent = root.GetComponent<LightPairRotationEventEffect>();
            if (lightPairRotationEvent != null)
            {
                LightPairRotationEventEffect originalLightPairRotationEvent = original.GetComponent<LightPairRotationEventEffect>();

                _lightPairCallbackControllerAccessor(ref lightPairRotationEvent) = _lightPairCallbackControllerAccessor(ref originalLightPairRotationEvent);

                Transform transformL = _transformLAccessor(ref originalLightPairRotationEvent);
                Transform transformR = _transformRAccessor(ref originalLightPairRotationEvent);

                _transformLAccessor(ref lightPairRotationEvent) = root.GetChild(transformL.GetSiblingIndex());
                _transformRAccessor(ref lightPairRotationEvent) = root.GetChild(transformR.GetSiblingIndex());

                // We have to enable the object to tell unity to run Start
                lightPairRotationEvent.enabled = true;

                if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                {
                    ChromaLogger.Log($"Initialized LightPairRotationEventEffect");
                }
            }

            foreach (Transform transform in root)
            {
                if (transform == root)
                {
                    continue;
                }

                int index = transform.GetSiblingIndex();
                InitializeComponents(transform, original.GetChild(index));
            }
        }
    }
}
