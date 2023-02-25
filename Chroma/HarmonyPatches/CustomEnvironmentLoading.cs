using System.Collections.Generic;
using System.Linq;
using Chroma.Settings;
using CustomJSONData.CustomBeatmap;
using SiraUtil.Affinity;
using static Chroma.ChromaController;

namespace Chroma.HarmonyPatches
{
    internal class CustomEnvironmentLoading : IAffinity
    {
        private readonly Config _config;

        private CustomEnvironmentLoading(Config config)
        {
            _config = config;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapDataTransformHelper), nameof(BeatmapDataTransformHelper.CreateTransformedBeatmapData))]
        private void Prefix(ref IReadonlyBeatmapData beatmapData, ref EnvironmentEffectsFilterPreset environmentEffectsFilterPreset)
        {
            CustomBeatmapData customBeatmapData = (CustomBeatmapData)beatmapData;
            if ((customBeatmapData.beatmapCustomData.Get<List<object>>(V2_ENVIRONMENT_REMOVAL)?.Any() ?? false) ||
                 (customBeatmapData.customData.Get<List<object>>(V2_ENVIRONMENT)?.Any() ?? false) ||
                 (customBeatmapData.customData.Get<List<object>>(ENVIRONMENT)?.Any() ?? false))
            {
                return;
            }

            if (!_config.CustomEnvironmentEnabled)
            {
                return;
            }

            EnvironmentEffectsFilterPreset? forcedPreset = _config.CustomEnvironment?.Features.ForcedPreset;
            if (forcedPreset.HasValue)
            {
                environmentEffectsFilterPreset = forcedPreset.Value;
                if (forcedPreset == EnvironmentEffectsFilterPreset.NoEffects)
                {
                    beatmapData = beatmapData.GetFilteredCopy(n =>
                        n is BasicBeatmapEventData or LightColorBeatmapEventData or LightRotationBeatmapEventData ? null : n);
                }
            }

            List<CustomBeatmapSaveData.BasicEventData>? basicEventDatas = _config.CustomEnvironment?.Features.BasicEventDatas;

            // ReSharper disable once InvertIf
            if (basicEventDatas != null)
            {
                if (beatmapData != customBeatmapData)
                {
                    beatmapData = beatmapData.GetCopy();
                }

                BeatmapData nonreadonlyBeatmapData = (BeatmapData)beatmapData;
                basicEventDatas.ForEach(n => nonreadonlyBeatmapData.InsertBeatmapEventData(new CustomBasicBeatmapEventData(
                    -1,
                    (BasicBeatmapEventType)n.eventType,
                    n.value,
                    n.floatValue,
                    n.customData)));
                nonreadonlyBeatmapData.ProcessAndSortBeatmapData();
            }
        }
    }
}
