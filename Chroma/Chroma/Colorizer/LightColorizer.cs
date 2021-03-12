namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using System.Linq;
    using IPA.Utilities;
    using UnityEngine;

    public static class LightColorizer
    {
        private static readonly HashSet<LSEColorManager> _lseColorManagers = new HashSet<LSEColorManager>();

        public static void Reset(this MonoBehaviour lse)
        {
            LSEColorManager.GetLSEColorManager(lse)?.Reset();
        }

        public static void ResetAllLightingColors()
        {
            foreach (LSEColorManager lseColorManager in _lseColorManagers)
            {
                lseColorManager.Reset();
            }
        }

        public static void SetLightingColors(this MonoBehaviour lse, Color? color0, Color? color1, Color? color0Boost = null, Color? color1Boost = null)
        {
            LSEColorManager.GetLSEColorManager(lse)?.SetLightingColors(color0, color1, color0Boost, color1Boost);
        }

        public static void SetLightingColors(this BeatmapEventType lse, Color? color0, Color? color1, Color? color0Boost = null, Color? color1Boost = null)
        {
            foreach (LSEColorManager l in LSEColorManager.GetLSEColorManager(lse))
            {
                l.SetLightingColors(color0, color1, color0Boost, color1Boost);
            }
        }

        public static void SetAllLightingColors(Color? color0, Color? color1, Color? color0Boost = null, Color? color1Boost = null)
        {
            foreach (LSEColorManager lseColorManager in _lseColorManagers)
            {
                lseColorManager.SetLightingColors(color0, color1, color0Boost, color1Boost);
            }
        }

        public static void SetActiveColors(this BeatmapEventType lse)
        {
            foreach (LSEColorManager l in LSEColorManager.GetLSEColorManager(lse))
            {
                l.SetActiveColors();
            }
        }

        public static void SetAllActiveColors()
        {
            foreach (LSEColorManager lseColorManager in _lseColorManagers)
            {
                lseColorManager.SetActiveColors();
            }
        }

        internal static void ClearLSEColorManagers()
        {
            _lseColorManagers.Clear();
        }

        internal static void SetLastValue(this MonoBehaviour lse, int value)
        {
            LSEColorManager.GetLSEColorManager(lse)?.SetLastValue(value);
        }

        internal static ILightWithId[] GetLights(this LightSwitchEventEffect lse)
        {
            return LSEColorManager.GetLSEColorManager(lse)?.Lights.ToArray();
        }

        internal static ILightWithId[][] GetLightsPropagationGrouped(this LightSwitchEventEffect lse)
        {
            return LSEColorManager.GetLSEColorManager(lse)?.LightsPropagationGrouped;
        }

        /*
         * LSE ColorSO holders
         */

        internal static void LSEStart(MonoBehaviour lse, BeatmapEventType type)
        {
            LSEColorManager.CreateLSEColorManager(lse, type);
        }

        private class LSEColorManager
        {
            private readonly MonoBehaviour _lse;
            private readonly BeatmapEventType _type;

            private readonly Color _lightColor0_Original;
            private readonly Color _lightColor1_Original;
            private readonly Color _lightColor0Boost_Original;
            private readonly Color _lightColor1Boost_Original;

            private readonly SimpleColorSO _lightColor0;
            private readonly SimpleColorSO _lightColor1;
            private readonly SimpleColorSO _lightColor0Boost;
            private readonly SimpleColorSO _lightColor1Boost;

            private readonly MultipliedColorSO _mLightColor0;
            private readonly MultipliedColorSO _mHighlightColor0;
            private readonly MultipliedColorSO _mLightColor1;
            private readonly MultipliedColorSO _mHighlightColor1;

            private readonly MultipliedColorSO _mLightColor0Boost;
            private readonly MultipliedColorSO _mHighlightColor0Boost;
            private readonly MultipliedColorSO _mLightColor1Boost;
            private readonly MultipliedColorSO _mHighlightColor1Boost;

            private readonly bool _supportBoostColor;

            private float _lastValue;

            private LSEColorManager(MonoBehaviour mono, BeatmapEventType type)
            {
                _lse = mono;
                _type = type;
                InitializeSOs(mono, "_lightColor0", ref _lightColor0, ref _lightColor0_Original, ref _mLightColor0);
                InitializeSOs(mono, "_highlightColor0", ref _lightColor0, ref _lightColor0_Original, ref _mHighlightColor0);
                InitializeSOs(mono, "_lightColor1", ref _lightColor1, ref _lightColor1_Original, ref _mLightColor1);
                InitializeSOs(mono, "_highlightColor1", ref _lightColor1, ref _lightColor1_Original, ref _mHighlightColor1);

                if (mono is LightSwitchEventEffect lse)
                {
                    InitializeSOs(mono, "_lightColor0Boost", ref _lightColor0Boost, ref _lightColor0Boost_Original, ref _mLightColor0Boost);
                    InitializeSOs(mono, "_highlightColor0Boost", ref _lightColor0Boost, ref _lightColor0Boost_Original, ref _mHighlightColor0Boost);
                    InitializeSOs(mono, "_lightColor1Boost", ref _lightColor1Boost, ref _lightColor1Boost_Original, ref _mLightColor1Boost);
                    InitializeSOs(mono, "_highlightColor1Boost", ref _lightColor1Boost, ref _lightColor1Boost_Original, ref _mHighlightColor1Boost);
                    _supportBoostColor = true;

                    Lights = lse.GetField<LightWithIdManager, LightSwitchEventEffect>("_lightManager").GetField<List<ILightWithId>[], LightWithIdManager>("_lights")[lse.lightsId];
                    IDictionary<int, List<ILightWithId>> lightsPreGroup = new Dictionary<int, List<ILightWithId>>();
                    TrackLaneRingsManager[] managers = Object.FindObjectsOfType<TrackLaneRingsManager>();
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
                }
            }

            internal List<ILightWithId> Lights { get; private set; }

            internal ILightWithId[][] LightsPropagationGrouped { get; private set; }

            internal static IEnumerable<LSEColorManager> GetLSEColorManager(BeatmapEventType type)
            {
                return _lseColorManagers.Where(n => n._type == type);
            }

            internal static LSEColorManager GetLSEColorManager(MonoBehaviour lse)
            {
                return _lseColorManagers.FirstOrDefault(n => n._lse == lse);
            }

            internal static LSEColorManager CreateLSEColorManager(MonoBehaviour lse, BeatmapEventType type)
            {
                LSEColorManager lsecm;
                lsecm = new LSEColorManager(lse, type);
                _lseColorManagers.Add(lsecm);
                return lsecm;
            }

            internal void Reset()
            {
                _lightColor0.SetColor(_lightColor0_Original);
                _lightColor1.SetColor(_lightColor1_Original);
                if (_supportBoostColor)
                {
                    _lightColor0Boost.SetColor(_lightColor0Boost_Original);
                    _lightColor1Boost.SetColor(_lightColor1Boost_Original);
                }
            }

            internal void SetLightingColors(Color? color0, Color? color1, Color? color0Boost = null, Color? color1Boost = null)
            {
                if (color0.HasValue)
                {
                    _lightColor0.SetColor(color0.Value);
                }

                if (color1.HasValue)
                {
                    _lightColor1.SetColor(color1.Value);
                }

                if (_supportBoostColor)
                {
                    if (color0Boost.HasValue)
                    {
                        _lightColor0Boost.SetColor(color0Boost.Value);
                    }

                    if (color1Boost.HasValue)
                    {
                        _lightColor1Boost.SetColor(color1Boost.Value);
                    }
                }
            }

            internal void SetLastValue(int value)
            {
                _lastValue = value;
            }

            internal void SetActiveColors()
            {
                // Replace with ProcessLightSwitchEvent
                if (_lastValue == 0)
                {
                    return;
                }

                bool warm;
                switch (_lastValue)
                {
                    case 1:
                    case 2:
                    case 3:
                    default:
                        warm = false;
                        break;

                    case 5:
                    case 6:
                    case 7:
                        warm = true;
                        break;
                }

                Color c;
                switch (_lastValue)
                {
                    case 1:
                    case 5:
                    default:
                        c = warm ? _mLightColor0.color : _mLightColor1.color;
                        break;

                    case 2:
                    case 6:
                    case 3:
                    case 7:
                        c = warm ? _mHighlightColor0.color : _mHighlightColor1.color;
                        break;
                }

                if (_lse.enabled)
                {
                    if (_lse is LightSwitchEventEffect l1)
                    {
                        l1.SetField("_highlightColor", c);
                    }
                    else if (_lse is ParticleSystemEventEffect p1)
                    {
                        p1.SetField("_highlightColor", c);
                    }

                    if (_lastValue == 3 || _lastValue == 7)
                    {
                        if (_lse is LightSwitchEventEffect l2)
                        {
                            l2.SetField("_afterHighlightColor", c.ColorWithAlpha(0f));
                        }
                        else if (_lse is ParticleSystemEventEffect p2)
                        {
                            p2.SetField("_afterHighlightColor", c.ColorWithAlpha(0f));
                        }
                    }
                    else
                    {
                        if (_lse is LightSwitchEventEffect l3)
                        {
                            l3.SetField("_afterHighlightColor", c);
                        }
                        else if (_lse is ParticleSystemEventEffect p3)
                        {
                            p3.SetField("_afterHighlightColor", c);
                        }
                    }
                }
                else
                {
                    if (_lastValue == 1 || _lastValue == 5 || _lastValue == 2 || _lastValue == 6)
                    {
                        if (_lse is LightSwitchEventEffect l4)
                        {
                            l4.SetColor(c);
                        }
                        else if (_lse is ParticleSystemEventEffect p4)
                        {
                            p4.SetField("_particleColor", c);
                            p4.RefreshParticles();
                        }
                    }
                }

                if (_lse is LightSwitchEventEffect l5)
                {
                    l5.SetField("_offColor", c.ColorWithAlpha(0f));
                }
                else if (_lse is ParticleSystemEventEffect p5)
                {
                    p5.SetField("_offColor", c.ColorWithAlpha(0f));
                }
            }

            private void InitializeSOs(MonoBehaviour lse, string id, ref SimpleColorSO sColorSO, ref Color originalColor, ref MultipliedColorSO mColorSO)
            {
                MultipliedColorSO lightMultSO = null;
                if (lse is LightSwitchEventEffect l1)
                {
                    lightMultSO = (MultipliedColorSO)l1.GetField<ColorSO, LightSwitchEventEffect>(id);
                }
                else if (lse is ParticleSystemEventEffect p1)
                {
                    lightMultSO = (MultipliedColorSO)p1.GetField<ColorSO, ParticleSystemEventEffect>(id);
                }

                Color multiplierColor = lightMultSO.GetField<Color, MultipliedColorSO>("_multiplierColor");
                SimpleColorSO lightSO = lightMultSO.GetField<SimpleColorSO, MultipliedColorSO>("_baseColor");
                originalColor = lightSO.color;

                if (mColorSO == null)
                {
                    mColorSO = ScriptableObject.CreateInstance<MultipliedColorSO>();
                    mColorSO.SetField("_multiplierColor", multiplierColor);

                    if (sColorSO == null)
                    {
                        sColorSO = ScriptableObject.CreateInstance<SimpleColorSO>();
                        sColorSO.SetColor(originalColor);
                    }

                    mColorSO.SetField("_baseColor", sColorSO);
                }

                if (lse is LightSwitchEventEffect l2)
                {
                    l2.SetField<LightSwitchEventEffect, ColorSO>(id, mColorSO);
                }
                else if (lse is ParticleSystemEventEffect p2)
                {
                    p2.SetField<ParticleSystemEventEffect, ColorSO>(id, mColorSO);
                }
            }
        }
    }
}
