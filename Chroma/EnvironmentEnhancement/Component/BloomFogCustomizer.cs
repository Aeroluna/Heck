using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using static Chroma.EnvironmentEnhancement.Component.ComponentConstants;

namespace Chroma.EnvironmentEnhancement.Component;

// TODO: zenject this
internal static class BloomFogCustomizer
{
    internal static void BloomFogEnvironmentInit(List<UnityEngine.Component> allComponents, CustomData customData)
    {
        BloomFogEnvironment[] bloomFogEnvironments = allComponents
            .OfType<BloomFogEnvironment>()
            .ToArray();
        if (bloomFogEnvironments.Length == 0)
        {
            Plugin.Log.Error($"No [{BLOOM_FOG_ENVIRONMENT}] component found");
            return;
        }

        float? attenuation = customData.Get<float?>(ATTENUATION);
        float? offset = customData.Get<float?>(OFFSET);
        float? heightFogStartY = customData.Get<float?>(HEIGHT_FOG_STARTY);
        float? heightFogHeight = customData.Get<float?>(HEIGHT_FOG_HEIGHT);

        foreach (BloomFogEnvironment bloomFogEnvironment in bloomFogEnvironments)
        {
            BloomFogEnvironmentParams fogParams = bloomFogEnvironment._fogParams;

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

    internal static object[] GetComponents(Track track)
    {
        return track
            .GameObjects
            .SelectMany(n => n.GetComponentsInChildren<BloomFogEnvironment>())
            .Select(n => n._fogParams)
            .Cast<object>()
            .ToArray();
    }
}
