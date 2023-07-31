using System.Collections.Generic;
using System.Linq;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.Settings;
using CustomJSONData.CustomBeatmap;
using SiraUtil.Affinity;
using static Chroma.ChromaController;

namespace Chroma.HarmonyPatches
{
    internal class CustomEnvironmentLoading : IAffinity
    {
        private readonly Config _config;
        private readonly SavedEnvironmentLoader _savedEnvironmentLoader;

        private CustomEnvironmentLoading(Config config, SavedEnvironmentLoader savedEnvironmentLoader)
        {
            _config = config;
            _savedEnvironmentLoader = savedEnvironmentLoader;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapDataTransformHelper), nameof(BeatmapDataTransformHelper.CreateTransformedBeatmapData))]
        private void Prefix(ref IReadonlyBeatmapData beatmapData, ref EnvironmentEffectsFilterPreset environmentEffectsFilterPreset)
        {
            if (!_config.CustomEnvironmentEnabled)
            {
                return;
            }

            CustomBeatmapData customBeatmapData = (CustomBeatmapData)beatmapData;
            if (!_config.EnvironmentEnhancementsDisabled && ((customBeatmapData.beatmapCustomData.Get<List<object>>(V2_ENVIRONMENT_REMOVAL)?.Any() ?? false) ||
                (customBeatmapData.customData.Get<List<object>>(V2_ENVIRONMENT)?.Any() ?? false) ||
                (customBeatmapData.customData.Get<List<object>>(ENVIRONMENT)?.Any() ?? false)))
            {
                return;
            }

            EnvironmentEffectsFilterPreset? forcedPreset = _savedEnvironmentLoader.SavedEnvironment?.Features.ForcedPreset;
            if (forcedPreset.HasValue)
            {
                environmentEffectsFilterPreset = forcedPreset.Value;
                if (forcedPreset == EnvironmentEffectsFilterPreset.NoEffects)
                {
                    beatmapData = beatmapData.GetFilteredCopy(n =>
                        n is BasicBeatmapEventData or LightColorBeatmapEventData or LightRotationBeatmapEventData ? null : n);
                }

                Log.Logger.Log(forcedPreset.Value);
            }

            List<CustomBeatmapSaveData.BasicEventData>? basicEventDatas = _savedEnvironmentLoader.SavedEnvironment?.Features.BasicEventDatas;

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
