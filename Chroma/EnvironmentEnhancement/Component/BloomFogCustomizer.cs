using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using static Chroma.EnvironmentEnhancement.Component.ComponentConstants;

namespace Chroma.EnvironmentEnhancement.Component
{
    internal static class BloomFogCustomizer
    {
        private static readonly FieldAccessor<BloomFogEnvironment, BloomFogEnvironmentParams>.Accessor _fogParamsAccessor = FieldAccessor<BloomFogEnvironment, BloomFogEnvironmentParams>.GetAccessor("_fogParams");

        internal static void BloomFogEnvironmentInit(List<UnityEngine.Component> allComponents, CustomData customData)
        {
            BloomFogEnvironment[] bloomFogEnvironments = allComponents
                .OfType<BloomFogEnvironment>()
                .ToArray();
            if (bloomFogEnvironments.Length == 0)
            {
                Log.Logger.Log($"No [{BLOOM_FOG_ENVIRONMENT}] component found.");
                return;
            }

            float? attenuation = customData.Get<float?>(ATTENUATION);
            float? offset = customData.Get<float?>(OFFSET);
            float? heightFogStartY = customData.Get<float?>(HEIGHT_FOG_STARTY);
            float? heightFogHeight = customData.Get<float?>(HEIGHT_FOG_HEIGHT);

            foreach (BloomFogEnvironment bloomFogEnvironment in bloomFogEnvironments)
            {
                BloomFogEnvironment fuckref = bloomFogEnvironment;
                BloomFogEnvironmentParams fogParams = _fogParamsAccessor(ref fuckref);

                if (attenuation.HasValue)
                {
                    fogParams.attenuation = attenuation.Value;
                }

                if (offset.HasValue)
                {
                    fogParams.offset = offset.Value;
                }

                if (heightFogStartY.HasValue)
                {
                    fogParams.heightFogStartY = heightFogStartY.Value;
                }

                if (heightFogHeight.HasValue)
                {
                    fogParams.heightFogHeight = heightFogHeight.Value;
                }
            }
        }
    }
}
