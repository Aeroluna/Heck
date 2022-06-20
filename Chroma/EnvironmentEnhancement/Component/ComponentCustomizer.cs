using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using UnityEngine;
using static Chroma.EnvironmentEnhancement.Component.ComponentConstants;

namespace Chroma.EnvironmentEnhancement.Component
{
    internal class ComponentCustomizer
    {
        private readonly ILightWithIdCustomizer _lightWithIdCustomizer;

        [UsedImplicitly]
        private ComponentCustomizer(
            ILightWithIdCustomizer _lightWithIdCustomizer)
        {
            this._lightWithIdCustomizer = _lightWithIdCustomizer;
        }

        internal static void GetAllComponents(List<UnityEngine.Component> components, Transform root)
        {
            components.AddRange(root.GetComponents<UnityEngine.Component>());

            foreach (Transform transform in root)
            {
                GetAllComponents(components, transform);
            }
        }

        internal void Customize(Transform gameObject, CustomData customData)
        {
            List<UnityEngine.Component> allComponents = new();
            GetAllComponents(allComponents, gameObject);

            CustomData? lightWithID = customData.Get<CustomData>(LIGHT_WITH_ID);
            if (lightWithID != null)
            {
                _lightWithIdCustomizer.ILightWithIdInit(allComponents, lightWithID);
            }

            CustomData? bloomFogEnvironment = customData.Get<CustomData>(BLOOM_FOG_ENVIRONMENT);
            if (bloomFogEnvironment != null)
            {
                BloomFogCustomizer.BloomFogEnvironmentInit(allComponents, bloomFogEnvironment);
            }

            CustomData? tubeBloomPrePassLight = customData.Get<CustomData>(TUBE_BLOOM_PRE_PASS_LIGHT);
            if (tubeBloomPrePassLight != null)
            {
                TubeBloomLightCustomizer.TubeBloomPrePassLightInit(allComponents, tubeBloomPrePassLight);
            }
        }
    }
}
