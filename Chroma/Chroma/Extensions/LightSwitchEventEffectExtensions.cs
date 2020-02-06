using Chroma.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using IPA.Utilities;

namespace Chroma.Extensions {

    public static class LightSwitchEventEffectExtensions {

        public static void Reset(this MonoBehaviour lse) {
            LSEColourManager.GetLSEColourManager(lse)?.Reset();
        }

        public static Color? GetLightingColourA(this MonoBehaviour lse) {
            return LSEColourManager.GetLSEColourManager(lse)?.GetLightingColourA();
        }

        public static Color? GetLightingColourB(this MonoBehaviour lse) {
            return LSEColourManager.GetLSEColourManager(lse)?.GetLightingColourB();
        }

        public static void SetLightingColourA(this MonoBehaviour lse, Color colour) {
            lse.SetLightingColours(colour, Color.clear);
        }

        public static void SetLightingColourB(this MonoBehaviour lse, Color colour) {
            lse.SetLightingColours(Color.clear, colour);
        }

        public static void SetLightingColours(this MonoBehaviour lse, Color colourA, Color colourB) {
            LSEColourManager.GetLSEColourManager(lse)?.SetLightingColours(colourA, colourB);
        }

        public static void SetLightingColours(this BeatmapEventType lse, Color colourA, Color colourB) {
            LSEColourManager.GetLSEColourManager(lse)?.SetLightingColours(colourA, colourB);
        }

        public static LightWithId[] GetLights(this LightSwitchEventEffect lse) {
            return LSEColourManager.GetLSEColourManager(lse)?.lights.ToArray();
        }

        public static LightWithId[][] GetLightsPropagationGrouped(this LightSwitchEventEffect lse) {
            return LSEColourManager.GetLSEColourManager(lse)?.lightsPropagationGrouped;
        }

        /*
         * LSE ColourSO holders
         */

        internal static void LSEStart(MonoBehaviour lse, BeatmapEventType type) {
            LSEColourManager lsecm = LSEColourManager.CreateLSEColourManager(lse, type);
            /*if (type == BeatmapEventType.Event1) {
                ChromaTesting.lse = lse; ChromaTesting.type = type;
            }*/
        }

        internal static void LSEDestroy(MonoBehaviour lse, BeatmapEventType type) {
            LSEColourManager.GetLSEColourManager(type)?.LSEDestroyed();
        }
        
        private static List<LSEColourManager> LSEColourManagers = new List<LSEColourManager>();

        private class LSEColourManager {

            public static LSEColourManager GetLSEColourManager(BeatmapEventType type) {
                for (int i = 0; i < LSEColourManagers.Count; i++) {
                    if (LSEColourManagers[i].type == type) return LSEColourManagers[i];
                }
                return null;
            }

            public static LSEColourManager GetLSEColourManager(MonoBehaviour lse) {
                for (int i = 0; i < LSEColourManagers.Count; i++) {
                    if (LSEColourManagers[i].lse == lse) return LSEColourManagers[i];
                }
                return null;
            }

            public static LSEColourManager CreateLSEColourManager(MonoBehaviour lse, BeatmapEventType type) {
                LSEColourManager lsecm;
                try {
                    lsecm = GetLSEColourManager(type);
                } catch (Exception e) {
                    ChromaLogger.Log(e);
                    return null;
                }
                try {
                    lsecm = new LSEColourManager(lse, type);
                    lsecm.Initialize(lse, type);
                    LSEColourManagers.Add(lsecm);
                    return lsecm;
                } catch (Exception e) {
                    ChromaLogger.Log(e);
                    return lsecm;
                }
            }

            public MonoBehaviour lse;
            public BeatmapEventType type;

            public Color _lightColor0_Original;
            public Color _highlightColor0_Original;
            public Color _lightColor1_Original;
            public Color _highlightColor1_Original;

            public SimpleColorSO _lightColor0;
            public SimpleColorSO _highlightColor0;
            public SimpleColorSO _lightColor1;
            public SimpleColorSO _highlightColor1;

            public MultipliedColorSO m_lightColor0;
            public MultipliedColorSO m_highlightColor0;
            public MultipliedColorSO m_lightColor1;
            public MultipliedColorSO m_highlightColor1;

            public List<LightWithId> lights;
            public LightWithId[][] lightsPropagationGrouped;

            private LSEColourManager(MonoBehaviour lse, BeatmapEventType type) {
                Initialize(lse, type);
            }

            private void Initialize(MonoBehaviour mono, BeatmapEventType type) {
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
                foreach (LightWithId light in lights) {
                    int z = Mathf.RoundToInt(light.transform.position.z);
                    if (lightsPreGroup.TryGetValue(z, out List<LightWithId> list)) {
                        list.Add(light);
                    } else {
                        list = new List<LightWithId>();
                        list.Add(light);
                        lightsPreGroup.Add(z, list);
                    }
                }
                lightsPropagationGrouped = new LightWithId[lightsPreGroup.Count][];
                int i = 0;
                foreach (List<LightWithId> lightList in lightsPreGroup.Values) {
                    if (lightList is null) continue;
                    lightsPropagationGrouped[i] = lightList.ToArray();
                    i++;
                }

                Reset();
            }

            //We still need to do the first half of this even if the LSECM already exists as custom map colours exist and we need to be able to know the default colour
            private void InitializeSOs(MonoBehaviour lse, string id, ref SimpleColorSO sColorSO, ref Color originalColour, ref MultipliedColorSO mColorSO) {
                //ChromaLogger.Log(lse.GetField<ColorSO>(id).GetType().Name, ChromaLogger.Level.ERROR, false);
                MultipliedColorSO lightMultSO = lse.GetPrivateField<MultipliedColorSO>(id);
                Color multiplierColour = lightMultSO.GetPrivateField<Color>("_multiplierColor");
                SimpleColorSO lightSO = lightMultSO.GetPrivateField<SimpleColorSO>("_baseColor");
                originalColour = lightSO.color;

                if (mColorSO == null) {
                    mColorSO = ScriptableObject.CreateInstance<MultipliedColorSO>();
                    mColorSO.SetPrivateField("_multiplierColor", multiplierColour);

                    sColorSO = ScriptableObject.CreateInstance<SimpleColorSO>();
                    sColorSO.SetColor(originalColour);
                    mColorSO.SetPrivateField("_baseColor", sColorSO);
                }

                lse.SetPrivateField(id, mColorSO);
            }

            internal void LSEDestroyed() {
                this.lse = null;
            }

            internal void Reset() {
                if (ColourManager.LightB == Color.clear) {
                    _lightColor0.SetColor(_lightColor0_Original);
                    _highlightColor0.SetColor(_highlightColor0_Original);
                } else {
                    _lightColor0.SetColor(ColourManager.LightB);
                    _highlightColor0.SetColor(ColourManager.LightB);
                }
                if (ColourManager.LightA == Color.clear) {
                    _lightColor1.SetColor(_lightColor1_Original);
                    _highlightColor1.SetColor(_highlightColor1_Original);
                } else {
                    _lightColor1.SetColor(ColourManager.LightA);
                    _highlightColor1.SetColor(ColourManager.LightA);
                }
            }

            internal void SetLightingColours(Color colourA, Color colourB) {
                if (colourB != Color.clear) {
                    _lightColor0.SetColor(colourB);
                    _highlightColor0.SetColor(colourB);
                }
                if (colourA != Color.clear) {
                    _lightColor1.SetColor(colourA);
                    _highlightColor1.SetColor(colourA);
                }
            }

            internal Color GetLightingColourA() {
                return _lightColor1;
            }

            internal Color GetLightingColourB() {
                return _lightColor0;
            }
        }

    }

}
