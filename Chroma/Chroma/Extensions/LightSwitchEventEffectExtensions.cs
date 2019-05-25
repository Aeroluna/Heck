using Chroma.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Extensions {

    public static class LightSwitchEventEffectExtensions {

        public static void Reset(this LightSwitchEventEffect lse) {
            LSEColourManager.GetLSEColourManager(lse)?.Reset();
        }

        public static void SetLightingColourA(this LightSwitchEventEffect lse, Color colour) {
            lse.SetLightingColours(colour, Color.clear);
        }

        public static void SetLightingColourB(this LightSwitchEventEffect lse, Color colour) {
            lse.SetLightingColours(Color.clear, colour);
        }

        public static void SetLightingColours(this LightSwitchEventEffect lse, Color colourA, Color colourB) {
            LSEColourManager.GetLSEColourManager(lse)?.SetLightingColours(colourA, colourB);
        }

        /*
         * LSE ColourSO holders
         */

        internal static void LSEStart(LightSwitchEventEffect lse, BeatmapEventType type) {
            LSEColourManager lsecm = LSEColourManager.GetOrCreateLSEColourManager(lse, type);
            if (type == BeatmapEventType.Event1) {
                ChromaTesting.lse = lse; ChromaTesting.type = type;
            }
        }

        internal static void LSEDestroy(LightSwitchEventEffect lse, BeatmapEventType type) {
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

            public static LSEColourManager GetLSEColourManager(LightSwitchEventEffect lse) {
                for (int i = 0; i < LSEColourManagers.Count; i++) {
                    if (LSEColourManagers[i].lse == lse) return LSEColourManagers[i];
                }
                return null;
            }

            public static LSEColourManager GetOrCreateLSEColourManager(LightSwitchEventEffect lse, BeatmapEventType type) {
                LSEColourManager lsecm;
                try {
                    lsecm = GetLSEColourManager(type);
                } catch (Exception e) {
                    ChromaLogger.Log(e);
                    return null;
                }
                try {
                    if (lsecm != null) {
                        lsecm.Initialize(lse, type);
                        return lsecm;
                    } else {
                        lsecm = new LSEColourManager(lse, type);
                        lsecm.Initialize(lse, type);
                        LSEColourManagers.Add(lsecm);
                        return lsecm;
                    }
                } catch (Exception e) {
                    ChromaLogger.Log(e);
                    return lsecm;
                }
            }

            public LightSwitchEventEffect lse;
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

            private LSEColourManager(LightSwitchEventEffect lse, BeatmapEventType type) {
                Initialize(lse, type);
            }

            private void Initialize(LightSwitchEventEffect lse, BeatmapEventType type) {
                this.lse = lse;
                this.type = type;
                InitializeSOs(lse, "_lightColor0", ref _lightColor0, ref _lightColor0_Original, ref m_lightColor0);
                InitializeSOs(lse, "_highlightColor0", ref _highlightColor0, ref _highlightColor0_Original, ref m_highlightColor0);
                InitializeSOs(lse, "_lightColor1", ref _lightColor1, ref _lightColor1_Original, ref m_lightColor1);
                InitializeSOs(lse, "_highlightColor1", ref _highlightColor1, ref _highlightColor1_Original, ref m_highlightColor1);
                Reset();
            }

            //We still need to do the first half of this even if the LSECM already exists as custom map colours exist and we need to be able to know the default colour
            private void InitializeSOs(LightSwitchEventEffect lse, string id, ref SimpleColorSO sColorSO, ref Color originalColour, ref MultipliedColorSO mColorSO) {
                MultipliedColorSO lightMultSO = lse.GetField<MultipliedColorSO>(id);
                Color multiplierColour = lightMultSO.GetField<Color>("_multiplierColor");
                SimpleColorSO lightSO = lightMultSO.GetField<SimpleColorSO>("_baseColor");
                originalColour = lightSO.color;

                if (mColorSO == null) {
                    mColorSO = ScriptableObject.CreateInstance<MultipliedColorSO>();
                    mColorSO.SetField("_multiplierColor", multiplierColour);

                    sColorSO = ScriptableObject.CreateInstance<SimpleColorSO>();
                    sColorSO.SetColor(originalColour);
                    mColorSO.SetField("_baseColor", sColorSO);
                }

                lse.SetField(id, mColorSO);
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
        }

    }

}
