using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using static Chroma.EnvironmentEnhancement.Component.ComponentConstants;

namespace Chroma.EnvironmentEnhancement.Component
{
    internal static class TubeBloomLightCustomizer
    {
        private static readonly FieldAccessor<TubeBloomPrePassLight, float>.Accessor _colorAlphaMultiplierAccessor = FieldAccessor<TubeBloomPrePassLight, float>.GetAccessor("_colorAlphaMultiplier");
        private static readonly FieldAccessor<TubeBloomPrePassLight, float>.Accessor _bloomFogIntensityMultiplierAccessor = FieldAccessor<TubeBloomPrePassLight, float>.GetAccessor("_bloomFogIntensityMultiplier");

        internal static void TubeBloomPrePassLightInit(List<UnityEngine.Component> allComponents, CustomData customData)
        {
            TubeBloomPrePassLight[] tubeBloomPrePassLights = allComponents
                .OfType<TubeBloomPrePassLight>()
                .ToArray();
            if (tubeBloomPrePassLights.Length == 0)
            {
                Log.Logger.Log($"No [{TUBE_BLOOM_PRE_PASS_LIGHT}] component found.");
                return;
            }

            float? colorAlphaMultiplier = customData.Get<float?>(COLOR_ALPHA_MULTIPLIER);
            float? bloomFogIntensityMultiplier = customData.Get<float?>(BLOOM_FOG_INTENSITY_MULTIPLIER);

            foreach (TubeBloomPrePassLight tubeBloomPrePassLight in tubeBloomPrePassLights)
            {
                TubeBloomPrePassLight tubeBloomPrePassLightRef = tubeBloomPrePassLight;

                if (colorAlphaMultiplier.HasValue)
                {
                    _colorAlphaMultiplierAccessor(ref tubeBloomPrePassLightRef) = colorAlphaMultiplier.Value;
                }

                if (bloomFogIntensityMultiplier.HasValue)
                {
                    _bloomFogIntensityMultiplierAccessor(ref tubeBloomPrePassLightRef) = bloomFogIntensityMultiplier.Value;
                }
            }
        }
    }
}
