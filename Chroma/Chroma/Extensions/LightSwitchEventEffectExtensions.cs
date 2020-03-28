using BS_Utils.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Extensions
{
    internal static class LightSwitchEventEffectExtensions
    {
        internal static void Reset(this MonoBehaviour lse)
        {
            LSEColourManager.GetLSEColourManager(lse)?.Reset();
        }

        internal static void SetLightingColours(this MonoBehaviour lse, Color? colourA, Color? colourB)
        {
            LSEColourManager.GetLSEColourManager(lse)?.SetLightingColours(colourA, colourB);
        }

        internal static void SetLightingColours(this BeatmapEventType lse, Color? colourA, Color? colourB)
        {
            LSEColourManager.GetLSEColourManager(lse)?.SetLightingColours(colourA, colourB);
        }

        internal static void SetActiveColours(this MonoBehaviour lse, int value)
        {
            LSEColourManager.GetLSEColourManager(lse)?.SetActiveColours(value);
        }

        internal static LightWithId[] GetLights(this LightSwitchEventEffect lse)
        {
            return LSEColourManager.GetLSEColourManager(lse)?.lights.ToArray();
        }

        internal static LightWithId[][] GetLightsPropagationGrouped(this LightSwitchEventEffect lse)
        {
            return LSEColourManager.GetLSEColourManager(lse)?.lightsPropagationGrouped;
        }

        /*
         * LSE ColourSO holders
         */

        internal static void LSEStart(MonoBehaviour lse, BeatmapEventType type)
        {
            LSEColourManager lsecm = LSEColourManager.CreateLSEColourManager(lse, type);
        }

        internal static void LSEDestroy(MonoBehaviour lse, BeatmapEventType type)
        {
            LSEColourManager.GetLSEColourManager(type)?.LSEDestroyed();
        }

        private static List<LSEColourManager> LSEColourManagers = new List<LSEColourManager>();

        private class LSEColourManager
        {
            internal static LSEColourManager GetLSEColourManager(BeatmapEventType type)
            {
                for (int i = 0; i < LSEColourManagers.Count; i++)
                {
                    if (LSEColourManagers[i].type == type) return LSEColourManagers[i];
                }
                return null;
            }

            internal static LSEColourManager GetLSEColourManager(MonoBehaviour lse)
            {
                for (int i = 0; i < LSEColourManagers.Count; i++)
                {
                    if (LSEColourManagers[i].lse == lse) return LSEColourManagers[i];
                }
                return null;
            }

            internal static LSEColourManager CreateLSEColourManager(MonoBehaviour lse, BeatmapEventType type)
            {
                LSEColourManager lsecm;
                lsecm = GetLSEColourManager(type);

                lsecm = new LSEColourManager(lse, type);
                lsecm.Initialize(lse, type);
                LSEColourManagers.Add(lsecm);
                return lsecm;
            }

            private MonoBehaviour lse;
            private BeatmapEventType type;

            private Color _lightColor0_Original;
            private Color _highlightColor0_Original;
            private Color _lightColor1_Original;
            private Color _highlightColor1_Original;

            private SimpleColorSO _lightColor0;
            private SimpleColorSO _highlightColor0;
            private SimpleColorSO _lightColor1;
            private SimpleColorSO _highlightColor1;

            private MultipliedColorSO m_lightColor0;
            private MultipliedColorSO m_highlightColor0;
            private MultipliedColorSO m_lightColor1;
            private MultipliedColorSO m_highlightColor1;

            internal List<LightWithId> lights { get; private set; }
            internal LightWithId[][] lightsPropagationGrouped { get; private set; }

            private LSEColourManager(MonoBehaviour lse, BeatmapEventType type)
            {
                Initialize(lse, type);
            }

            private void Initialize(MonoBehaviour mono, BeatmapEventType type)
            {
                this.lse = mono;
                this.type = type;
                InitializeSOs(mono, "_lightColor0", ref _lightColor0, ref _lightColor0_Original, ref m_lightColor0);
                InitializeSOs(mono, "_highlightColor0", ref _highlightColor0, ref _highlightColor0_Original, ref m_highlightColor0);
                InitializeSOs(mono, "_lightColor1", ref _lightColor1, ref _lightColor1_Original, ref m_lightColor1);
                InitializeSOs(mono, "_highlightColor1", ref _highlightColor1, ref _highlightColor1_Original, ref m_highlightColor1);

                if (!(mono is LightSwitchEventEffect))
                {
                    Reset();
                    return;
                }
                LightSwitchEventEffect lse = (LightSwitchEventEffect)mono;
                lights = lse.GetPrivateField<LightWithIdManager>("_lightManager").GetPrivateField<List<LightWithId>[]>("_lights")[lse.LightsID];
                Dictionary<int, List<LightWithId>> lightsPreGroup = new Dictionary<int, List<LightWithId>>();
                foreach (LightWithId light in lights)
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
                lightsPropagationGrouped = new LightWithId[lightsPreGroup.Count][];
                int i = 0;
                foreach (List<LightWithId> lightList in lightsPreGroup.Values)
                {
                    if (lightList is null) continue;
                    lightsPropagationGrouped[i] = lightList.ToArray();
                    i++;
                }

                Reset();
            }

            //We still need to do the first half of this even if the LSECM already exists as custom map colours exist and we need to be able to know the default colour
            private void InitializeSOs(MonoBehaviour lse, string id, ref SimpleColorSO sColorSO, ref Color originalColour, ref MultipliedColorSO mColorSO)
            {
                MultipliedColorSO lightMultSO = lse.GetPrivateField<MultipliedColorSO>(id);
                Color multiplierColour = lightMultSO.GetPrivateField<Color>("_multiplierColor");
                SimpleColorSO lightSO = lightMultSO.GetPrivateField<SimpleColorSO>("_baseColor");
                originalColour = lightSO.color;

                if (mColorSO == null)
                {
                    mColorSO = ScriptableObject.CreateInstance<MultipliedColorSO>();
                    mColorSO.SetPrivateField("_multiplierColor", multiplierColour);

                    sColorSO = ScriptableObject.CreateInstance<SimpleColorSO>();
                    sColorSO.SetColor(originalColour);
                    mColorSO.SetPrivateField("_baseColor", sColorSO);
                }

                lse.SetPrivateField(id, mColorSO);
            }

            internal void LSEDestroyed()
            {
                lse = null;
            }

            internal void Reset()
            {
                _lightColor0.SetColor(_lightColor0_Original);
                _highlightColor0.SetColor(_highlightColor0_Original);
                _lightColor1.SetColor(_lightColor1_Original);
                _highlightColor1.SetColor(_highlightColor1_Original);
            }

            internal void SetLightingColours(Color? colourA, Color? colourB)
            {
                if (colourB.HasValue)
                {
                    _lightColor0.SetColor(colourB.Value);
                    _highlightColor0.SetColor(colourB.Value);
                }
                if (colourA.HasValue)
                {
                    _lightColor1.SetColor(colourA.Value);
                    _highlightColor1.SetColor(colourA.Value);
                }
            }

            internal void SetActiveColours(int value)
            {
                if (value == 0) return;

                bool warm;
                switch (value)
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
                switch (value)
                {
                    case 1:
                    case 5:
                    default:
                        c = warm ? _lightColor0.color : _lightColor1.color;
                        break;

                    case 2:
                    case 6:
                    case 3:
                    case 7:
                        c = warm ? _highlightColor0.color : _highlightColor1.color;
                        break;
                }
                if (lse.enabled)
                {
                    lse.SetPrivateField("_highlightColor", c);
                    if (value == 3 || value == 7) lse.SetPrivateField("_afterHighlightColor", c.ColorWithAlpha(0f));
                    else lse.SetPrivateField("_afterHighlightColor", c);
                }
                else
                {
                    if (value == 1 || value == 5 || value == 2 || value == 6)
                    {
                        if (lse is LightSwitchEventEffect mono)
                            mono.SetColor(c);
                        else
                            lse.SetPrivateField("_particleColor", c);
                    }
                }
                lse.SetPrivateField("_offColor", c.ColorWithAlpha(0f));
            }
        }
    }
}