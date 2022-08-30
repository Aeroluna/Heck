using System.Collections.Generic;
using System.Linq;
using Chroma.Settings;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using JetBrains.Annotations;
using static Chroma.ChromaController;

namespace Chroma.HarmonyPatches
{
    [HeckPatch(PatchType.Environment)]
    [HarmonyPatch(typeof(BeatmapDataTransformHelper), nameof(BeatmapDataTransformHelper.CreateTransformedBeatmapData))]
    internal static class CustomEnvironmentLoading
    {
        [UsedImplicitly]
        [HarmonyPrefix]
        private static void Prefix(ref IReadonlyBeatmapData beatmapData, ref EnvironmentEffectsFilterPreset environmentEffectsFilterPreset)
        {
            CustomBeatmapData customBeatmapData = (CustomBeatmapData)beatmapData;
            if ((customBeatmapData.beatmapCustomData.Get<List<object>>(V2_ENVIRONMENT_REMOVAL)?.Any() ?? false) ||
                 (customBeatmapData.customData.Get<List<object>>(V2_ENVIRONMENT)?.Any() ?? false) ||
                 (customBeatmapData.customData.Get<List<object>>(ENVIRONMENT)?.Any() ?? false))
            {
                return;
            }

            if (!ChromaConfig.Instance.CustomEnvironmentEnabled)
            {
                return;
            }

            EnvironmentEffectsFilterPreset? forcedPreset = ChromaConfig.Instance.CustomEnvironment?.Features.ForcedPreset;
            if (forcedPreset.HasValue)
            {
                environmentEffectsFilterPreset = forcedPreset.Value;
                if (forcedPreset == EnvironmentEffectsFilterPreset.NoEffects)
                {
                    beatmapData = beatmapData.GetFilteredCopy(n =>
                        n is BasicBeatmapEventData or LightColorBeatmapEventData or LightRotationBeatmapEventData ? null : n);
                }
            }

            List<CustomBeatmapSaveData.BasicEventData>? basicEventDatas = ChromaConfig.Instance.CustomEnvironment?.Features.BasicEventDatas;

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
            }
        }
    }
}
