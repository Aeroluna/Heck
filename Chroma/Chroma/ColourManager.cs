using Chroma.Beatmap.Events;
using Chroma.Extensions;
using Chroma.Settings;
using Chroma.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chroma {

    public static class ColourManager {

        private static Color[] noteTypeColourOverrides = new Color[] { Color.clear, Color.clear };
        public static Color GetNoteTypeColourOverride(NoteType noteType) {
            return noteTypeColourOverrides[noteType == NoteType.NoteA ? 0 : 1];
        }

        public static void SetNoteTypeColourOverride(NoteType noteType, Color color) {
            noteTypeColourOverrides[noteType == NoteType.NoteA ? 0 : 1] = color;
        }

        public static void RemoveNoteTypeColourOverride(NoteType noteType) {
            noteTypeColourOverrides[noteType == NoteType.NoteA ? 0 : 1] = noteType == NoteType.NoteA ? A : B;
        }

        public static float barrierColourCorrectionScale = 1f;
        public static Color GetBarrierColour(float time) {
            if (TechnicolourBarriers) return GetTechnicolour(true, time, ChromaConfig.TechnicolourWallsStyle);
            else {
                Color c = ChromaBarrierColourEvent.GetColor(time);
                return (c == Color.clear ? ColourManager.BarrierColour : c);
            }
        }

        /*
         * TECHNICOLOUR
         */

        #region technicolour

        private static Color[] _technicolourWarmPalette;
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
        }

        public enum TechnicolourStyle {
            OFF = 0,
            WARM_COLD = 1,
            ANY_PALETTE = 2,
            PURE_RANDOM = 3
        }

        public enum TechnicolourTransition {
            FLAT = 0,
            SMOOTH = 1,
        }

        public enum TechnicolourLightsGrouping {
            STANDARD = 0,
            ISOLATED_GROUP = 1,
            ISOLATED = 2
        }

        public static TechnicolourStyle GetTechnicolourStyleFromFloat(float f) {
            if (f == 1) return TechnicolourStyle.WARM_COLD;
            else if (f == 2) return TechnicolourStyle.ANY_PALETTE;
            else if (f == 3) return TechnicolourStyle.PURE_RANDOM;
            else return TechnicolourStyle.OFF;
        }

        public static TechnicolourLightsGrouping GetTechnicolourLightsGroupingFromFloat(float f) {
            if (f == 1) return TechnicolourLightsGrouping.ISOLATED_GROUP;
            else if (f == 2) return TechnicolourLightsGrouping.ISOLATED;
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

        public static bool TechnicolourLights {
            get {
                return !ChromaBehaviour.IsLoadingSong && !technicolourLightsForceDisabled && ChromaConfig.TechnicolourEnabled && ChromaConfig.TechnicolourLightsStyle != TechnicolourStyle.OFF; }
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
        }

        public static Color GetRandomFromArray(Color[] colors, float time, float seedMult = 8) {
            System.Random rand = new System.Random(Mathf.FloorToInt(seedMult * time));
            return colors[rand.Next(0, colors.Length)];
        }

        public static Color GetLerpedFromArray(Color[] colors, float time) {
            float tm = Mathf.Repeat(time, TechnicolourWarmPalette.Length);
            int t0 = Mathf.FloorToInt(tm);
            int t1 = Mathf.CeilToInt(tm);
            if (t1 >= TechnicolourWarmPalette.Length) t1 = 0;
            return (Color.Lerp(colors[t0], colors[t1], Mathf.Repeat(tm, 1)));
        }

        #endregion



        /*
         * LIGHTS
         */

        //public static Color DefaultLightAmbient { get; set; } = new Color(0, 0.3765f, 0.5f, 1); //0, 192, 255
        public static Color DefaultLightAmbient { get; set; } = new Color(0, 0.706f, 1f, 1);

        public static Color DefaultLightA { get; } = new Color(1, 0.016f, 0.016f, 1); //255, 4, 4

        public static Color DefaultLightB { get; } = new Color(0, 0.753f, 1, 1); //0, 192, 255

        public static Color DefaultLightAltA { get; } = new Color(1, 0.032f, 1, 1); //255, 8, 255

        public static Color DefaultLightAltB { get; } = new Color(0.016f, 1, 0.016f, 1); //4, 255, 4

        public static Color DefaultLightWhite { get; } = new Color(1, 1, 1, 1); //Color.white

        public static Color DefaultLightGrey { get; } = new Color(0.6f, 0.6f, 0.6f, 1); //Color.white

        public static Color LightAmbient { get; set; } = Color.clear; //new Color(0, 0.3765f, 0.5f, 1); //0, 192, 255

        public static Color LightA { get; set; } = Color.clear; //new Color(1, 0, 0, 1);

        public static Color LightB { get; set; } = Color.clear; //new Color(0, 0.502f, 1, 1);

        public static Color LightAltA { get; set; } = Color.clear; //new Color(1, 0, 1, 1); //Color.magenta

        public static Color LightAltB { get; set; } = Color.clear; //new Color(0, 1, 0, 1); //Color.green

        public static Color LightWhite { get; set; } = Color.clear; //new Color(1, 1, 1, 1); //Color.white

        public static Color LightGrey { get; set; } = Color.clear; //new Color(0.5f, 0.5f, 0.5f, 1); //128, 128, 128

        /*
         * BLOCKS / SABERS
         */

        public static Color DefaultA { get; } = new Color(1, 0, 0, 1);

        public static Color DefaultB { get; } = new Color(0, 0.502f, 1, 1);

        public static Color DefaultAltA { get; } = new Color(1, 0, 1, 1); //Color.magenta

        public static Color DefaultAltB { get; } = new Color(0, 1, 0, 1); //Color.green

        public static Color DefaultDoubleHit { get; } = new Color(1.05f, 0, 2.188f, 1);

        public static Color DefaultNonColoured { get; } = new Color(1, 1, 1, 1); //Color.white

        public static Color DefaultSuper { get; set; } = new Color(1, 1, 0, 1);

        public static Color A { get; set; } = Color.clear; //new Color(1, 0, 0, 1);

        public static Color B { get; set; } = Color.clear; //new Color(0, 0.502f, 1, 1);

        public static Color AltA { get; set; } = Color.clear; //new Color(1, 0, 1, 1); //Color.magenta

        public static Color AltB { get; set; } = Color.clear; //new Color(0, 1, 0, 1); //Color.green

        public static Color DoubleHit { get; set; } = Color.clear; //new Color(1.05f, 0, 2.188f, 1);

        public static Color NonColoured { get; set; } = Color.clear; //new Color(1, 1, 1, 1);

        public static Color Super { get; set; } = Color.clear; //new Color(1, 1, 0, 1);

        /*
         * OTHER
         */

        public static Color DefaultBarrierColour { get; } = Color.red;

        public static Color BarrierColour { get; set; } = DefaultBarrierColour;

        public static Color LaserPointerColour { get; set; } = Color.clear; //B;

        public static Color SignA { get; set; } = Color.clear; //LightA;

        public static Color SignB { get; set; } = Color.clear; //LightB;

        public static Color Platform { get; set; } = Color.clear;

        public static String ColourToString(Color color) {
            return Mathf.RoundToInt(color.r * 255) + ";" + Mathf.RoundToInt(color.g * 255) + ";" + Mathf.RoundToInt(color.b * 255) + ";" + Mathf.RoundToInt(color.a * 255);
        }

        public static Color ColourFromString(String colorString) {
            Color color = Color.black;
            try {
                String[] split = colorString.Split(';');
                if (split.Length > 2) {
                    color.r = float.Parse(split[0]) / 255f;
                    color.g = float.Parse(split[1]) / 255f;
                    color.b = float.Parse(split[2]) / 255f;
                    if (split.Length > 3)
                        color.a = float.Parse(split[3]) / 255f;
                }
            } catch (Exception) { }
            return color;
        }

        public const int RGB_INT_OFFSET = 2000000000;

        public static int ColourToInt(Color color) {
            int r = Mathf.FloorToInt(color.r * 255);
            int g = Mathf.FloorToInt(color.g * 255);
            int b = Mathf.FloorToInt(color.b * 255);
            return RGB_INT_OFFSET + (((r & 0x0ff) << 16) | ((g & 0x0ff) << 8) | (b & 0x0ff));
        }

        public static Color ColourFromInt(int rgb) {
            rgb = rgb - RGB_INT_OFFSET;
            int red = (rgb >> 16) & 0x0ff;
            int green = (rgb >> 8) & 0x0ff;
            int blue = (rgb) & 0x0ff;
            return new Color(red / 255f, green / 255f, blue / 255f, 1);
        }

        public static void SetNoteColour(NoteController note, Color c) {
            SetNoteColour(note.GetComponentInChildren<ColorNoteVisuals>(), c);
        }

        public static void SetNoteColour(ColorNoteVisuals noteVis, Color c) {
            SpriteRenderer ____arrowGlowSpriteRenderer = noteVis.GetField<SpriteRenderer>("_arrowGlowSpriteRenderer");
            SpriteRenderer ____circleGlowSpriteRenderer = noteVis.GetField<SpriteRenderer>("_circleGlowSpriteRenderer");
            MaterialPropertyBlockController ____materialPropertyBlockController = noteVis.GetField<MaterialPropertyBlockController>("_materialPropertyBlockController");
            if (____arrowGlowSpriteRenderer != null) ____arrowGlowSpriteRenderer.color = c;
            if (____circleGlowSpriteRenderer != null) ____circleGlowSpriteRenderer.color = c;
            MaterialPropertyBlock block = ____materialPropertyBlockController.materialPropertyBlock;
            block.SetColor(noteVis.GetField<int>("_colorID"), c);
        }

        public static LightSwitchEventEffect[] GetAllLightSwitches() {
            return Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>();
        }

        
        public static void ResetAllLights() {
            LightSwitchEventEffect[] lights = GetAllLightSwitches();
            foreach (LightSwitchEventEffect light in lights) light.Reset();
        }

        public static void RecolourAllLights(Color red, Color blue) {
            LightSwitchEventEffect[] lights = GetAllLightSwitches();
            RecolourLights(ref lights, red, blue);
        }

        public static void RecolourLights(ref LightSwitchEventEffect[] lights, Color red, Color blue) {
            for (int i = 0; i < lights.Length; i++) {
                RecolourLight(ref lights[i], red, blue);
            }
        }

        public static void RecolourLight(ref LightSwitchEventEffect obj, Color red, Color blue) {
            obj.SetLightingColours(red, blue);
        }

        /*public static void RecolourLight(ref LightSwitchEventEffect obj, Color red, Color blue) {
            if (obj.name.Contains("nightmare")) return;
            string[] sa = new string[] { "_lightColor0", "_highlightColor0", "_lightColor1", "_highlightColor1" };

            for (int i = 0; i < sa.Length; i++) {
                string s = sa[i];

                SimpleColorSO baseSO = SetupNewLightColourSOs(obj, s);

                Color newColour = i < sa.Length / 2 ? blue : red;
                if (newColour == Color.clear) continue;

                baseSO.SetColor(newColour);
            }
        }*/

        public static void RecolourMenuStuff(Color red, Color blue, Color redLight, Color blueLight, Color platformLight, Color laser) {

            Renderer[] rends2 = GameObject.FindObjectsOfType<Renderer>();

            foreach (Renderer rend in rends2) {


                if (rend.name.Contains("Laser") && laser != Color.clear) {
                    rend.material.color = laser;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", laser);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", laser);
                }
                if (rend.name.Contains("Glow") && platformLight != Color.clear) {
                    rend.material.color = platformLight;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", platformLight);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", platformLight);
                }
                if (rend.name.Contains("Feet") && platformLight != Color.clear) {
                    rend.material.color = platformLight;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", platformLight);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", platformLight);
                }
                /*if (rend.name.Contains("Neon")) {
                    rend.material.color = blue;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", blue);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", blue);
                }
                if (rend.name.Contains("Border")) {
                    rend.material.color = blue;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", blueLight);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", blueLight);
                }*/
                /*if (rend.name.Contains("Light")) {
                    rend.material.color = blue;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", blue);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", blue);
                }*/
                if (rend.name.Contains("VRCursor") && ColourManager.LaserPointerColour != Color.clear) {
                    rend.material.color = ColourManager.LaserPointerColour;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", ColourManager.LaserPointerColour);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", ColourManager.LaserPointerColour);
                }

                /*Plugin.Log(rend.gameObject.name + " ::: " + rend.name.ToString());
                if (rend.materials.Length > 0) {
                    foreach (Material m in rend.materials) {
                        Plugin.Log("___" + m.name);
                    }
                }*/
            }

        }

        public static void RecolourAmbientLights(Color color) {
            List<TubeBloomPrePassLight> bls = UnityEngine.Object.FindObjectsOfType<TubeBloomPrePassLight>().ToList();
            LightSwitchEventEffect[] lights = GetAllLightSwitches();
            foreach (LightSwitchEventEffect light in lights) {
                BloomPrePassLight[] blsInLight = light.GetField<BloomPrePassLight[]>("_lights");
                foreach (BloomPrePassLight b in blsInLight) {
                    if (b is TubeBloomPrePassLight tb) bls.Remove(tb);
                }
            }
            foreach (TubeBloomPrePassLight tb in bls) {
                if (color == Color.clear) tb.Reset();
                else tb.ApplyColour(color);
            }
        }

        public static void BreakReality() {
            Color color = Color.black;
            Renderer[] rends = GameObject.FindObjectsOfType<Renderer>();
            foreach (Renderer rend in rends) {
                if (color == Color.black) rend.enabled = false;
                else {
                    rend.enabled = true;
                    if (rend.materials.Length > 0) {
                        if (rend.material.shader.name == "Custom/ParametricBox" || rend.material.shader.name == "Custom/ParametricBoxOpaque") {
                            rend.material.SetColor("_Color", new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 1.0f));
                            Console.WriteLine("found material");
                        }
                    }
                }
            }
        }

        public static void RecolourNeonSign(Color colorA, Color colorB) {

            TubeBloomPrePassLight[] _prePassLights = UnityEngine.Object.FindObjectsOfType<TubeBloomPrePassLight>();

            foreach (var prePassLight in _prePassLights) {

                if (prePassLight != null) {
                    if (prePassLight.name.Contains("NeonLight (6)")) {
                        if (colorA != Color.clear) prePassLight.color = colorA;

                    }
                    if (prePassLight.name.Contains("NeonLight (8)")) {
                        if (prePassLight.gameObject.transform.position.ToString() == "(0.0, 17.2, 24.8)") {
                            if (colorA != Color.clear) prePassLight.color = colorA;
                        }

                    }
                    if (prePassLight.name.Contains("BATNeon") || prePassLight.name.Contains("ENeon"))
                        if (colorB != Color.clear) prePassLight.color = colorB;

                    //    Log($"PrepassLight: {prePassLight.name}");
                }
            }

            SpriteRenderer[] sprites = Resources.FindObjectsOfTypeAll<SpriteRenderer>();
            foreach (SpriteRenderer sprite in sprites) {
                if (sprite != null) {
                    if (sprite.name == "LogoSABER")
                        if (colorA != Color.clear) sprite.color = colorA;
                    if (sprite.name == "LogoBAT" || sprite.name == "LogoE")
                        if (colorB != Color.clear) sprite.color = colorB;
                }

            }

            TextMeshPro[] tmps = GameObject.FindObjectsOfType<TextMeshPro>();
            foreach (TextMeshPro tmp in tmps) {
                if (tmp.gameObject.name == "CustomMenuText") {
                    if (colorB != Color.clear) tmp.color = colorB;
                } else if (tmp.gameObject.name == "CustomMenuText-Bot") {
                    if (colorA != Color.clear) tmp.color = colorA;
                }
            }
            
            ChromaLogger.Log("Sign recoloured A:"+colorA.ToString() + " B:"+colorB.ToString());

        }

        public delegate void RefreshLightsDelegate(ref Color ambientLight, ref Color red, ref Color blue, ref Color redLight, ref Color blueLight, ref Color platform, ref Color signA, ref Color signB, ref Color laser, ref string ambientSound);
        public static event RefreshLightsDelegate RefreshLightsEvent;

        public static void RefreshLights() {

            try {

                ChromaLogger.Log("Refreshing Lights");

                Color ambientLight = ColourManager.LightAmbient;
                Color red = ColourManager.LightAmbient;
                Color blue = ColourManager.LightAmbient;
                Color redLight = ColourManager.LightAmbient;
                Color blueLight = ColourManager.LightAmbient;
                Color platform = ColourManager.LightAmbient;
                Color signA = ColourManager.LightAmbient;
                Color signB = ColourManager.LightAmbient;
                Color laser = ColourManager.LaserPointerColour;

                string ambientSound = null;

                RefreshLightsEvent?.Invoke(ref ambientLight, ref red, ref blue, ref redLight, ref blueLight, ref platform, ref signA, ref signB, ref laser, ref ambientSound);

                //ColourManager.RecolourAllLights(ColourManager.LightA, ColourManager.LightB);
                ResetAllLights();
                ColourManager.RecolourAmbientLights(ambientLight);
                if (!SceneUtils.IsTargetGameScene(SceneManager.GetActiveScene())) {
                    ColourManager.RecolourNeonSign(signA, signB);
                    ColourManager.RecolourMenuStuff(red, blue, redLight, blueLight, platform, laser);
                }

                if (ambientSound == null) AudioUtil.Instance.StopAmbianceSound();
                else AudioUtil.Instance.StartAmbianceSound(ambientSound);

            } catch (Exception e) {
                ChromaLogger.Log("Error refreshing lights!");
                ChromaLogger.Log(e, ChromaLogger.Level.WARNING);
            }

        }

        /*public static SimpleColorSO SetupNewLightColourSOs(LightSwitchEventEffect light, String s) {
            return SetupNewLightColourSOs(light, s, Color.clear);
        }

        public static SimpleColorSO SetupNewLightColourSOs(LightSwitchEventEffect light, String s, Color overrideMultiplierColour) {
            MultipliedColorSO mColorSO = light.GetField<MultipliedColorSO>(s);
            SimpleColorSO baseSO = mColorSO.GetField<SimpleColorSO>("_baseColor");
            
            if (overrideMultiplierColour == Color.clear) {
                mColorSO.SetField("_multiplierColor", mColorSO.GetField<Color>("_multiplierColor"));
            } else {
                mColorSO.SetField("_multiplierColor", overrideMultiplierColour);
            }
            //mColorSO.SetField("_baseColor", newBaseSO);

            //light.SetField(s, newMColorSO);
            //if (!light.name.Contains("chroma")) light.name = light.name + "_chroma";
            return baseSO;
        }

        public static void SetupAllNewLightColourSOs() {
            LightSwitchEventEffect[] lights = GetAllLightSwitches();
            string[] sa = new string[] { "_lightColor0", "_highlightColor0", "_lightColor1", "_highlightColor1" };
            foreach (LightSwitchEventEffect light in lights) {
                for (int i = 0; i < sa.Length; i++) {
                    SetupNewLightColourSOs(light, sa[i]);
                }
            }
        }*/


        //TODO
        //This shit is messy.  Fix that.
        #region savedColours

        [XmlRoot("Colour")]
        public class XmlColour {

            [XmlElement("Name")]
            public string name;
            [XmlElement("R")]
            public int r;
            [XmlElement("G")]
            public int g;
            [XmlElement("B")]
            public int b;
            [XmlElement("A")]
            public int a;

            public XmlColour() {

            }

            public XmlColour(string name, int r, int g, int b, int a = 255) {
                this.name = name;
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }

            public XmlColour(string name, Color color) {
                this.name = name;
                this.r = Mathf.FloorToInt(color.r);
                this.r = Mathf.FloorToInt(color.g);
                this.r = Mathf.FloorToInt(color.b);
                this.r = Mathf.FloorToInt(color.a);
            }

            public Color Color {
                get {
                    return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
                }
            }

        }

        public static List<NamedColor> LoadColoursFromFile() {

            List<NamedColor> colors = new List<NamedColor>();

            string filePath = Environment.CurrentDirectory.Replace('\\', '/') + "/UserData/Chroma/Colours.xml";

            List<XmlColour> xms = new List<XmlColour>();

            try {

                if (File.Exists(filePath)) {
                    XmlSerializer ser = new XmlSerializer(typeof(List<XmlColour>));
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open)) {
                        xms = (List<XmlColour>)ser.Deserialize(fileStream);
                    }
                }

            } catch (Exception e) {
                ChromaLogger.Log(e);
            }

            if (xms != null) {
                foreach (XmlColour xm in xms) {
                    colors.Add(new NamedColor(xm.name, xm.Color));
                }
            }

            return colors;

        }

        public static void SaveExampleColours() {
            
            try {

                string filePath = Environment.CurrentDirectory.Replace('\\', '/') + "/UserData/Chroma/Colours.xml";
                if (!File.Exists(filePath)) {

                    XmlColour[] exampleColours = new XmlColour[] {
                        new XmlColour("Dark Red", 128, 0, 0, 255),
                        new XmlColour("Mega Blue", 0, 0, 700, 255),
                        new XmlColour("Brown(Why?)", 139, 69, 19, 255),
                    };

                    SaveColours(exampleColours);
                }

            } catch (Exception e) {
                ChromaLogger.Log(e);
            }
        }

        public static void SaveColours(params XmlColour[] colours) {

            if (colours.Length < 1) return;

            string filePath = Environment.CurrentDirectory.Replace('\\', '/') + "/UserData/Chroma/Colours.xml";

            List<NamedColor> namedColors = LoadColoursFromFile();
            List<XmlColour> xmColours = new List<XmlColour>();
            foreach (NamedColor nc in namedColors) xmColours.Add(new XmlColour(nc.name, nc.color));
            foreach (XmlColour xc in colours) xmColours.Add(xc);

            try {

                using (StreamWriter w = File.CreateText(filePath)) {
                    XmlSerializer ser = new XmlSerializer(xmColours.GetType());
                    ser.Serialize(w, xmColours);
                }
            } catch (Exception e) {
                ChromaLogger.Log(e);
            }

        }
        #endregion

    }

}
