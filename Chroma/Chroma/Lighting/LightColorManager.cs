namespace Chroma
{
    using System.Collections.Generic;
    using System.Linq;
    using Chroma.Colorizer;
    using UnityEngine;
    using static ChromaEventDataManager;

    internal static class LightColorManager
    {
        internal static List<int> LightIDOverride { get; set; }

        internal static void ColorLightSwitch(MonoBehaviour monobehaviour, BeatmapEventData beatmapEventData)
        {
            if (!ChromaEventDatas.TryGetValue(beatmapEventData, out ChromaEventData chromaEventData))
            {
                return;
            }

            ChromaLightEventData chromaData = (ChromaLightEventData)chromaEventData;

            Color? color = null;

            // legacy was a mistake
            color = LegacyLightHelper.GetLegacyColor(beatmapEventData) ?? color;

            if (monobehaviour is LightSwitchEventEffect lightSwitchEventEffect)
            {
                object lightID = chromaData.LightID;
                if (lightID != null)
                {
                    switch (lightID)
                    {
                        case List<object> lightIDobjects:
                            LightIDOverride = lightIDobjects.Select(n => System.Convert.ToInt32(n)).ToList();

                            break;

                        case long lightIDint:
                            LightIDOverride = new List<int> { (int)lightIDint };

                            break;
                    }
                }

                // propID is now DEPRECATED!!!!!!!!
                object propID = chromaData.PropID;
                if (propID != null)
                {
                    ILightWithId[][] lights = lightSwitchEventEffect.GetLightsPropagationGrouped();
                    int lightCount = lights.Length;
                    switch (propID)
                    {
                        case List<object> propIDobjects:
                            int[] propIDArray = propIDobjects.Select(n => System.Convert.ToInt32(n)).ToArray();
                            List<ILightWithId> overrideLights = new List<ILightWithId>();
                            for (int i = 0; i < propIDArray.Length; i++)
                            {
                                if (lightCount > propIDArray[i])
                                {
                                    overrideLights.AddRange(lights[propIDArray[i]]);
                                }
                            }

                            SetLegacyPropIdOverride(overrideLights.ToArray());

                            break;

                        case long propIDlong:
                            if (lightCount > propIDlong)
                            {
                                SetLegacyPropIdOverride(lights[propIDlong]);
                            }

                            break;
                    }
                }

                ChromaLightEventData.GradientObjectData gradientObject = chromaData.GradientObject;
                if (gradientObject != null)
                {
                    color = ChromaGradientController.AddGradient(gradientObject, beatmapEventData.type, beatmapEventData.time);
                }
            }

            Color? colorData = chromaData.ColorData;
            if (colorData.HasValue)
            {
                color = colorData;
                ChromaGradientController.CancelGradient(beatmapEventData.type);
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

        private static void SetLegacyPropIdOverride(params ILightWithId[] lights)
        {
            HarmonyPatches.LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.LegacyLightOverride = lights;
        }
    }
}
