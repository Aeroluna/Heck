using Chroma.Extensions;
using Chroma.Settings;
using Chroma.Utils;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Chroma
{
    public static class ColourManager
    {
        private static Color?[] noteTypeColourOverrides = new Color?[] { null, null };

        public static Color? GetNoteTypeColourOverride(NoteType noteType)
        {
            return noteTypeColourOverrides[noteType == NoteType.NoteA ? 0 : 1];
        }

        public static void SetNoteTypeColourOverride(NoteType noteType, Color color)
        {
            noteTypeColourOverrides[noteType == NoteType.NoteA ? 0 : 1] = color;
        }

        public static void RemoveNoteTypeColourOverride(NoteType noteType)
        {
            noteTypeColourOverrides[noteType == NoteType.NoteA ? 0 : 1] = null;
        }

        /*
         * TECHNICOLOUR
         */

        #region technicolour

        private static Color[] _technicolourWarmPalette;
        private static Color[] _technicolourColdPalette;

        public static Color[] TechnicolourCombinedPalette { get; private set; }

        public static Color[] TechnicolourWarmPalette
        {
            get { return _technicolourWarmPalette; }
            set
            {
                _technicolourWarmPalette = value;
                SetupCombinedTechnicolourPalette();
            }
        }

        public static Color[] TechnicolourColdPalette
        {
            get { return _technicolourColdPalette; }
            set
            {
                _technicolourColdPalette = value;
                SetupCombinedTechnicolourPalette();
            }
        }

        private static void SetupCombinedTechnicolourPalette()
        {
            if (_technicolourColdPalette == null || _technicolourWarmPalette == null) return;
            Color[] newCombined = new Color[_technicolourColdPalette.Length + _technicolourWarmPalette.Length];
            for (int i = 0; i < _technicolourColdPalette.Length; i++) newCombined[i] = _technicolourColdPalette[i];
            for (int i = 0; i < _technicolourWarmPalette.Length; i++) newCombined[_technicolourColdPalette.Length + i] = _technicolourWarmPalette[i];
            System.Random shuffleRandom = new System.Random();
            TechnicolourCombinedPalette = newCombined.OrderBy(x => shuffleRandom.Next()).ToArray();
            ChromaLogger.Log("Combined TC Palette formed : " + TechnicolourCombinedPalette.Length);
        }

        public enum TechnicolourStyle
        {
            OFF = 0,
            WARM_COLD = 1,
            ANY_PALETTE = 2,
            PURE_RANDOM = 3,
            GRADIENT = 4
        }

        public enum TechnicolourTransition
        {
            FLAT = 0,
            SMOOTH = 1,
        }

        public enum TechnicolourLightsGrouping
        {
            STANDARD = 0,
            ISOLATED_GROUP = 1,
            ISOLATED = 2
        }

        public static bool TechnicolourLightsForceDisabled { get; set; } = false;
        public static bool TechnicolourBlocksForceDisabled { get; set; } = false;
        public static bool TechnicolourBarriersForceDisabled { get; set; } = false;
        public static bool TechnicolourBombsForceDisabled { get; set; } = false;

        public static bool TechnicolourLights
        {
            get
            {
                return ChromaConfig.TechnicolourEnabled && !TechnicolourLightsForceDisabled && ChromaConfig.TechnicolourLightsStyle != TechnicolourStyle.OFF;
            }
        }

        public static bool TechnicolourSabers
        {
            get { return ChromaConfig.TechnicolourEnabled && !TechnicolourBlocksForceDisabled && ChromaConfig.TechnicolourSabersStyle != TechnicolourStyle.OFF; }
        }

        public static bool TechnicolourBlocks
        {
            get { return ChromaConfig.TechnicolourEnabled && !TechnicolourBlocksForceDisabled && ChromaConfig.TechnicolourBlocksStyle != TechnicolourStyle.OFF; }
        }

        public static bool TechnicolourBarriers
        {
            get { return ChromaConfig.TechnicolourEnabled && !TechnicolourBarriersForceDisabled && ChromaConfig.TechnicolourWallsStyle != TechnicolourStyle.OFF; }
        }

        public static bool TechnicolourBombs
        {
            get { return ChromaConfig.TechnicolourEnabled && !TechnicolourBombsForceDisabled && ChromaConfig.TechnicolourBombsStyle != TechnicolourStyle.OFF; }
        }

        public static Color GetTechnicolour(bool warm, float time, TechnicolourStyle style, TechnicolourTransition transition = TechnicolourTransition.FLAT)
        {
            switch (style)
            {
                case TechnicolourStyle.ANY_PALETTE:
                    return GetEitherTechnicolour(time, transition);

                case TechnicolourStyle.PURE_RANDOM:
                    return Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f);

                case TechnicolourStyle.WARM_COLD:
                    return warm ? GetWarmTechnicolour(time, transition) : GetColdTechnicolour(time, transition);

                default: return Color.white;
            }
        }

        public static Color GetEitherTechnicolour(float time, TechnicolourTransition transition)
        {
            switch (transition)
            {
                case TechnicolourTransition.FLAT:
                    return GetRandomFromArray(TechnicolourCombinedPalette, time);

                case TechnicolourTransition.SMOOTH:
                    return GetLerpedFromArray(TechnicolourCombinedPalette, time);

                default:
                    return Color.white;
            }
        }

        public static Color GetWarmTechnicolour(float time, TechnicolourTransition transition)
        {
            switch (transition)
            {
                case TechnicolourTransition.FLAT:
                    return GetRandomFromArray(TechnicolourWarmPalette, time);

                case TechnicolourTransition.SMOOTH:
                    return GetLerpedFromArray(TechnicolourWarmPalette, time);

                default:
                    return Color.white;
            }
        }

        public static Color GetColdTechnicolour(float time, TechnicolourTransition transition)
        {
            switch (transition)
            {
                case TechnicolourTransition.FLAT:
                    return GetRandomFromArray(TechnicolourColdPalette, time);

                case TechnicolourTransition.SMOOTH:
                    return GetLerpedFromArray(TechnicolourColdPalette, time);

                default:
                    return Color.white;
            }
        }

        public static Color GetRandomFromArray(Color[] colors, float time, float seedMult = 8)
        {
            System.Random rand = new System.Random(Mathf.FloorToInt(seedMult * time));
            return colors[rand.Next(0, colors.Length)];
        }

        public static Color GetLerpedFromArray(Color[] colors, float time)
        {
            float tm = Mathf.Repeat(time, colors.Length);
            int t0 = Mathf.FloorToInt(tm);
            int t1 = Mathf.CeilToInt(tm);
            if (t1 >= colors.Length) t1 = 0;
            return (Color.Lerp(colors[t0], colors[t1], Mathf.Repeat(tm, 1)));
        }

        #endregion technicolour

        /*
         * COLORS
         */

        public static Color? LaserPointerColour { get; set; } = null; //B;

        public static Color? SignA { get; set; } = null; //LightA;

        public static Color? SignB { get; set; } = null; //LightB;

        public static Color? Platform { get; set; } = null;

        public static Color ColourFromString(string colorString)
        {
            Color color = Color.black;
            try
            {
                string[] split = colorString.Split(';');
                if (split.Length > 2)
                {
                    color.r = float.Parse(split[0]) / 255f;
                    color.g = float.Parse(split[1]) / 255f;
                    color.b = float.Parse(split[2]) / 255f;
                    if (split.Length > 3)
                        color.a = float.Parse(split[3]) / 255f;
                }
            }
            catch (Exception) { }
            return color;
        }

        public const int RGB_INT_OFFSET = 2000000000;

        public static Color ColourFromInt(int rgb)
        {
            rgb = rgb - RGB_INT_OFFSET;
            int red = (rgb >> 16) & 0x0ff;
            int green = (rgb >> 8) & 0x0ff;
            int blue = (rgb) & 0x0ff;
            return new Color(red / 255f, green / 255f, blue / 255f, 1);
        }

        public static LightSwitchEventEffect[] GetAllLightSwitches()
        {
            if (_lightSwitches == null) _lightSwitches = Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>();
            return _lightSwitches;
        }

        private static LightSwitchEventEffect[] _lightSwitches = null;

        public static ParticleSystemEventEffect[] GetAllParticleSystems()
        {
            if (_particleSystems == null) _particleSystems = Resources.FindObjectsOfTypeAll<ParticleSystemEventEffect>();
            return _particleSystems;
        }

        private static ParticleSystemEventEffect[] _particleSystems = null;

        public static void ResetAllLights()
        {
            LightSwitchEventEffect[] lights = GetAllLightSwitches();
            foreach (LightSwitchEventEffect light in lights) light.Reset();
            ParticleSystemEventEffect[] particles = GetAllParticleSystems();
            foreach (ParticleSystemEventEffect particle in particles) particle.Reset();
            _lightSwitches = null;
            _particleSystems = null;
        }

        public static void RecolourAllLights(Color? red, Color? blue)
        {
            MonoBehaviour[] lights = GetAllLightSwitches();
            RecolourLights(ref lights, red, blue);
            MonoBehaviour[] particles = GetAllParticleSystems();
            RecolourLights(ref particles, red, blue);
        }

        public static void RecolourLights(ref MonoBehaviour[] lights, Color? red, Color? blue)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].SetLightingColours(red, blue);
            }
        }

        public static void RecolourLight(ref MonoBehaviour obj, Color red, Color blue)
        {
            obj.SetLightingColours(red, blue);
        }

        public static void RecolourLight(BeatmapEventType obj, Color red, Color blue)
        {
            obj.SetLightingColours(red, blue);
        }

        public static Dictionary<BeatmapEventType, LightSwitchEventEffect> LightSwitchs
        {
            get
            {
                if (_lightSwitchs == null)
                {
                    _lightSwitchs = new Dictionary<BeatmapEventType, LightSwitchEventEffect>();
                    foreach (LightSwitchEventEffect l in Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>())
                    {
                        _lightSwitchs.Add(l.GetPrivateField<BeatmapEventType>("_event"), l);
                    }
                }
                return _lightSwitchs;
            }
            set
            {
                if (_lightSwitchs != null)
                {
                    _lightSwitchs = null;
                }
            }
        }

        private static Dictionary<BeatmapEventType, LightSwitchEventEffect> _lightSwitchs;

        public static void RecolourMenuStuff(Color? platformLight, Color? laser)
        {
            Renderer[] rends2 = UnityEngine.Object.FindObjectsOfType<Renderer>();

            foreach (Renderer rend in rends2)
            {
                if (rend.name.Contains("Laser") && laser.HasValue)
                {
                    rend.material.color = laser.Value;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", laser.Value);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", laser.Value);
                }
                if (rend.name.Contains("Glow") && platformLight.HasValue)
                {
                    rend.material.color = platformLight.Value;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", platformLight.Value);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", platformLight.Value);
                }
                if (rend.name.Contains("Feet") && platformLight.HasValue)
                {
                    rend.material.color = platformLight.Value;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", platformLight.Value);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", platformLight.Value);
                }
                if (rend.name.Contains("VRCursor") && LaserPointerColour.HasValue)
                {
                    rend.material.color = LaserPointerColour.Value;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", LaserPointerColour.Value);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", LaserPointerColour.Value);
                }
                if (rend.name.Contains("Frame") && platformLight.HasValue)
                {
                    rend.material.color = platformLight.Value;
                    if (rend.material.HasProperty("_color")) rend.material.SetColor("_color", platformLight.Value);
                    if (rend.material.HasProperty("_Color")) rend.material.SetColor("_Color", platformLight.Value);
                }
            }

            //ChromaLogger.Log("Colourizing menustuff");
        }

        private static Dictionary<LightWithId, Color> _originalLightColors = new Dictionary<LightWithId, Color>();

        public static void RecolourAmbientLights(Color? color)
        {
            if (!color.HasValue) return;
            try
            {
                HashSet<BloomPrePassBGLight> bls = new HashSet<BloomPrePassBGLight>(BloomPrePassBGLight.bloomBGLightList);
                foreach (BloomPrePassBGLight light in bls) light.color = color.Value;
            }
            catch (Exception e)
            {
                ChromaLogger.Log(e);
            }
        }

        public static void RecolourNeonSign(Color? colorA, Color? colorB)
        {
            bool Aclear = (colorA == null);
            bool Bclear = (colorB == null);
            TubeBloomPrePassLight[] _prePassLights = UnityEngine.Object.FindObjectsOfType<TubeBloomPrePassLight>();
            foreach (var prePassLight in _prePassLights)
            {
                if (prePassLight != null)
                {
                    if (prePassLight.name.Contains("SaberNeon"))
                        prePassLight.color = Aclear ? new Color(0.188f, 0.62f, 1f, 0.8f) : colorA.Value.ColorWithAlpha(0.8f);
                    if (prePassLight.name.Contains("BATNeon") || prePassLight.name.Contains("ENeon"))
                        prePassLight.color = Bclear ? new Color(1f, 0.031f, 0.031f, 1f) : colorB.Value.ColorWithAlpha(1f);
                }
            }

            SpriteRenderer[] sprites = Resources.FindObjectsOfTypeAll<SpriteRenderer>();
            foreach (var sprite in sprites)
            {
                if (sprite != null)
                {
                    if (sprite.name == "SaberLogo")
                        sprite.color = Aclear ? new Color(0f, 0.569f, 1f, 1f) : colorA.Value;
                    if (sprite.name == "BatLogo" || sprite.name == "LogoE")
                        sprite.color = Bclear ? new Color(1f, 0f, 0f, 1f) : colorB.Value;
                }
            }

            FlickeringNeonSign[] _flickers = Resources.FindObjectsOfTypeAll<FlickeringNeonSign>();
            foreach (var flicker in _flickers)
            {
                if (flicker != null)
                {
                    flicker.Start();
                }
            }

            //ChromaLogger.Log("Sign recoloured A:"+colorA.ToString() + " B:"+colorB.ToString());
        }

        public static void RefreshLights()
        {
            try
            {
                ResetAllLights();
                RecolourNeonSign(SignA, SignB);
                RecolourMenuStuff(Platform, LaserPointerColour);
            }
            catch (Exception e)
            {
                ChromaLogger.Log("Error refreshing lights!");
                ChromaLogger.Log(e, ChromaLogger.Level.WARNING);
            }
        }

        //TODO
        //This shit is messy.  Fix that.

        #region savedColours

        [XmlRoot("Colour")]
        public class XmlColour
        {
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

            public XmlColour()
            {
            }

            public XmlColour(string name, int r, int g, int b, int a = 255)
            {
                this.name = name;
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }

            public XmlColour(string name, Color color)
            {
                this.name = name;
                this.r = Mathf.FloorToInt(color.r);
                this.r = Mathf.FloorToInt(color.g);
                this.r = Mathf.FloorToInt(color.b);
                this.r = Mathf.FloorToInt(color.a);
            }

            public Color Color
            {
                get
                {
                    return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
                }
            }
        }

        public static List<NamedColor> LoadColoursFromFile()
        {
            List<NamedColor> colors = new List<NamedColor>();

            string filePath = Environment.CurrentDirectory.Replace('\\', '/') + "/UserData/Chroma/Colours.xml";

            List<XmlColour> xms = new List<XmlColour>();

            try
            {
                if (File.Exists(filePath))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(List<XmlColour>));
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        xms = (List<XmlColour>)ser.Deserialize(fileStream);
                    }
                }
            }
            catch (Exception e)
            {
                ChromaLogger.Log(e);
            }

            if (xms != null)
            {
                foreach (XmlColour xm in xms)
                {
                    colors.Add(new NamedColor(xm.name, xm.Color));
                }
            }

            return colors;
        }

        public static void SaveExampleColours()
        {
            try
            {
                string filePath = Environment.CurrentDirectory.Replace('\\', '/') + "/UserData/Chroma/Colours.xml";
                if (!File.Exists(filePath))
                {
                    XmlColour[] exampleColours = new XmlColour[] {
                        new XmlColour("Dark Red", 128, 0, 0, 255),
                        new XmlColour("Mega Blue", 0, 0, 700, 255),
                        new XmlColour("Brown(Why?)", 139, 69, 19, 255),
                    };

                    SaveColours(exampleColours);
                }
            }
            catch (Exception e)
            {
                ChromaLogger.Log(e);
            }
        }

        public static void SaveColours(params XmlColour[] colours)
        {
            if (colours.Length < 1) return;

            string filePath = Environment.CurrentDirectory.Replace('\\', '/') + "/UserData/Chroma/Colours.xml";

            List<NamedColor> namedColors = LoadColoursFromFile();
            List<XmlColour> xmColours = new List<XmlColour>();
            foreach (NamedColor nc in namedColors) xmColours.Add(new XmlColour(nc.name, nc.color.Value));
            foreach (XmlColour xc in colours) xmColours.Add(xc);

            try
            {
                using (StreamWriter w = File.CreateText(filePath))
                {
                    XmlSerializer ser = new XmlSerializer(xmColours.GetType());
                    ser.Serialize(w, xmColours);
                }
            }
            catch (Exception e)
            {
                ChromaLogger.Log(e);
            }
        }

        #endregion savedColours
    }
}