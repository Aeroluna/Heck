namespace Chroma
{
    using Chroma.Colorizer;
    using Chroma.Utils;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;

    internal static class LightColorManager
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        internal static void ColorLightSwitch(MonoBehaviour monobehaviour, BeatmapEventData beatmapEventData)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            monobehaviour.SetLastValue(beatmapEventData.value);

            Color? color = null;

            // legacy was a mistake
            color = LegacyLightHelper.GetLegacyColor(beatmapEventData) ?? color;

            if (beatmapEventData is CustomBeatmapEventData customData)
            {
                dynamic dynData = customData.customData;
                if (monobehaviour is LightSwitchEventEffect lightSwitchEventEffect)
                {
                    int? lightID = (int?)Trees.at(dynData, "_lightID");
                    if (lightID.HasValue)
                    {
                        LightWithId[] lights = lightSwitchEventEffect.GetLights();
                        if (lights.Length > lightID)
                        {
                            SetOverrideLightWithIds(lights[lightID.Value]);
                        }
                    }

                    int? propID = (int?)Trees.at(dynData, "_propID");
                    if (propID.HasValue)
                    {
                        LightWithId[][] lights = lightSwitchEventEffect.GetLightsPropagationGrouped();
                        if (lights.Length > propID)
                        {
                            SetOverrideLightWithIds(lights[propID.Value]);
                        }
                    }

                    dynamic gradientObject = Trees.at(dynData, "_lightGradient");
                    if (gradientObject != null)
                    {
                        color = ChromaGradientController.AddGradient(gradientObject, beatmapEventData.type, beatmapEventData.time);
                    }
                }

                Color? colorData = ChromaUtils.GetColorFromData(dynData);
                if (colorData.HasValue)
                {
                    color = colorData;
                    ChromaGradientController.CancelGradient(beatmapEventData.type);
                }
            }

            if (color.HasValue)
            {
                monobehaviour.SetLightingColors(color.Value, color.Value);
            }
            else if (!ChromaGradientController.IsGradientActive(beatmapEventData.type))
            {
                monobehaviour.Reset();
            }
        }

        private static void SetOverrideLightWithIds(params LightWithId[] lights)
        {
            HarmonyPatches.LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.OverrideLightWithIdActivation = lights;
        }
    }
}
