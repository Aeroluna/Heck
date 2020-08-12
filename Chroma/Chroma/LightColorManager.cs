

namespace Chroma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Chroma.Extensions;
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
                }

                Color? colorData = ChromaUtils.GetColorFromData(dynData);
                if (colorData != null)
                {
                    color = colorData;
                }
            }

            if (color.HasValue)
            {
                monobehaviour.SetLightingColors(color.Value, color.Value);
            }
            else
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
