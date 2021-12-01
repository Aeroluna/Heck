using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.Lighting;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Logger = IPA.Logging.Logger;
using Object = UnityEngine.Object;

namespace Chroma.Colorizer
{
    public class LightColorizer
    {
        private const int COLOR_FIELDS = 4;

        private static readonly PropertyAccessor<LightWithIds, IEnumerable<LightWithIds.LightData>>.Getter _lightIntensityDataAcessor =
            PropertyAccessor<LightWithIds, IEnumerable<LightWithIds.LightData>>.GetGetter("lightIntensityData");

        private static readonly FieldAccessor<MultipliedColorSO, Color>.Accessor _multiplierColorAccessor = FieldAccessor<MultipliedColorSO, Color>.GetAccessor("_multiplierColor");
        private static readonly FieldAccessor<MultipliedColorSO, SimpleColorSO>.Accessor _baseColorAccessor = FieldAccessor<MultipliedColorSO, SimpleColorSO>.GetAccessor("_baseColor");

        private static readonly FieldAccessor<LightWithIdManager, List<ILightWithId>[]>.Accessor _lightsAccessor = FieldAccessor<LightWithIdManager, List<ILightWithId>[]>.GetAccessor("_lights");

        private readonly ChromaLightSwitchEventEffect _chromaLightSwitchEventEffect;
        private readonly BeatmapEventType _eventType;
        private readonly Color?[] _colors = new Color?[COLOR_FIELDS];

        private readonly Color[] _originalColors = new Color[COLOR_FIELDS];
        private readonly SimpleColorSO[] _simpleColorSOs = new SimpleColorSO[COLOR_FIELDS];

        private LightColorizer(ChromaLightSwitchEventEffect chromaLightSwitchEventEffect, BeatmapEventType beatmapEventType, LightWithIdManager lightManager)
        {
            _chromaLightSwitchEventEffect = chromaLightSwitchEventEffect;
            _eventType = beatmapEventType;

            // AAAAAA PROPAGATION STUFFF
            Lights = _lightsAccessor(ref lightManager)[chromaLightSwitchEventEffect.lightsId].ToList();

            IDictionary<int, List<ILightWithId>> lightsPreGroup = new Dictionary<int, List<ILightWithId>>();
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

        internal static event Action<BeatmapEventType, Color[]>? LightColorChanged;

        public static Dictionary<BeatmapEventType, LightColorizer> Colorizers { get; } = new();

        public static Color?[] GlobalColor { get; } = new Color?[COLOR_FIELDS];

        public List<ILightWithId> Lights { get; }

        public ILightWithId[][] LightsPropagationGrouped { get; }

        public Color[] Color
        {
            get
            {
                Color[] colors = new Color[COLOR_FIELDS];
                for (int i = 0; i < COLOR_FIELDS; i++)
                {
                    colors[i] = _colors[i] ?? GlobalColor[i] ?? _originalColors[i];
                }

                return colors;
            }
        }

        [PublicAPI]
        public static void GlobalColorize(IEnumerable<ILightWithId>? selectLights, params Color?[] colors)
        {
            GlobalColorize(true, selectLights, colors);
        }

        [PublicAPI]
        public static void GlobalColorize(bool refresh, params Color?[] colors)
        {
            GlobalColorize(refresh, null, colors);
        }

        public static void GlobalColorize(bool refresh, IEnumerable<ILightWithId>? selectLights, Color?[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                GlobalColor[i] = colors[i];
            }

            IEnumerable<ILightWithId>? lightWithIds = selectLights as ILightWithId[] ?? selectLights?.ToArray();
            foreach ((_, LightColorizer lightColorizer) in Colorizers)
            {
                lightColorizer.SetSOs(lightColorizer.Color);

                // Allow light colorizer to not force color
                if (!refresh)
                {
                    continue;
                }

                lightColorizer.Refresh(lightWithIds);
            }
        }

        public static void RegisterLight(MonoBehaviour lightWithId, int? lightId)
        {
            void RegisterLightWithID(ILightWithId lightToRegister)
            {
                int type = lightToRegister.lightId - 1;
                LightColorizer lightColorizer = ((BeatmapEventType)type).GetLightColorizer();
                int index = lightColorizer.Lights.Count;
                LightIDTableManager.RegisterIndex(lightToRegister.lightId - 1, index, lightId);
                lightColorizer._chromaLightSwitchEventEffect.RegisterLight(lightToRegister, type, index);
                lightColorizer.Lights.Add(lightToRegister);
            }

            switch (lightWithId)
            {
                case LightWithIdMonoBehaviour monoBehaviour:
                    RegisterLightWithID(monoBehaviour);
                    break;

                case LightWithIds lightWithIds:
                    IEnumerable<ILightWithId> lightsWithId = _lightIntensityDataAcessor(ref lightWithIds);
                    foreach (ILightWithId light in lightsWithId)
                    {
                        RegisterLightWithID(light);
                    }

                    break;
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

            SetSOs(Color);

            // Allow light colorizer to not force color
            if (!refresh)
            {
                return;
            }

            Refresh(selectLights);
        }

        public IEnumerable<ILightWithId> GetLightWithIds(IEnumerable<int> ids)
        {
            List<ILightWithId> result = new();
            int type = (int)_eventType;
            IEnumerable<int> newIds = ids.Select(n => LightIDTableManager.GetActiveTableValue(type, n) ?? n);
            foreach (int id in newIds)
            {
                ILightWithId? lightWithId = Lights.ElementAtOrDefault(id);
                if (lightWithId != null)
                {
                    result.Add(lightWithId);
                }
                else
                {
                    Log.Logger.Log($"Type [{type}] does not contain id [{id}].", Logger.Level.Error);
                }
            }

            return result;
        }

        internal static LightColorizer Create(ChromaLightSwitchEventEffect chromaLightSwitchEventEffect, BeatmapEventType beatmapEventType, LightWithIdManager lightManager)
        {
            LightColorizer lightColorizer = new(chromaLightSwitchEventEffect, beatmapEventType, lightManager);
            Colorizers.Add(beatmapEventType, lightColorizer);
            return lightColorizer;
        }

        internal static void Reset()
        {
            for (int i = 0; i < COLOR_FIELDS; i++)
            {
                GlobalColor[i] = null;
            }
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

        internal void InitializeSOs(
            ref ColorSO lightColor0,
            ref ColorSO highlightColor0,
            ref ColorSO lightColor1,
            ref ColorSO highlightColor1,
            ref ColorSO lightColor0Boost,
            ref ColorSO highlightColor0Boost,
            ref ColorSO lightColor1Boost,
            ref ColorSO highlightColor1Boost)
        {
            void Initialize(ref ColorSO colorSO, int index)
            {
                MultipliedColorSO lightMultSO = (MultipliedColorSO)colorSO;

                Color multiplierColor = _multiplierColorAccessor(ref lightMultSO);
                SimpleColorSO lightSO = _baseColorAccessor(ref lightMultSO);
                _originalColors[index] = lightSO.color;

                MultipliedColorSO mColorSO = ScriptableObject.CreateInstance<MultipliedColorSO>();
                _multiplierColorAccessor(ref mColorSO) = multiplierColor;

                SimpleColorSO sColorSO;
                if (_simpleColorSOs[index] == null)
                {
                    sColorSO = ScriptableObject.CreateInstance<SimpleColorSO>();
                    sColorSO.SetColor(lightSO.color);
                    _simpleColorSOs[index] = sColorSO;
                }
                else
                {
                    sColorSO = _simpleColorSOs[index];
                }

                _baseColorAccessor(ref mColorSO) = sColorSO;

                colorSO = mColorSO;
            }

            Initialize(ref lightColor0, 0);
            Initialize(ref highlightColor0, 0);
            Initialize(ref lightColor1, 1);
            Initialize(ref highlightColor1, 1);
            Initialize(ref lightColor0Boost, 2);
            Initialize(ref highlightColor0Boost, 2);
            Initialize(ref lightColor1Boost, 3);
            Initialize(ref highlightColor1Boost, 3);
        }

        private void SetSOs(Color[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                _simpleColorSOs[i].SetColor(colors[i]);
            }

            LightColorChanged?.Invoke(_eventType, colors);
        }

        private void Refresh(IEnumerable<ILightWithId>? selectLights)
        {
            _chromaLightSwitchEventEffect.Refresh(false, selectLights);
        }
    }
}
