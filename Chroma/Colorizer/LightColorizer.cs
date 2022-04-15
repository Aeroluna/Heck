using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.Lighting;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Chroma.Colorizer
{
    [UsedImplicitly]
    public class LightColorizerManager
    {
        private const int COLOR_FIELDS = LightColorizer.COLOR_FIELDS;

        private readonly LightColorizer.Factory _factory;

        private readonly List<Tuple<BasicBeatmapEventType, Action<LightColorizer>>> _contracts = new();
        private readonly List<Tuple<int, Action<LightColorizer>>> _contractsByLightID = new();

        internal LightColorizerManager(LightColorizer.Factory factory)
        {
            _factory = factory;
        }

        public Dictionary<BasicBeatmapEventType, LightColorizer> Colorizers { get; } = new();

        public Dictionary<int, LightColorizer> ColorizersByLightID { get; } = new();

        public Color?[] GlobalColor { get; } = new Color?[COLOR_FIELDS];

        public LightColorizer GetColorizer(BasicBeatmapEventType eventType) => Colorizers[eventType];

        public void Colorize(BasicBeatmapEventType eventType, bool refresh, params Color?[] colors) => GetColorizer(eventType).Colorize(refresh, colors);

        [PublicAPI]
        public void Colorize(BasicBeatmapEventType eventType, IEnumerable<ILightWithId> selectLights, params Color?[] colors) => GetColorizer(eventType).Colorize(selectLights, colors);

        [PublicAPI]
        public void GlobalColorize(IEnumerable<ILightWithId>? selectLights, params Color?[] colors)
        {
            GlobalColorize(true, selectLights, colors);
        }

        [PublicAPI]
        public void GlobalColorize(bool refresh, params Color?[] colors)
        {
            GlobalColorize(refresh, null, colors);
        }

        public void GlobalColorize(bool refresh, IEnumerable<ILightWithId>? selectLights, Color?[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                GlobalColor[i] = colors[i];
            }

            IEnumerable<ILightWithId>? lightWithIds = selectLights as ILightWithId[] ?? selectLights?.ToArray();
            foreach ((_, LightColorizer lightColorizer) in Colorizers)
            {
                // Allow light colorizer to not force color
                if (!refresh)
                {
                    continue;
                }

                lightColorizer.Refresh(lightWithIds);
            }
        }

        internal LightColorizer Create(ChromaLightSwitchEventEffect chromaLightSwitchEventEffect)
        {
            LightColorizer colorizer = _factory.Create(chromaLightSwitchEventEffect);
            Colorizers.Add(chromaLightSwitchEventEffect.EventType, colorizer);
            ColorizersByLightID.Add(chromaLightSwitchEventEffect.LightsID, colorizer);
            return colorizer;
        }

        internal void CompleteContracts(ChromaLightSwitchEventEffect chromaLightSwitchEventEffect)
        {
            // complete open contracts
            Tuple<BasicBeatmapEventType, Action<LightColorizer>>[] contracts = _contracts.ToArray();
            foreach (Tuple<BasicBeatmapEventType, Action<LightColorizer>> contract in contracts)
            {
                if (chromaLightSwitchEventEffect.EventType != contract.Item1)
                {
                    continue;
                }

                contract.Item2(chromaLightSwitchEventEffect.Colorizer);
                _contracts.Remove(contract);
            }

            Tuple<int, Action<LightColorizer>>[] contractsByLightID = _contractsByLightID.ToArray();
            foreach (Tuple<int, Action<LightColorizer>> contract in contractsByLightID)
            {
                if (chromaLightSwitchEventEffect.LightsID != contract.Item1)
                {
                    continue;
                }

                contract.Item2(chromaLightSwitchEventEffect.Colorizer);
                _contractsByLightID.Remove(contract);
            }
        }

        internal void CreateLightColorizerContractByLightID(int lightId, Action<LightColorizer> callback)
        {
            if (ColorizersByLightID.TryGetValue(lightId, out LightColorizer colorizer))
            {
                callback(colorizer);
            }
            else
            {
                _contractsByLightID.Add(lightId, callback);
            }
        }

        internal void CreateLightColorizerContract(BasicBeatmapEventType type, Action<LightColorizer> callback)
        {
            if (Colorizers.TryGetValue(type, out LightColorizer colorizer))
            {
                callback(colorizer);
            }
            else
            {
                _contracts.Add(type, callback);
            }
        }
    }

    [UsedImplicitly]
    public class LightColorizer
    {
        internal const int COLOR_FIELDS = 4;

        private static readonly FieldAccessor<LightSwitchEventEffect, ColorSO>.Accessor _lightColor0Accessor = FieldAccessor<LightSwitchEventEffect, ColorSO>.GetAccessor("_lightColor0");
        private static readonly FieldAccessor<LightSwitchEventEffect, ColorSO>.Accessor _lightColor1Accessor = FieldAccessor<LightSwitchEventEffect, ColorSO>.GetAccessor("_lightColor1");
        private static readonly FieldAccessor<LightSwitchEventEffect, ColorSO>.Accessor _lightColor0BoostAccessor = FieldAccessor<LightSwitchEventEffect, ColorSO>.GetAccessor("_lightColor0Boost");
        private static readonly FieldAccessor<LightSwitchEventEffect, ColorSO>.Accessor _lightColor1BoostAccessor = FieldAccessor<LightSwitchEventEffect, ColorSO>.GetAccessor("_lightColor1Boost");

        private static readonly FieldAccessor<MultipliedColorSO, SimpleColorSO>.Accessor _baseColorAccessor = FieldAccessor<MultipliedColorSO, SimpleColorSO>.GetAccessor("_baseColor");
        private static readonly FieldAccessor<LightWithIdManager, List<ILightWithId>?[]>.Accessor _lightsAccessor = FieldAccessor<LightWithIdManager, List<ILightWithId>?[]>.GetAccessor("_lights");

        private readonly LightColorizerManager _colorizerManager;

        private readonly int _lightId;

        private readonly Color?[] _colors = new Color?[COLOR_FIELDS];
        private readonly SimpleColorSO[] _originalColors = new SimpleColorSO[COLOR_FIELDS];

        private LightColorizer(
            ChromaLightSwitchEventEffect chromaLightSwitchEventEffect,
            LightColorizerManager colorizerManager,
            LightWithIdManager lightManager)
        {
            ChromaLightSwitchEventEffect = chromaLightSwitchEventEffect;
            _colorizerManager = colorizerManager;

            _lightId = chromaLightSwitchEventEffect.LightsID;

            void Initialize(ColorSO colorSO, int index)
            {
                switch (colorSO)
                {
                    case MultipliedColorSO lightMultSO:
                        SimpleColorSO lightSO = _baseColorAccessor(ref lightMultSO);
                        _originalColors[index] = lightSO;
                        break;

                    case SimpleColorSO simpleColorSO:
                        _originalColors[index] = simpleColorSO;
                        break;

                    default:
                        throw new InvalidOperationException($"Unhandled ColorSO type: [{colorSO.GetType().Name}].");
                }
            }

            LightSwitchEventEffect lightSwitchEventEffect = chromaLightSwitchEventEffect.LightSwitchEventEffect;
            Initialize(_lightColor0Accessor(ref lightSwitchEventEffect), 0);
            Initialize(_lightColor1Accessor(ref lightSwitchEventEffect), 1);
            Initialize(_lightColor0BoostAccessor(ref lightSwitchEventEffect), 2);
            Initialize(_lightColor1BoostAccessor(ref lightSwitchEventEffect), 3);

            List<ILightWithId>? lights = _lightsAccessor(ref lightManager)[lightSwitchEventEffect.lightsId];

            // possible uninitialized
            if (lights == null)
            {
                lights = new List<ILightWithId>(10);
                _lightsAccessor(ref lightManager)[lightSwitchEventEffect.lightsId] = lights;
            }

            Lights = lights;

            // AAAAAA PROPAGATION STUFFF
            IDictionary<int, List<ILightWithId>> lightsPreGroup = new Dictionary<int, List<ILightWithId>>();
            TrackLaneRingsManager[] managers = Object.FindObjectsOfType<TrackLaneRingsManager>();
            foreach (ILightWithId light in lights)
            {
                if (light is not MonoBehaviour monoBehaviour)
                {
                    continue;
                }

                int z = Mathf.RoundToInt(monoBehaviour.transform.position.z);

                TrackLaneRing? ring = monoBehaviour.GetComponentInParent<TrackLaneRing>();
                if (ring != null)
                {
                    TrackLaneRingsManager? mngr = managers.FirstOrDefault(it => it.Rings.IndexOf(ring) >= 0);
                    if (mngr != null)
                    {
                        z = 1000 + mngr.Rings.IndexOf(ring);
                    }
                }

                if (lightsPreGroup.TryGetValue(z, out List<ILightWithId> list))
                {
                    list.Add(light);
                }
                else
                {
                    list = new List<ILightWithId> { light };
                    lightsPreGroup.Add(z, list);
                }
            }

            LightsPropagationGrouped = new ILightWithId[lightsPreGroup.Count][];
            int i = 0;
            foreach (List<ILightWithId> lightList in lightsPreGroup.Values)
            {
                if (lightList is null)
                {
                    continue;
                }

                LightsPropagationGrouped[i] = lightList.ToArray();
                i++;
            }
        }

        public ChromaLightSwitchEventEffect ChromaLightSwitchEventEffect { get; }

        public IReadOnlyList<ILightWithId> Lights { get; }

        public ILightWithId[][] LightsPropagationGrouped { get; }

        public Color[] Color
        {
            get
            {
                Color[] colors = new Color[COLOR_FIELDS];
                for (int i = 0; i < COLOR_FIELDS; i++)
                {
                    colors[i] = _colors[i] ?? _colorizerManager.GlobalColor[i] ?? _originalColors[i];
                }

                return colors;
            }
        }

        public void Colorize(IEnumerable<ILightWithId>? selectLights, params Color?[] colors)
        {
            Colorize(true, selectLights, colors);
        }

        public void Colorize(bool refresh, params Color?[] colors)
        {
            Colorize(refresh, null, colors);
        }

        public void Colorize(bool refresh, IEnumerable<ILightWithId>? selectLights, Color?[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                _colors[i] = colors[i];
            }

            // Allow light colorizer to not force color
            if (!refresh)
            {
                return;
            }

            Refresh(selectLights);
        }

        public void Refresh(IEnumerable<ILightWithId>? selectLights)
        {
            ChromaLightSwitchEventEffect.Refresh(false, selectLights);
        }

        public IEnumerable<ILightWithId> GetLightWithIds(IEnumerable<int> ids)
        {
            IEnumerable<int> newIds = ids.Select(n => LightIDTableManager.GetActiveTableValue(_lightId, n) ?? n);

            return newIds.Select(id => Lights.ElementAtOrDefault(id)).Where(lightWithId => lightWithId != null).ToList();
        }

        // dont use this please
        // cant be fucked to make an overload for this
        internal IEnumerable<ILightWithId> GetPropagationLightWithIds(IEnumerable<int> ids)
        {
            List<ILightWithId> result = new();
            int lightCount = LightsPropagationGrouped.Length;
            foreach (int id in ids)
            {
                if (lightCount > id)
                {
                    result.AddRange(LightsPropagationGrouped[id]);
                }
            }

            return result;
        }

        [UsedImplicitly]
        internal class Factory : PlaceholderFactory<ChromaLightSwitchEventEffect, LightColorizer>
        {
        }
    }
}
