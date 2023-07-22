using System.Collections.Generic;
using System.Linq;
using Chroma.Colorizer;
using Chroma.HarmonyPatches.Colorizer.Initialize;
using CustomJSONData.CustomBeatmap;
using IPA.Logging;
using JetBrains.Annotations;
using static Chroma.EnvironmentEnhancement.Component.ComponentConstants;

namespace Chroma.EnvironmentEnhancement.Component
{
    internal class ILightWithIdCustomizer
    {
        private readonly LightColorizerManager _lightColorizerManager;
        private readonly LightWithIdRegisterer _lightWithIdRegisterer;
        private readonly LightWithIdManager _lightWithIdManager;

        [UsedImplicitly]
        private ILightWithIdCustomizer(
            LightColorizerManager lightColorizerManager,
            LightWithIdRegisterer lightWithIdRegisterer,
            LightWithIdManager lightWithIdManager)
        {
            _lightColorizerManager = lightColorizerManager;
            _lightWithIdRegisterer = lightWithIdRegisterer;
            _lightWithIdManager = lightWithIdManager;
        }

        internal void ILightWithIdInit(List<UnityEngine.Component> allComponents, CustomData customData)
        {
            ILightWithId[] lightWithIds = allComponents
                .OfType<LightWithIds>()
                .SelectMany(n => n._lightWithIds)
                .Cast<ILightWithId>()
                .Concat(allComponents.OfType<LightWithIdMonoBehaviour>())
                .ToArray();
            if (lightWithIds.Length == 0)
            {
                Log.Logger.Log($"No [{LIGHT_WITH_ID}] component found.", Logger.Level.Error);
                return;
            }

            int? lightID = customData.Get<int?>(LIGHT_ID);
            int? type = customData.Get<int?>(LIGHT_TYPE);
            if (!type.HasValue && !lightID.HasValue)
            {
                return;
            }

            foreach (ILightWithId lightWithId in lightWithIds)
            {
                void SetType()
                {
                    if (!type.HasValue)
                    {
                        return;
                    }

                    int lightId = _lightColorizerManager.GetColorizer((BasicBeatmapEventType)type.Value).ChromaLightSwitchEventEffect.LightsID;

                    switch (lightWithId)
                    {
                        case LightWithIds.LightWithId lightWithIdsLightWithId:
                            lightWithIdsLightWithId._lightId = lightId;
                            break;

                        case LightWithIdMonoBehaviour lightWithIdMonoBehaviour:
                            lightWithIdMonoBehaviour._ID = lightId;
                            break;
                    }
                }

                void SetLightID()
                {
                    if (lightID.HasValue)
                    {
                        _lightWithIdRegisterer.SetRequestedId(lightWithId, lightID.Value);
                    }
                }

                if (lightWithId.isRegistered)
                {
                    _lightWithIdRegisterer.ForceUnregister(lightWithId);
                    _lightWithIdRegisterer.MarkForTableRegister(lightWithId);
                    SetType();
                    SetLightID();
                    _lightWithIdManager.RegisterLight(lightWithId);
                }
                else
                {
                    SetType();
                    SetLightID();
                }
            }
        }
    }
}
