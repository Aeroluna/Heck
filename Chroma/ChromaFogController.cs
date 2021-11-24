using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using static Chroma.ChromaController;
using static Heck.Animation.AnimationHelper;

namespace Chroma
{
    internal class ChromaFogController : MonoBehaviour
    {
        private static readonly FieldAccessor<BloomFogSO, float>.Accessor _transitionAccessor = FieldAccessor<BloomFogSO, float>.GetAccessor("_transition");

        private static readonly BloomFogSO _bloomFog = Resources.FindObjectsOfTypeAll<BloomFogSO>().First();

        private static ChromaFogController? _instance;

        private BloomFogEnvironmentParams _transitionFogParams = null!;
        private Track _track = null!;

        [UsedImplicitly]
        internal static void OnTrackManagerCreated(TrackBuilder trackManager, CustomBeatmapData customBeatmapData)
        {
            List<CustomEventData> customEventsData = customBeatmapData.customEventsData;
            foreach (CustomEventData customEventData in customEventsData)
            {
                if (customEventData.type != ASSIGN_FOG_TRACK)
                {
                    continue;
                }

                string? trackName = customEventData.data.Get<string>("_track");
                if (trackName != null)
                {
                    trackManager.AddTrack(trackName);
                }
            }
        }

        internal static void CustomEventCallbackInit(CustomEventCallbackController customEventCallbackController)
        {
            customEventCallbackController.AddCustomEventCallback(Callback);
        }

        private static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type != ASSIGN_FOG_TRACK)
            {
                return;
            }

            ChromaCustomEventData? chromaData = ChromaCustomDataManager.TryGetCustomEventData(customEventData);
            if (chromaData == null)
            {
                return;
            }

            if (_instance == null)
            {
                _instance = new GameObject(nameof(ChromaFogController)).AddComponent<ChromaFogController>();
            }

            _instance._track = chromaData.Track;
        }

        private void Awake()
        {
            _transitionFogParams = ScriptableObject.CreateInstance<BloomFogEnvironmentParams>();
            BloomFogEnvironmentParams defaultParams = _bloomFog.defaultForParams;
            _transitionFogParams.attenuation = defaultParams.attenuation;
            _transitionFogParams.offset = defaultParams.offset;
            _transitionFogParams.heightFogStartY = defaultParams.heightFogStartY;
            _transitionFogParams.heightFogHeight = defaultParams.heightFogHeight;
            _bloomFog.transitionFogParams = _transitionFogParams;
        }

        private void OnDestroy()
        {
            _bloomFog.transitionFogParams = null;
            Destroy(_transitionFogParams);
        }

        private void Update()
        {
            float? attenuation = TryGetProperty<float?>(_track, ATTENUATION);
            if (attenuation.HasValue)
            {
                _transitionFogParams.attenuation = attenuation.Value;
            }

            float? offset = TryGetProperty<float?>(_track, OFFSET);
            if (offset.HasValue)
            {
                _transitionFogParams.offset = offset.Value;
            }

            float? startY = TryGetProperty<float?>(_track, HEIGHT_FOG_STARTY);
            if (startY.HasValue)
            {
                _transitionFogParams.heightFogStartY = startY.Value;
            }

            float? height = TryGetProperty<float?>(_track, HEIGHT_FOG_HEIGHT);
            if (height.HasValue)
            {
                _transitionFogParams.heightFogHeight = height.Value;
            }

            BloomFogSO bloomFog = _bloomFog;
            _transitionAccessor(ref bloomFog) = 1;
        }
    }
}
