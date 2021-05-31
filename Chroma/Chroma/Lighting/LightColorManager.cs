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

        internal static void ColorLightSwitch(LightSwitchEventEffect lightSwitchEventEffect, BeatmapEventData beatmapEventData)
        {
            ChromaLightEventData chromaData = TryGetEventData<ChromaLightEventData>(beatmapEventData);
            if (chromaData == null)
            {
                return;
            }

            Color? color = null;

            // legacy was a mistake
            color = LegacyLightHelper.GetLegacyColor(beatmapEventData) ?? color;

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
            if (propID != null && beatmapEventData.type.TryGetLightColorizer(out LightColorizer lightColorizer))
            {
                ILightWithId[][] lights = lightColorizer.LightsPropagationGrouped;
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

            Color? colorData = chromaData.ColorData;
            if (colorData.HasValue)
            {
                color = colorData;
                ChromaGradientController.CancelGradient(beatmapEventData.type);
            }

            if (color.HasValue)
            {
                Color finalColor = color.Value;
                beatmapEventData.type.ColorizeLight(finalColor, finalColor, finalColor, finalColor);
            }
            else if (!ChromaGradientController.IsGradientActive(beatmapEventData.type))
            {
                beatmapEventData.type.ColorizeLight(null, null, null, null);
            }
        }

        private static void SetLegacyPropIdOverride(params ILightWithId[] lights)
        {
            HarmonyPatches.LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.LegacyLightOverride = lights;
        }
    }
}
