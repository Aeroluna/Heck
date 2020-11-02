namespace Chroma
{
    using System.Collections.Generic;
    using System.Linq;
    using Chroma.Colorizer;
    using Chroma.Utils;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;
    using static Plugin;

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
                    object lightID = Trees.at(dynData, LIGHTID);
                    if (lightID != null)
                    {
                        LightWithId[] lights = lightSwitchEventEffect.GetLights();
                        int lightCount = lights.Length;
                        switch (lightID)
                        {
                            case List<object> lightIDobjects:
                                int[] lightIDArray = lightIDobjects.Select(n => System.Convert.ToInt32(n)).ToArray();
                                List<LightWithId> overrideLights = new List<LightWithId>();
                                for (int i = 0; i < lightIDArray.Length; i++)
                                {
                                    if (lightCount > lightIDArray[i])
                                    {
                                        overrideLights.Add(lights[lightIDArray[i]]);
                                    }
                                }

                                SetOverrideLightWithIds(overrideLights.ToArray());

                                break;

                            case long lightIDint:
                                if (lightCount > lightIDint)
                                {
                                    SetOverrideLightWithIds(lights[lightIDint]);
                                }

                                break;
                        }
                    }

                    object propID = Trees.at(dynData, PROPAGATIONID);
                    if (propID != null)
                    {
                        LightWithId[][] lights = lightSwitchEventEffect.GetLightsPropagationGrouped();
                        int lightCount = lights.Length;
                        switch (propID)
                        {
                            case List<object> propIDobjects:
                                int[] propIDArray = propIDobjects.Select(n => System.Convert.ToInt32(n)).ToArray();
                                List<LightWithId> overrideLights = new List<LightWithId>();
                                for (int i = 0; i < propIDArray.Length; i++)
                                {
                                    if (lightCount > propIDArray[i])
                                    {
                                        overrideLights.AddRange(lights[propIDArray[i]]);
                                    }
                                }

                                SetOverrideLightWithIds(overrideLights.ToArray());

                                break;

                            case long propIDlong:
                                if (lightCount > propIDlong)
                                {
                                    SetOverrideLightWithIds(lights[propIDlong]);
                                }

                                break;
                        }
                    }

                    dynamic gradientObject = Trees.at(dynData, LIGHTGRADIENT);
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
                monobehaviour.SetLightingColors(color.Value, color.Value, color.Value, color.Value);
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
