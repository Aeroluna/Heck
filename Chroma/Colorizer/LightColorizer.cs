using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.Lighting;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Chroma.Colorizer;

[UsedImplicitly]
public class LightColorizerManager
{
    private const int COLOR_FIELDS = LightColorizer.COLOR_FIELDS;

    private readonly List<Tuple<BasicBeatmapEventType, Action<LightColorizer>>> _contracts = [];
    private readonly List<Tuple<int, Action<LightColorizer>>> _contractsByLightID = [];

    private readonly LightColorizer.Factory _factory;

    internal LightColorizerManager(LightColorizer.Factory factory)
    {
        _factory = factory;
    }

    public Dictionary<BasicBeatmapEventType, LightColorizer> Colorizers { get; } = new();

    public Dictionary<int, LightColorizer> ColorizersByLightID { get; } = new();

    public Color?[] GlobalColor { get; } = new Color?[COLOR_FIELDS];

    public void Colorize(BasicBeatmapEventType eventType, bool refresh, params Color?[] colors)
    {
        GetColorizer(eventType).Colorize(refresh, colors);
    }

    [PublicAPI]
    public void Colorize(
        BasicBeatmapEventType eventType,
        IEnumerable<ILightWithId> selectLights,
        params Color?[] colors)
    {
        GetColorizer(eventType).Colorize(selectLights, colors);
    }

    public LightColorizer GetColorizer(BasicBeatmapEventType eventType)
    {
        return Colorizers[eventType];
    }

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

    internal LightColorizer Create(ChromaLightSwitchEventEffect chromaLightSwitchEventEffect)
    {
        LightColorizer colorizer = _factory.Create(chromaLightSwitchEventEffect);
        Colorizers.Add(chromaLightSwitchEventEffect.EventType, colorizer);
        ColorizersByLightID.Add(chromaLightSwitchEventEffect.LightsID, colorizer);
        return colorizer;
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
}

[UsedImplicitly]
public class LightColorizer
{
    internal const int COLOR_FIELDS = 4;

    private readonly LightColorizerManager _colorizerManager;

    private readonly Color?[] _colors = new Color?[COLOR_FIELDS];

    private readonly int _lightId;
    private readonly SimpleColorSO[] _originalColors = new SimpleColorSO[COLOR_FIELDS];

    private readonly Color[] _reusableColorsList = new Color[COLOR_FIELDS]; // This prevents a big amount of allocation.

    private readonly HashSet<ILightWithId>
        _reusableLightsList = []; // This prevents a significant amount of allocation.

    private readonly HashSet<ILightWithId>
        _reusablePropagationLightsList = []; // This prevents a significant amount of allocation.

    private readonly LightIDTableManager _tableManager;

    private ILightWithId[][]? _lightsPropagationGrouped;

    private LightColorizer(
        ChromaLightSwitchEventEffect chromaLightSwitchEventEffect,
        LightColorizerManager colorizerManager,
        LightWithIdManager lightManager,
        LightIDTableManager tableManager)
    {
        ChromaLightSwitchEventEffect = chromaLightSwitchEventEffect;
        _colorizerManager = colorizerManager;
        _tableManager = tableManager;

        _lightId = chromaLightSwitchEventEffect.LightsID;

        LightSwitchEventEffect lightSwitchEventEffect = chromaLightSwitchEventEffect.LightSwitchEventEffect;
        Initialize(lightSwitchEventEffect._lightColor0, 0);
        Initialize(lightSwitchEventEffect._lightColor1, 1);
        Initialize(lightSwitchEventEffect._lightColor0Boost, 2);
        Initialize(lightSwitchEventEffect._lightColor1Boost, 3);

        List<ILightWithId>? lights = lightManager._lights[lightSwitchEventEffect.lightsId];

        // possible uninitialized
        if (lights == null)
        {
            lights = new List<ILightWithId>(10);
            lightManager._lights[lightSwitchEventEffect.lightsId] = lights;
        }

        Lights = lights;
        return;

        void Initialize(ColorSO colorSO, int index)
        {
            _originalColors[index] = colorSO switch
            {
                MultipliedColorSO lightMultSO => lightMultSO._baseColor,
                SimpleColorSO simpleColorSO => simpleColorSO,
                _ => throw new InvalidOperationException($"Unhandled ColorSO type: [{colorSO.GetType().Name}].")
            };
        }
    }

    public ChromaLightSwitchEventEffect ChromaLightSwitchEventEffect { get; }

    public Color[] Color
    {
        get
        {
            for (int i = 0; i < COLOR_FIELDS; i++)
            {
                _reusableColorsList[i] = _colors[i] ?? _colorizerManager.GlobalColor[i] ?? _originalColors[i];
            }

            return _reusableColorsList;
        }
    }

    public IReadOnlyList<ILightWithId> Lights { get; }

    public ILightWithId[][] LightsPropagationGrouped
    {
        get
        {
            if (_lightsPropagationGrouped != null)
            {
                return _lightsPropagationGrouped;
            }

            // AAAAAA PROPAGATION STUFFF
            Dictionary<int, List<ILightWithId>> lightsPreGroup = new();
            TrackLaneRingsManager[] managers = Object.FindObjectsOfType<TrackLaneRingsManager>();
            foreach (ILightWithId light in Lights)
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
                    list = [light];
                    lightsPreGroup.Add(z, list);
                }
            }

            _lightsPropagationGrouped = new ILightWithId[lightsPreGroup.Count][];
            int i = 0;
            foreach (List<ILightWithId> lightList in lightsPreGroup.Values)
            {
                if (lightList is null)
                {
                    continue;
                }

                _lightsPropagationGrouped[i] = lightList.ToArray();
                i++;
            }

            return _lightsPropagationGrouped;
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

    public IEnumerable<ILightWithId> GetLightWithIds(IEnumerable<int> ids)
    {
        _reusableLightsList.Clear();
        foreach (int id in ids)
        {
            int newId = _tableManager.GetActiveTableValue(_lightId, id) ?? id;
            ILightWithId? lightWithId = Lights.ElementAtOrDefault(newId);
            if (lightWithId != null)
            {
                _reusableLightsList.Add(lightWithId);
            }
        }

        return _reusableLightsList;
    }

    public void Refresh(IEnumerable<ILightWithId>? selectLights)
    {
        ChromaLightSwitchEventEffect.Refresh(false, selectLights);
    }

    internal IEnumerable<ILightWithId> GetPropagationLightWithId(int id)
    {
        return LightsPropagationGrouped.Length > id ? LightsPropagationGrouped[id] : [];
    }

    // dont use this please
    // cant be fucked to make an overload for this
    internal IEnumerable<ILightWithId> GetPropagationLightWithIds(IEnumerable<object> ids)
    {
        _reusablePropagationLightsList.Clear();
        int lightCount = LightsPropagationGrouped.Length;
        foreach (int id in ids)
        {
            if (lightCount > id)
            {
                _reusablePropagationLightsList.UnionWith(LightsPropagationGrouped[id]);
            }
        }

        return _reusablePropagationLightsList;
    }

    [UsedImplicitly]
    internal class Factory : PlaceholderFactory<ChromaLightSwitchEventEffect, LightColorizer>;
}
