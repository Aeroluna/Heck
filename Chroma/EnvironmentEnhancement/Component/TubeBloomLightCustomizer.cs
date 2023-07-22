using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using IPA.Logging;
using static Chroma.EnvironmentEnhancement.Component.ComponentConstants;

namespace Chroma.EnvironmentEnhancement.Component
{
    internal static class TubeBloomLightCustomizer
    {
        internal static void TubeBloomPrePassLightInit(List<UnityEngine.Component> allComponents, CustomData customData)
        {
            TubeBloomPrePassLight[] tubeBloomPrePassLights = allComponents
                .OfType<TubeBloomPrePassLight>()
                .ToArray();
            if (tubeBloomPrePassLights.Length == 0)
            {
                Log.Logger.Log($"No [{TUBE_BLOOM_PRE_PASS_LIGHT}] component found.", Logger.Level.Error);
                return;
            }

            float? colorAlphaMultiplier = customData.Get<float?>(COLOR_ALPHA_MULTIPLIER);
            float? bloomFogIntensityMultiplier = customData.Get<float?>(BLOOM_FOG_INTENSITY_MULTIPLIER);

            foreach (TubeBloomPrePassLight tubeBloomPrePassLight in tubeBloomPrePassLights)
            {
                if (colorAlphaMultiplier.HasValue)
                {
                    SetColorAlphaMultiplier(tubeBloomPrePassLight, colorAlphaMultiplier.Value);
                }

                if (bloomFogIntensityMultiplier.HasValue)
                {
                    tubeBloomPrePassLight.bloomFogIntensityMultiplier = bloomFogIntensityMultiplier.Value;
                }
            }
        }

        internal static object[] GetComponents(Track track)
        {
            return track.GameObjects
                .SelectMany(n => n.GetComponentsInChildren<TubeBloomPrePassLight>())
                .Cast<object>()
                .ToArray();
        }

        internal static void SetColorAlphaMultiplier(TubeBloomPrePassLight tubeBloomPrePassLight, float value)
        {
            tubeBloomPrePassLight._colorAlphaMultiplier = value;
            tubeBloomPrePassLight.MarkDirty();
        }
    }
}
