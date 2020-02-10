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
using IPA.Utilities;

namespace Chroma {

    public static class ColourManager {

        private static Color?[] noteTypeColourOverrides = new Color?[] { null, null };
        public static Color? GetNoteTypeColourOverride(NoteType noteType) {
            return noteTypeColourOverrides[noteType == NoteType.NoteA ? 0 : 1];
        }

        public static void SetNoteTypeColourOverride(NoteType noteType, Color color) {
            noteTypeColourOverrides[noteType == NoteType.NoteA ? 0 : 1] = color;
        }

        public static void RemoveNoteTypeColourOverride(NoteType noteType) {
            noteTypeColourOverrides[noteType == NoteType.NoteA ? 0 : 1] = noteType == NoteType.NoteA ? A : B;
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
            PURE_RANDOM = 3,
            GRADIENT = 4
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

        public static bool TechnicolourLightsForceDisabled { get; set; } = false;
        public static bool TechnicolourBarriersForceDisabled { get; set; } = false;
        public static bool TechnicolourBombsForceDisabled { get; set; } = false;

        public static bool TechnicolourLights {
            get {
                return ChromaConfig.TechnicolourEnabled && !TechnicolourLightsForceDisabled && ChromaConfig.TechnicolourLightsStyle != TechnicolourStyle.OFF; }
        }

        public static bool TechnicolourSabers {
            get { return ChromaConfig.TechnicolourEnabled && ChromaConfig.TechnicolourSabersStyle != TechnicolourStyle.OFF; }
        }

        public static bool TechnicolourBlocks {
            get { return ChromaConfig.TechnicolourEnabled && ChromaConfig.TechnicolourBlocksStyle != TechnicolourStyle.OFF; }
        }

        public static bool TechnicolourBarriers {
            get { return ChromaConfig.TechnicolourEnabled && !TechnicolourBarriersForceDisabled && ChromaConfig.TechnicolourWallsStyle != TechnicolourStyle.OFF; }
        }

        public static bool TechnicolourBombs {
            get { return ChromaConfig.TechnicolourEnabled && !TechnicolourBombsForceDisabled && ChromaConfig.TechnicolourBombsStyle != TechnicolourStyle.OFF; }
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
                    return Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f);
                case TechnicolourStyle.WARM_COLD:
                    return warm ? GetWarmTechnicolour(time, transition) : GetColdTechnicolour(time, transition);
                default: return Color.white;
            }
        }

        public static Color GetEitherTechnicolour(float time, TechnicolourTransition transition) {
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
            float tm = Mathf.Repeat(time, colors.Length);
            int t0 = Mathf.FloorToInt(tm);
            int t1 = Mathf.CeilToInt(tm);
            if (t1 >= colors.Length) t1 = 0;
            return (Color.Lerp(colors[t0], colors[t1], Mathf.Repeat(tm, 1)));
        }

        #endregion



        /*
         * LIGHTS
         */

        public static Color DefaultLightAmbient { get; set; } = new Color(0, 0.706f, 1f, 1);

        public static Color DefaultLightA { get; } = new Color(1, 0.016f, 0.016f, 1); //255, 4, 4

        public static Color DefaultLightB { get; } = new Color(0, 0.753f, 1, 1); //0, 192, 255

        public static Color DefaultLightAltA { get; } = new Color(1, 0.032f, 1, 1); //255, 8, 255

        public static Color DefaultLightAltB { get; } = new Color(0.016f, 1, 0.016f, 1); //4, 255, 4

        public static Color DefaultLightWhite { get; } = new Color(1, 1, 1, 1); //Color.white

        public static Color DefaultLightGrey { get; } = new Color(0.6f, 0.6f, 0.6f, 1); //Color.white

        public static Color? LightAmbient { get; set; } = null; //new Color(0, 0.3765f, 0.5f, 1); //0, 192, 255

        public static Color? LightA { get; set; } = null; //new Color(1, 0, 0, 1);

        public static Color? LightB { get; set; } = null; //new Color(0, 0.502f, 1, 1);

        public static Color? LightAltA { get; set; } = null; //new Color(1, 0, 1, 1); //Color.magenta

        public static Color? LightAltB { get; set; } = null; //new Color(0, 1, 0, 1); //Color.green

        public static Color? LightWhite { get; set; } = null; //new Color(1, 1, 1, 1); //Color.white

        public static Color? LightGrey { get; set; } = null; //new Color(0.5f, 0.5f, 0.5f, 1); //128, 128, 128

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

        public static Color? A { get; set; } = null; //new Color(1, 0, 0, 1);

        public static Color? B { get; set; } = null; //new Color(0, 0.502f, 1, 1);

        public static Color? AltA { get; set; } = null; //new Color(1, 0, 1, 1); //Color.magenta

        public static Color? AltB { get; set; } = null; //new Color(0, 1, 0, 1); //Color.green

        public static Color? DoubleHit { get; set; } = null; //new Color(1.05f, 0, 2.188f, 1);

        public static Color? NonColoured { get; set; } = null; //new Color(1, 1, 1, 1);

        public static Color? Super { get; set; } = null; //new Color(1, 1, 0, 1);

        /*
         * OTHER
         */

        public static Color DefaultBarrierColour { get; } = Color.red;

        public static Color? BarrierColour { get; set; } = null;

        public static Color? LaserPointerColour { get; set; } = null; //B;

        public static Color? SignA { get; set; } = null; //LightA;

        public static Color? SignB { get; set; } = null; //LightB;

        public static Color? Platform { get; set; } = null;

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

        public static LightSwitchEventEffect[] GetAllLightSwitches() {
            if (_lightSwitches == null) _lightSwitches = Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>();
            return _lightSwitches;
        }
        private static LightSwitchEventEffect[] _lightSwitches = null;

        public static ParticleSystemEventEffect[] GetAllParticleSystems() {
            if (_particleSystems == null) _particleSystems = Resources.FindObjectsOfTypeAll<ParticleSystemEventEffect>();
            return _particleSystems;
        }
        private static ParticleSystemEventEffect[] _particleSystems = null;


        public static void ResetAllLights() {
            LightSwitchEventEffect[] lights = GetAllLightSwitches();
            foreach (LightSwitchEventEffect light in lights) light.Reset();
            ParticleSystemEventEffect[] particles = GetAllParticleSystems();
            foreach (ParticleSystemEventEffect particle in particles) particle.Reset();
            _lightSwitches = null;
            _particleSystems = null;
        }

        public static void RecolourAllLights(Color? red, Color? blue) {
            MonoBehaviour[] lights = GetAllLightSwitches();
            RecolourLights(ref lights, red, blue);
            MonoBehaviour[] particles = GetAllParticleSystems();
            RecolourLights(ref particles, red, blue);
        }

        public static void RecolourLights(ref MonoBehaviour[] lights, Color? red, Color? blue) {
            for (int i = 0; i < lights.Length; i++) {
                lights[i].SetLightingColours(red, blue);
            }
        }

        public static void RecolourLight(ref MonoBehaviour obj, Color red, Color blue)
        {
            obj.SetLightingColours(red, blue);
        }

        public static void RecolourLight(BeatmapEventType obj, Color red, Color blue) {
            obj.SetLightingColours(red, blue);
        }

        public static Dictionary<BeatmapEventType, LightSwitchEventEffect> LightSwitchs {
            get {
                if (_lightSwitchs == null) {
                    _lightSwitchs = new Dictionary<BeatmapEventType, LightSwitchEventEffect>();
                    foreach (LightSwitchEventEffect l in Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>()) {
                        _lightSwitchs.Add(l.GetPrivateField<BeatmapEventType>("_event"), l);
                    }
                }
                return _lightSwitchs;
            }
            set {
                if (_lightSwitchs != null) {
                    _lightSwitchs.Clear();
                    _lightSwitchs = null;
                }
            }
        }
        private static Dictionary<BeatmapEventType, LightSwitchEventEffect> _lightSwitchs;

        public static void RecolourMenuStuff(Color? red, Color? blue, Color? redLight, Color? blueLight, Color? platformLight, Color? laser) {

            Renderer[] rends2 = GameObject.FindObjectsOfType<Renderer>();

            foreach (Renderer rend in rends2) {


                if (rend.name.Contains("Laser") && laser != null) {
                    rend.material.color = (Color)laser;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", (Color)laser);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", (Color)laser);
                }
                if (rend.name.Contains("Glow") && platformLight != null) {
                    rend.material.color = (Color)platformLight;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", (Color)platformLight);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", (Color)platformLight);
                }
                if (rend.name.Contains("Feet") && platformLight != null) {
                    rend.material.color = (Color)platformLight;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", (Color)platformLight);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", (Color)platformLight);
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
                if (rend.name.Contains("VRCursor") && LaserPointerColour != null) {
                    rend.material.color = (Color)LaserPointerColour;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", (Color)LaserPointerColour);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", (Color)LaserPointerColour);
                }
                if (rend.name.Contains("Frame") && platformLight != null) {
                    rend.material.color = (Color)platformLight;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", (Color)platformLight);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", (Color)platformLight);
                }
            }

            //ChromaLogger.Log("Colourizing menustuff");
        }

        private static Dictionary<LightWithId, Color> _originalLightColors = new Dictionary<LightWithId, Color>();
        public static void RecolourAmbientLights(Color? color) {
            if (color == null) return;
            try {
                HashSet<BloomPrePassBGLight> bls = new HashSet<BloomPrePassBGLight>(BloomPrePassBGLight.bloomBGLightList);
                foreach (BloomPrePassBGLight light in bls) light.color = (Color)color;
            } catch (Exception e) {
                ChromaLogger.Log(e);
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

        public static void RecolourNeonSign(Color? colorA, Color? colorB) {
            bool Aclear = (colorA == null);
            bool Bclear = (colorB == null);
            TubeBloomPrePassLight[] _prePassLights = UnityEngine.Object.FindObjectsOfType<TubeBloomPrePassLight>();
            foreach (var prePassLight in _prePassLights) {
                if (prePassLight != null) {
                    if (prePassLight.name.Contains("SaberNeon"))
                        prePassLight.color = Aclear ? new Color(0.188f, 0.62f, 1f, 0.8f) : ((Color)colorA).ColorWithAlpha(0.8f);
                    if (prePassLight.name.Contains("BATNeon") || prePassLight.name.Contains("ENeon"))
                        prePassLight.color = Bclear ? new Color(1f, 0.031f, 0.031f, 1f) : ((Color)colorB).ColorWithAlpha(1f);

                }
            }

            SpriteRenderer[] sprites = Resources.FindObjectsOfTypeAll<SpriteRenderer>();
            foreach (var sprite in sprites) {
                if (sprite != null) {
                    if (sprite.name == "SaberLogo")
                        sprite.color = Aclear ? new Color(0f, 0.569f, 1f, 1f) : (Color)colorA;
                    if (sprite.name == "BatLogo" || sprite.name == "LogoE")
                        sprite.color = Bclear ? new Color(1f, 0f, 0f, 1f) : (Color)colorB;
                }
            }

            FlickeringNeonSign[] _flickers = Resources.FindObjectsOfTypeAll<FlickeringNeonSign>();
            foreach (var flicker in _flickers) {
                if (flicker != null) {
                    flicker.Start();
                }
            }

            //ChromaLogger.Log("Sign recoloured A:"+colorA.ToString() + " B:"+colorB.ToString());

        }

        public delegate void RefreshLightsDelegate(ref Color? ambientLight, ref Color? red, ref Color? blue, ref Color? redLight, ref Color? blueLight, ref Color? platform, ref Color? signA, ref Color? signB, ref Color? laser, ref string ambientSound);
        public static event RefreshLightsDelegate RefreshLightsEvent;

        public static void RefreshLights() {

            try {

                //ChromaLogger.Log("Refreshing Lights");

                Color? ambientLight = ColourManager.LightAmbient;
                Color? red = ColourManager.A;
                Color? blue = ColourManager.B;
                Color? redLight = ColourManager.LightA;
                Color? blueLight = ColourManager.LightB;
                Color? platform = ColourManager.Platform;
                Color? signA = ColourManager.SignA;
                Color? signB = ColourManager.SignB;
                Color? laser = ColourManager.LaserPointerColour;

                string ambientSound = null;

                RefreshLightsEvent?.Invoke(ref ambientLight, ref red, ref blue, ref redLight, ref blueLight, ref platform, ref signA, ref signB, ref laser, ref ambientSound);

                ResetAllLights();
                ColourManager.RecolourAmbientLights(ambientLight);
                ColourManager.RecolourNeonSign(signA, signB);
                ColourManager.RecolourMenuStuff(red, blue, redLight, blueLight, platform, laser);

                if (ambientSound == null) AudioUtil.Instance.StopAmbianceSound();
                else AudioUtil.Instance.StartAmbianceSound(ambientSound);

            } catch (Exception e) {
                ChromaLogger.Log("Error refreshing lights!");
                ChromaLogger.Log(e, ChromaLogger.Level.WARNING);
            }

        }

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
            foreach (NamedColor nc in namedColors) xmColours.Add(new XmlColour(nc.name, (Color)nc.color));
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
