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
            if (knownLSEs.TryGetValue(lse, out LSEHolder holder)) holder.Reset();
        }

        public static void SetLightingColourA(this LightSwitchEventEffect lse, Color colour) {
            lse.SetLightingColours(colour, Color.clear);
        }

        public static void SetLightingColourB(this LightSwitchEventEffect lse, Color colour) {
            lse.SetLightingColours(Color.clear, colour);
        }

        public static void SetLightingColours(this LightSwitchEventEffect lse, Color colourA, Color colourB) {
            if (knownLSEs.TryGetValue(lse, out LSEHolder holder)) holder.SetLightingColours(colourA, colourB);
        }

        /*
         * LSE ColourSO holders
         */

        internal static void LSEStart(LightSwitchEventEffect lse) {
            knownLSEs.Add(lse, new LSEHolder(lse));
        }

        internal static void LSEDestroy(LightSwitchEventEffect lse) {
            knownLSEs.Remove(lse);
        }

        private static Dictionary<LightSwitchEventEffect, LSEHolder> knownLSEs = new Dictionary<LightSwitchEventEffect, LSEHolder>();

        private class LSEHolder {
            public LightSwitchEventEffect lse;

            public Color _lightColor0_Original;
            public Color _highlightColor0_Original;
            public Color _lightColor1_Original;
            public Color _highlightColor1_Original;

            public SimpleColorSO _lightColor0;
            public SimpleColorSO _highlightColor0;
            public SimpleColorSO _lightColor1;
            public SimpleColorSO _highlightColor1;

            public LSEHolder(LightSwitchEventEffect lse) {
                this.lse = lse;

                /*
                MultipliedColorSO mColorSO = lse.GetField<MultipliedColorSO>("_lightColor0");
                _lightColor0 = mColorSO.GetField<SimpleColorSO>("_baseColor");
                _lightColor0_Original = _lightColor0.color;

                mColorSO = lse.GetField<MultipliedColorSO>("_highlightColor0");
                _highlightColor0 = mColorSO.GetField<SimpleColorSO>("_baseColor");
                _highlightColor0_Original = _highlightColor0.color;

                mColorSO = lse.GetField<MultipliedColorSO>("_lightColor1");
                _lightColor1 = mColorSO.GetField<SimpleColorSO>("_baseColor");
                _lightColor1_Original = _lightColor1.color;

                mColorSO = lse.GetField<MultipliedColorSO>("_highlightColor1");
                _highlightColor1 = mColorSO.GetField<SimpleColorSO>("_baseColor");
                _highlightColor1_Original = _highlightColor1.color;
                */

                /*
                //0 is blue
                MultipliedColorSO mColorSO = lse.GetField<MultipliedColorSO>("_lightColor0");
                _lightColor0 = mColorSO.GetField<SimpleColorSO>("_baseColor");
                _lightColor0_Original = _lightColor0.color;
                mColorSO = ScriptableObject.CreateInstance<MultipliedColorSO>();
                lse.SetField("_lightColor0", mColorSO);
                _lightColor0 = ScriptableObject.CreateInstance<SimpleColorSO>();
                _lightColor0.SetColor(_lightColor0_Original);
                mColorSO.SetField("_baseColor", _lightColor0);

                mColorSO = lse.GetField<MultipliedColorSO>("_highlightColor0");
                _highlightColor0 = mColorSO.GetField<SimpleColorSO>("_baseColor");
                _highlightColor0_Original = _highlightColor0.color;
                mColorSO = ScriptableObject.CreateInstance<MultipliedColorSO>();
                lse.SetField("_highlightColor0", mColorSO);
                _highlightColor0 = ScriptableObject.CreateInstance<SimpleColorSO>();
                _highlightColor0.SetColor(_highlightColor0_Original);
                mColorSO.SetField("_baseColor", _highlightColor0);

                //1 is red
                mColorSO = lse.GetField<MultipliedColorSO>("_lightColor1");
                _lightColor1 = mColorSO.GetField<SimpleColorSO>("_baseColor");
                _lightColor1_Original = _lightColor1.color;
                mColorSO = ScriptableObject.CreateInstance<MultipliedColorSO>();
                lse.SetField("_lightColor1", mColorSO);
                _lightColor1 = ScriptableObject.CreateInstance<SimpleColorSO>();
                _lightColor1.SetColor(_lightColor1_Original);
                mColorSO.SetField("_baseColor", _lightColor1);

                mColorSO = lse.GetField<MultipliedColorSO>("_highlightColor1");
                _highlightColor1 = mColorSO.GetField<SimpleColorSO>("_baseColor");
                _highlightColor1_Original = _highlightColor1.color;
                mColorSO = ScriptableObject.CreateInstance<MultipliedColorSO>();
                lse.SetField("_highlightColor1", mColorSO);
                _highlightColor1 = ScriptableObject.CreateInstance<SimpleColorSO>();
                _highlightColor1.SetColor(_highlightColor1_Original);
                mColorSO.SetField("_baseColor", _highlightColor1);
                */

                SetupNewSO(lse, "_lightColor0", ref _lightColor0, ref _lightColor0_Original);
                SetupNewSO(lse, "_highlightColor0", ref _highlightColor0, ref _highlightColor0_Original);
                SetupNewSO(lse, "_lightColor1", ref _lightColor1, ref _lightColor1_Original);
                SetupNewSO(lse, "_highlightColor1", ref _highlightColor1, ref _highlightColor1_Original);

                ChromaLogger.Log("Did the thing");
            }

            private void SetupNewSO(LightSwitchEventEffect lse, string id, ref SimpleColorSO sColorSO, ref Color originalColour) {
                MultipliedColorSO mColorSO = lse.GetField<MultipliedColorSO>(id);
                Color multiplierColour = mColorSO.GetField<Color>("_multiplierColor");
                SimpleColorSO lightSO = mColorSO.GetField<SimpleColorSO>("_baseColor");
                originalColour = lightSO.color;

                mColorSO = ScriptableObject.CreateInstance<MultipliedColorSO>();
                mColorSO.SetField("_multiplierColor", multiplierColour);

                sColorSO = ScriptableObject.CreateInstance<SimpleColorSO>();
                sColorSO.SetColor(originalColour);
                mColorSO.SetField("_baseColor", sColorSO);

                lse.SetField(id, mColorSO);
            }

            public void Reset() {
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
                    _lightColor1.SetColor(ColourManager.LightB);
                    _highlightColor1.SetColor(ColourManager.LightB);
                }
            }

            public void SetLightingColours(Color colourA, Color colourB) {
                if (colourA != Color.clear) {
                    _lightColor0.SetColor(colourA);
                    _highlightColor0.SetColor(colourA);
                }
                if (colourB != Color.clear) {
                    _lightColor1.SetColor(colourB);
                    _highlightColor1.SetColor(colourB);
                }
            }
        }

        internal static void RememberDefaultColours(LightSwitchEventEffect lse) {

        }

        internal static void GetDefaultColours() {

        }

    }

}
