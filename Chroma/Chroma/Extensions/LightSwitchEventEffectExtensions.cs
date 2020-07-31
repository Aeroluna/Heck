namespace Chroma.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using IPA.Utilities;
    using UnityEngine;

    internal static class LightSwitchEventEffectExtensions
    {
        private static List<LSEColorManager> _lseColorManagers = new List<LSEColorManager>();

        internal static void Reset(this MonoBehaviour lse)
        {
            LSEColorManager.GetLSEColorManager(lse)?.Reset();
        }

        internal static void ResetAllLightingColors()
        {
            _lseColorManagers.ForEach(n => n.Reset());
        }

        internal static void SetLightingColors(this MonoBehaviour lse, Color? colorA, Color? colorB)
        {
            LSEColorManager.GetLSEColorManager(lse)?.SetLightingColors(colorA, colorB);
        }

        internal static void SetLightingColors(this BeatmapEventType lse, Color? colorA, Color? colorB)
        {
            foreach (LSEColorManager l in LSEColorManager.GetLSEColorManager(lse))
            {
                l.SetLightingColors(colorA, colorB);
            }
        }

        internal static void SetAllLightingColors(Color? colorA, Color? colorB)
        {
            _lseColorManagers.ForEach(n => n.SetLightingColors(colorA, colorB));
        }

        internal static void SetActiveColors(this BeatmapEventType lse)
        {
            foreach (LSEColorManager l in LSEColorManager.GetLSEColorManager(lse))
            {
                l.SetActiveColors();
            }
        }

        internal static void SetAllActiveColors()
        {
            _lseColorManagers.ForEach(n => n.SetActiveColors());
        }

        internal static void SetLastValue(this MonoBehaviour lse, int value)
        {
            LSEColorManager.GetLSEColorManager(lse)?.SetLastValue(value);
        }

        internal static LightWithId[] GetLights(this LightSwitchEventEffect lse)
        {
            return LSEColorManager.GetLSEColorManager(lse)?.Lights.ToArray();
        }

        internal static LightWithId[][] GetLightsPropagationGrouped(this LightSwitchEventEffect lse)
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

        internal static void LSEDestroy(MonoBehaviour lse, BeatmapEventType type)
        {
            LSEColorManager[] managerList = LSEColorManager.GetLSEColorManager(type).ToArray();
            for (int i = 0; i < managerList.Length; i++)
            {
                managerList[i].LSEDestroyed();
            }
        }

        private class LSEColorManager
        {
            private MonoBehaviour _lse;
            private BeatmapEventType _type;

            private Color _lightColor0_Original;
            private Color _highlightColor0_Original;
            private Color _lightColor1_Original;
            private Color _highlightColor1_Original;

            private SimpleColorSO _lightColor0;
            private SimpleColorSO _highlightColor0;
            private SimpleColorSO _lightColor1;
            private SimpleColorSO _highlightColor1;

            private MultipliedColorSO _mLightColor0;
            private MultipliedColorSO _mHighlightColor0;
            private MultipliedColorSO _mLightColor1;
            private MultipliedColorSO _mHighlightColor1;

            private float _lastValue;

            private LSEColorManager(MonoBehaviour mono, BeatmapEventType type)
            {
                _lse = mono;
                _type = type;
                InitializeSOs(mono, "_lightColor0", ref _lightColor0, ref _lightColor0_Original, ref _mLightColor0);
                InitializeSOs(mono, "_highlightColor0", ref _highlightColor0, ref _highlightColor0_Original, ref _mHighlightColor0);
                InitializeSOs(mono, "_lightColor1", ref _lightColor1, ref _lightColor1_Original, ref _mLightColor1);
                InitializeSOs(mono, "_highlightColor1", ref _highlightColor1, ref _highlightColor1_Original, ref _mHighlightColor1);

                if (!(mono is LightSwitchEventEffect))
                {
                    return;
                }

                LightSwitchEventEffect lse = (LightSwitchEventEffect)mono;
                Lights = lse.GetField<LightWithIdManager, LightSwitchEventEffect>("_lightManager").GetField<List<LightWithId>[], LightWithIdManager>("_lights")[lse.LightsID];
                Dictionary<int, List<LightWithId>> lightsPreGroup = new Dictionary<int, List<LightWithId>>();
                foreach (LightWithId light in Lights)
                {
                    int z = Mathf.RoundToInt(light.transform.position.z);
                    if (lightsPreGroup.TryGetValue(z, out List<LightWithId> list))
                    {
                        list.Add(light);
                    }
                    else
                    {
                        list = new List<LightWithId>();
                        list.Add(light);
                        lightsPreGroup.Add(z, list);
                    }
                }

                LightsPropagationGrouped = new LightWithId[lightsPreGroup.Count][];
                int i = 0;
                foreach (List<LightWithId> lightList in lightsPreGroup.Values)
                {
                    if (lightList is null)
                    {
                        continue;
                    }

                    LightsPropagationGrouped[i] = lightList.ToArray();
                    i++;
                }
            }

            internal List<LightWithId> Lights { get; private set; }

            internal LightWithId[][] LightsPropagationGrouped { get; private set; }

            internal static IEnumerable<LSEColorManager> GetLSEColorManager(BeatmapEventType type)
            {
                return _lseColorManagers.Where(n => n._type == type);
            }

            internal static LSEColorManager GetLSEColorManager(MonoBehaviour lse)
            {
                for (int i = 0; i < _lseColorManagers.Count; i++)
                {
                    if (_lseColorManagers[i]._lse == lse)
                    {
                        return _lseColorManagers[i];
                    }
                }

                return null;
            }

            internal static LSEColorManager CreateLSEColorManager(MonoBehaviour lse, BeatmapEventType type)
            {
                LSEColorManager lsecm;
                lsecm = new LSEColorManager(lse, type);
                _lseColorManagers.Add(lsecm);
                return lsecm;
            }

            internal void LSEDestroyed()
            {
                _lseColorManagers.Remove(this);
            }

            internal void Reset()
            {
                _lightColor0.SetColor(_lightColor0_Original);
                _highlightColor0.SetColor(_highlightColor0_Original);
                _lightColor1.SetColor(_lightColor1_Original);
                _highlightColor1.SetColor(_highlightColor1_Original);
            }

            internal void SetLightingColors(Color? colorA, Color? colorB)
            {
                if (colorA.HasValue)
                {
                    _lightColor0.SetColor(colorA.Value);
                    _highlightColor0.SetColor(colorA.Value);
                }

                if (colorB.HasValue)
                {
                    _lightColor1.SetColor(colorB.Value);
                    _highlightColor1.SetColor(colorB.Value);
                }
            }

            internal void SetLastValue(int value)
            {
                _lastValue = value;
            }

            internal void SetActiveColors()
            {
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
                        if (_lse is LightSwitchEventEffect mono)
                        {
                            mono.SetColor(c);
                        }
                        else
                        {
                            ((ParticleSystemEventEffect)_lse).SetField("_particleColor", c);
                        }
                    }
                }

                if (_lse is LightSwitchEventEffect l4)
                {
                    l4.SetField("_offColor", c.ColorWithAlpha(0f));
                }
                else if (_lse is ParticleSystemEventEffect p4)
                {
                    p4.SetField("_offColor", c.ColorWithAlpha(0f));
                }
            }

            // We still need to do the first half of this even if the LSECM already exists as custom map colors exist and we need to be able to know the default color
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

                    sColorSO = ScriptableObject.CreateInstance<SimpleColorSO>();
                    sColorSO.SetColor(originalColor);
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
