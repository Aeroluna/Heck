namespace Chroma.Colorizer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using HarmonyLib;
    using IPA.Utilities;
    using UnityEngine;

    public class LightColorizer
    {
        private const int COLOR_FIELDS = 4;

        private static readonly FieldInfo _lightWithIdsData = AccessTools.Field(typeof(LightWithIds), "_lightIntensityData");

        private static readonly FieldAccessor<MultipliedColorSO, Color>.Accessor _multiplierColorAccessor = FieldAccessor<MultipliedColorSO, Color>.GetAccessor("_multiplierColor");
        private static readonly FieldAccessor<MultipliedColorSO, SimpleColorSO>.Accessor _baseColorAccessor = FieldAccessor<MultipliedColorSO, SimpleColorSO>.GetAccessor("_baseColor");

        private static readonly FieldAccessor<LightWithIdManager, List<ILightWithId>[]>.Accessor _lightsAccessor = FieldAccessor<LightWithIdManager, List<ILightWithId>[]>.GetAccessor("_lights");

        private readonly ChromaLightSwitchEventEffect _chromaLightSwitchEventEffect;
        private readonly BeatmapEventType _eventType;
        private readonly Color?[] _colors = new Color?[COLOR_FIELDS];

        private readonly Color[] _originalColors = new Color[COLOR_FIELDS];
        private readonly SimpleColorSO[] _simpleColorSOs = new SimpleColorSO[COLOR_FIELDS];

        internal LightColorizer(ChromaLightSwitchEventEffect chromaLightSwitchEventEffect, BeatmapEventType beatmapEventType, LightWithIdManager lightManager)
        {
            _chromaLightSwitchEventEffect = chromaLightSwitchEventEffect;
            _eventType = beatmapEventType;

            // AAAAAA PROPAGATION STUFFF
            Lights = _lightsAccessor(ref lightManager)[chromaLightSwitchEventEffect.lightsId].ToList();

            IDictionary<int, List<ILightWithId>> lightsPreGroup = new Dictionary<int, List<ILightWithId>>();
            TrackLaneRingsManager[] managers = UnityEngine.Object.FindObjectsOfType<TrackLaneRingsManager>();
            foreach (ILightWithId light in Lights)
            {
                if (light is MonoBehaviour monoBehaviour)
                {
                    int z = Mathf.RoundToInt(monoBehaviour.transform.position.z);

                    TrackLaneRing ring = monoBehaviour.GetComponentInParent<TrackLaneRing>();
                    if (ring != null)
                    {
                        TrackLaneRingsManager mngr = managers.FirstOrDefault(it => it.Rings.IndexOf(ring) >= 0);
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
                        list = new List<ILightWithId>() { light };
                        lightsPreGroup.Add(z, list);
                    }
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

            // ok we done
            Colorizers.Add(beatmapEventType, this);
        }

        internal static event Action<BeatmapEventType, Color[]>? LightColorChanged;

        public static Dictionary<BeatmapEventType, LightColorizer> Colorizers { get; } = new Dictionary<BeatmapEventType, LightColorizer>();

        public static Color?[] GlobalColor { get; private set; } = new Color?[COLOR_FIELDS];

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

        public static void GlobalColorize(IEnumerable<ILightWithId>? selectLights, params Color?[] colors)
        {
            GlobalColorize(true, selectLights, colors);
        }

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

            foreach (KeyValuePair<BeatmapEventType, LightColorizer> valuePair in Colorizers)
            {
                LightColorizer lightColorizer = valuePair.Value;
                lightColorizer.SetSOs(valuePair.Value.Color);

                // Allow light colorizer to not force color
                if (refresh)
                {
                    if (selectLights != null)
                    {
                        lightColorizer.Refresh(selectLights);
                    }
                    else
                    {
                        lightColorizer.Refresh(null);
                    }
                }
            }
        }

        public static void RegisterLight(MonoBehaviour lightWithId, int? lightId)
        {
            void RegisterLightWithID(ILightWithId lightWithId)
            {
                LightColorizer lightColorizer = ((BeatmapEventType)(lightWithId.lightId - 1)).GetLightColorizer();
                LightIDTableManager.RegisterIndex(lightWithId.lightId - 1, lightColorizer.Lights.Count, lightId);
                lightColorizer._chromaLightSwitchEventEffect.RegisterLight(lightWithId);
                lightColorizer.Lights.Add(lightWithId);
            }

            switch (lightWithId)
            {
                case LightWithIdMonoBehaviour monoBehaviour:
                    RegisterLightWithID(monoBehaviour);
                    break;

                case LightWithIds lightWithIds:
                    IEnumerable<ILightWithId> lightsWithId = ((IEnumerable)_lightWithIdsData.GetValue(lightWithId)).Cast<ILightWithId>();
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
            if (refresh)
            {
                if (selectLights != null)
                {
                    Refresh(selectLights);
                }
                else
                {
                    Refresh(null);
                }
            }
        }

        public IEnumerable<ILightWithId> GetLightWithIds(IEnumerable<int> ids)
        {
            List<ILightWithId> result = new List<ILightWithId>();
            int type = (int)_eventType;
            IEnumerable<int> newIds = ids.Select(n => LightIDTableManager.GetActiveTableValue(type, n) ?? n);
            foreach (int id in newIds)
            {
                ILightWithId lightWithId = Lights.ElementAtOrDefault(id);
                if (lightWithId != null)
                {
                    result.Add(lightWithId);
                }
                else
                {
                    Plugin.Logger.Log($"Type [{type}] does not contain id [{id}].", IPA.Logging.Logger.Level.Error);
                }
            }

            return result;
        }

        public IEnumerable<ILightWithId> GetLightWithIds(int id)
        {
            int newId = LightIDTableManager.GetActiveTableValue((int)_eventType, id) ?? id;
            ILightWithId lightWithId = Lights.ElementAtOrDefault(newId);
            if (lightWithId != null)
            {
                return new ILightWithId[] { lightWithId };
            }
            else
            {
                Plugin.Logger.Log($"Type [{(int)_eventType}] does not contain id [{id}].", IPA.Logging.Logger.Level.Error);
            }

            return new ILightWithId[0];
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
            List<ILightWithId> result = new List<ILightWithId>();
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
