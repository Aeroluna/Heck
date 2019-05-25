using Chroma.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Colours {

    public static class TechnicolourManager {
        
        /*private static Color[] _technicolourWarmPalette;
        private static Color[] _technicolourColdPalette;
        private static Color[] _technicolourCombinedPalette;

        public static Color[] TechnicolourCombinedPalette {
            get { return _technicolourCombinedPalette; }
        }
        public static Color[] TechnicolourWarmPalette {
            get { return _technicolourWarmPalette; }
            set {
                _technicolourWarmPalette = value;
                SetupCombinedTechnicolourPalette();
            }
        }
        public static Color[] TechnicolourColdPalette {
            get { return _technicolourColdPalette; }
            set {
                _technicolourColdPalette = value;
                SetupCombinedTechnicolourPalette();
            }
        }

        private static void SetupCombinedTechnicolourPalette() {
            if (_technicolourColdPalette == null || _technicolourWarmPalette == null) return;
            Color[] newCombined = new Color[_technicolourColdPalette.Length + _technicolourWarmPalette.Length];
            for (int i = 0; i < _technicolourColdPalette.Length; i++) newCombined[i] = _technicolourColdPalette[i];
            for (int i = 0; i < _technicolourWarmPalette.Length; i++) newCombined[_technicolourColdPalette.Length + i] = _technicolourWarmPalette[i];
            System.Random shuffleRandom = new System.Random();
            _technicolourCombinedPalette = newCombined.OrderBy(x => shuffleRandom.Next()).ToArray();
            ChromaLogger.Log("Combined TC Palette formed : " + _technicolourCombinedPalette.Length);
        }*/

        /*public enum TechnicolourStyle {
            OFF = 0,
            WARM_COLD = 1,
            ANY_PALETTE = 2,
            PURE_RANDOM = 3
        }

        public enum TechnicolourTransition {
            FLAT = 0,
            SMOOTH = 1,
        }*/

        public enum TechnicolourLightsGrouping {
            STANDARD = 0,
            ISOLATED = 1
        }

        /*public static TechnicolourStyle GetTechnicolourStyleFromFloat(float f) {
            if (f == 1) return TechnicolourStyle.WARM_COLD;
            else if (f == 2) return TechnicolourStyle.ANY_PALETTE;
            else if (f == 3) return TechnicolourStyle.PURE_RANDOM;
            else return TechnicolourStyle.OFF;
        }*/

        public static TechnicolourLightsGrouping GetTechnicolourLightsGroupingFromFloat(float f) {
            if (f == 1) return TechnicolourLightsGrouping.ISOLATED;
            else return TechnicolourLightsGrouping.STANDARD;
        }

        private static bool technicolourLightsForceDisabled = false;
        public static bool TechnicolourLightsForceDisabled {
            get { return technicolourLightsForceDisabled; }
            set {
                technicolourLightsForceDisabled = value;
            }
        }

        /*public static TechnicolourTransition _technicolourLightsTransition = TechnicolourTransition.FLAT;
        public static TechnicolourTransition _technicolourSabersTransition = TechnicolourTransition.FLAT;
        public static TechnicolourTransition _technicolourBlocksTransition = TechnicolourTransition.FLAT;
        public static TechnicolourTransition _technicolourWallsTransition = TechnicolourTransition.SMOOTH;*/

        /*public static bool TechnicolourLights {
            get {
                return !ChromaBehaviour.IsLoadingSong && !technicolourLightsForceDisabled && ChromaConfig.TechnicolourEnabled && ChromaConfig.TechnicolourLightsStyle != TechnicolourStyle.OFF;
            }
        }

        public static bool TechnicolourSabers {
            get { return ChromaConfig.TechnicolourEnabled && ChromaConfig.TechnicolourSabersStyle != TechnicolourStyle.OFF; }
        }

        public static bool TechnicolourBlocks {
            get { return ChromaConfig.TechnicolourEnabled && ChromaConfig.TechnicolourBlocksStyle != TechnicolourStyle.OFF; }
        }

        public static bool TechnicolourBarriers {
            get { return ChromaConfig.TechnicolourEnabled && ChromaConfig.TechnicolourWallsStyle != TechnicolourStyle.OFF; }
        }

        public static Color GetTechnicolour(NoteData noteData, TechnicolourStyle style) {
            return GetTechnicolour(noteData.noteType == NoteType.NoteA, noteData.time + noteData.lineIndex + (int)noteData.noteLineLayer, style);
        }

        public static Color GetTechnicolour(float time, TechnicolourStyle style, TechnicolourTransition transition = TechnicolourTransition.FLAT) {
            return GetTechnicolour(true, time, style, transition);
        }

        public static Color GetTechnicolour(bool warm, float time, TechnicolourStyle style, TechnicolourTransition transition = TechnicolourTransition.FLAT) {
            switch (style) {
                case TechnicolourStyle.ANY_PALETTE:
                    return GetEitherTechnicolour(time, transition);
                case TechnicolourStyle.PURE_RANDOM:
                    return Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f); //UnityEngine.Random.ColorHSV().ColorWithAlpha(1f);
                case TechnicolourStyle.WARM_COLD:
                    return warm ? GetWarmTechnicolour(time, transition) : GetColdTechnicolour(time, transition);
                default: return Color.clear;
            }
        }

        public static Color GetEitherTechnicolour(float time, TechnicolourTransition transition) {
            //System.Random rand = new System.Random(Mathf.FloorToInt(8*time));
            //return rand.NextDouble() < 0.5 ? GetWarmTechnicolour(time) : GetColdTechnicolour(time);
            switch (transition) {
                case TechnicolourTransition.FLAT:
                    return GetRandomFromArray(TechnicolourCombinedPalette, time);
                case TechnicolourTransition.SMOOTH:
                    return GetLerpedFromArray(TechnicolourCombinedPalette, time);
                default:
                    return Color.white;
            }
        }

        public static Color GetWarmTechnicolour(float time, TechnicolourTransition transition) {
            switch (transition) {
                case TechnicolourTransition.FLAT:
                    return GetRandomFromArray(TechnicolourWarmPalette, time);
                case TechnicolourTransition.SMOOTH:
                    return GetLerpedFromArray(TechnicolourWarmPalette, time);
                default:
                    return Color.white;
            }
        }

        public static Color GetColdTechnicolour(float time, TechnicolourTransition transition) {
            switch (transition) {
                case TechnicolourTransition.FLAT:
                    return GetRandomFromArray(TechnicolourColdPalette, time);
                case TechnicolourTransition.SMOOTH:
                    return GetLerpedFromArray(TechnicolourColdPalette, time);
                default:
                    return Color.white;
            }
        }*/

        public static Color GetRandomFromArray(Color[] colors, float time, float seedMult = 8) {
            System.Random rand = new System.Random(Mathf.FloorToInt(seedMult * time));
            return colors[rand.Next(0, colors.Length)];
        }

        public static Color GetLerpedFromArray(Color[] colors, float time) {
            float tm = Mathf.Repeat(time, colors.Length);
            int t0 = Mathf.FloorToInt(tm);
            int t1 = Mathf.CeilToInt(tm);
            if (t1 >= colors.Length) t1 = 0;
            return (Color.Lerp(colors[t0], colors[t1], Mathf.Repeat(tm, 1)));
        }

    }

}
