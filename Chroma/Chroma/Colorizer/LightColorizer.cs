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

        private static readonly FieldAccessor<LightSwitchEventEffect, int>.Accessor _prevValueAccessor = FieldAccessor<LightSwitchEventEffect, int>.GetAccessor("_prevLightSwitchBeatmapEventDataValue");
        private static readonly FieldAccessor<LightSwitchEventEffect, LightWithIdManager>.Accessor _lightManagerAccessor = FieldAccessor<LightSwitchEventEffect, LightWithIdManager>.GetAccessor("_lightManager");
        private static readonly FieldAccessor<LightWithIdManager, List<ILightWithId>[]>.Accessor _lightsAccessor = FieldAccessor<LightWithIdManager, List<ILightWithId>[]>.GetAccessor("_lights");

        private readonly LightSwitchEventEffect _lightSwitchEventEffect;
        private readonly BeatmapEventType _eventType;
        private readonly Color?[] _colors = new Color?[COLOR_FIELDS];

        private readonly Color[] _originalColors = new Color[COLOR_FIELDS];
        private readonly SimpleColorSO[] _simpleColorSOs = new SimpleColorSO[COLOR_FIELDS];

        internal LightColorizer(LightSwitchEventEffect lightSwitchEventEffect, BeatmapEventType beatmapEventType)
        {
            _lightSwitchEventEffect = lightSwitchEventEffect;
            _eventType = beatmapEventType;
            InitializeSO("_lightColor0", 0);
            InitializeSO("_highlightColor0", 0);
            InitializeSO("_lightColor1", 1);
            InitializeSO("_highlightColor1", 1);
            InitializeSO("_lightColor0Boost", 2);
            InitializeSO("_highlightColor0Boost", 2);
            InitializeSO("_lightColor1Boost", 3);
            InitializeSO("_highlightColor1Boost", 3);

            // AAAAAA PROPAGATION STUFFF
            LightWithIdManager lightManager = _lightManagerAccessor(ref lightSwitchEventEffect);
            Lights = _lightsAccessor(ref lightManager)[lightSwitchEventEffect.lightsId].ToList();

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

        internal static event Action<BeatmapEventType, Color[]> LightColorChanged;

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

        public static void GlobalColorize(bool refresh, params Color?[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                GlobalColor[i] = colors[i];
            }

            foreach (KeyValuePair<BeatmapEventType, LightColorizer> valuePair in Colorizers)
            {
                // Allow light colorizer to not force color
                if (refresh)
                {
                    valuePair.Value.Refresh();
                }
                else
                {
                    valuePair.Value.SetSOs(valuePair.Value.Color);
                }
            }
        }

        public static void RegisterLight(MonoBehaviour lightWithId, int? lightId)
        {
            LightColorizer lightColorizer;
            switch (lightWithId)
            {
                case LightWithIdMonoBehaviour monoBehaviour:
                    lightColorizer = ((BeatmapEventType)(monoBehaviour.lightId - 1)).GetLightColorizer();
                    LightIDTableManager.RegisterIndex(monoBehaviour.lightId - 1, lightColorizer.Lights.Count, lightId);
                    lightColorizer.Lights.Add(monoBehaviour);

                    break;

                case LightWithIds lightWithIds:
                    IEnumerable<ILightWithId> lightsWithId = ((IEnumerable)_lightWithIdsData.GetValue(lightWithId)).Cast<ILightWithId>();
                    foreach (ILightWithId light in lightsWithId)
                    {
                        lightColorizer = ((BeatmapEventType)(light.lightId - 1)).GetLightColorizer();
                        LightIDTableManager.RegisterIndex(light.lightId - 1, lightColorizer.Lights.Count, lightId);
                        lightColorizer.Lights.Add(light);
                    }

                    break;
            }
        }

        public void Colorize(bool refresh, params Color?[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                _colors[i] = colors[i];
            }

            // Allow light colorizer to not force color
            if (refresh)
            {
                Refresh();
            }
            else
            {
                SetSOs(Color);
            }
        }

        internal static void Reset()
        {
            for (int i = 0; i < COLOR_FIELDS; i++)
            {
                GlobalColor[i] = null;
            }
        }

        private void SetSOs(Color[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                _simpleColorSOs[i].SetColor(colors[i]);
            }

            LightColorChanged?.Invoke(_eventType, colors);
        }

        private void Refresh()
        {
            Color[] colors = Color;
            SetSOs(colors);

            LightSwitchEventEffect lightSwitchEventEffect = _lightSwitchEventEffect;
            _lightSwitchEventEffect.ProcessLightSwitchEvent(_prevValueAccessor(ref lightSwitchEventEffect), true);
        }

        private void InitializeSO(string id, int index)
        {
            LightSwitchEventEffect lightSwitchEventEffect = _lightSwitchEventEffect;
            FieldAccessor<LightSwitchEventEffect, ColorSO>.Accessor colorSOAcessor = FieldAccessor<LightSwitchEventEffect, ColorSO>.GetAccessor(id);

            MultipliedColorSO lightMultSO = (MultipliedColorSO)colorSOAcessor(ref lightSwitchEventEffect);

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

            colorSOAcessor(ref lightSwitchEventEffect) = mColorSO;
        }
    }
}
