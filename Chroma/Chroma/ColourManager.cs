using Chroma.Extensions;
using Chroma.Settings;
using BS_Utils.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chroma
{
    internal static class ColourManager
    {
        private static Color?[] noteTypeColourOverrides = new Color?[] { null, null };

        internal static Color? GetNoteTypeColourOverride(NoteType noteType)
        {
            return noteTypeColourOverrides[noteType == NoteType.NoteA ? 0 : 1];
        }

        internal static void SetNoteTypeColourOverride(NoteType noteType, Color color)
        {
            noteTypeColourOverrides[noteType == NoteType.NoteA ? 0 : 1] = color;
        }

        internal static void RemoveNoteTypeColourOverride(NoteType noteType)
        {
            noteTypeColourOverrides[noteType == NoteType.NoteA ? 0 : 1] = null;
        }

        /*
         * TECHNICOLOUR
         */

        #region technicolour

        private static Color[] _technicolourWarmPalette;
        private static Color[] _technicolourColdPalette;

        internal static Color[] TechnicolourCombinedPalette { get; private set; }

        internal static Color[] TechnicolourWarmPalette
        {
            get { return _technicolourWarmPalette; }
            set
            {
                _technicolourWarmPalette = value;
                SetupCombinedTechnicolourPalette();
            }
        }

        internal static Color[] TechnicolourColdPalette
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
        }

        internal enum TechnicolourStyle
        {
            OFF = 0,
            WARM_COLD = 1,
            ANY_PALETTE = 2,
            PURE_RANDOM = 3,
            GRADIENT = 4
        }

        internal enum TechnicolourTransition
        {
            FLAT = 0,
            SMOOTH = 1,
        }

        internal enum TechnicolourLightsGrouping
        {
            STANDARD = 0,
            ISOLATED_GROUP = 1,
            ISOLATED = 2
        }

        internal static bool TechnicolourLightsForceDisabled { get; set; } = false;
        internal static bool TechnicolourBlocksForceDisabled { get; set; } = false;
        internal static bool TechnicolourBarriersForceDisabled { get; set; } = false;
        internal static bool TechnicolourBombsForceDisabled { get; set; } = false;

        internal static bool TechnicolourLights
        {
            get
            {
                return ChromaConfig.TechnicolourEnabled && !TechnicolourLightsForceDisabled && ChromaConfig.TechnicolourLightsStyle != TechnicolourStyle.OFF;
            }
        }

        internal static bool TechnicolourSabers
        {
            get { return ChromaConfig.TechnicolourEnabled && !TechnicolourBlocksForceDisabled && ChromaConfig.TechnicolourSabersStyle != TechnicolourStyle.OFF; }
        }

        internal static bool TechnicolourBlocks
        {
            get { return ChromaConfig.TechnicolourEnabled && !TechnicolourBlocksForceDisabled && ChromaConfig.TechnicolourBlocksStyle != TechnicolourStyle.OFF; }
        }

        internal static bool TechnicolourBarriers
        {
            get { return ChromaConfig.TechnicolourEnabled && !TechnicolourBarriersForceDisabled && ChromaConfig.TechnicolourWallsStyle != TechnicolourStyle.OFF; }
        }

        internal static bool TechnicolourBombs
        {
            get { return ChromaConfig.TechnicolourEnabled && !TechnicolourBombsForceDisabled && ChromaConfig.TechnicolourBombsStyle != TechnicolourStyle.OFF; }
        }

        internal static Color GetTechnicolour(bool warm, float time, TechnicolourStyle style, TechnicolourTransition transition = TechnicolourTransition.FLAT)
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

        internal static Color GetEitherTechnicolour(float time, TechnicolourTransition transition)
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

        internal static Color GetWarmTechnicolour(float time, TechnicolourTransition transition)
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

        internal static Color GetColdTechnicolour(float time, TechnicolourTransition transition)
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

        internal static Color GetRandomFromArray(Color[] colors, float time, float seedMult = 8)
        {
            System.Random rand = new System.Random(Mathf.FloorToInt(seedMult * time));
            return colors[rand.Next(0, colors.Length)];
        }

        internal static Color GetLerpedFromArray(Color[] colors, float time)
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

        internal static Color? LaserPointerColour { get => ChromaConfig.LaserPointer.color; }

        internal static Color? SignA { get => ChromaConfig.NeonSignBottom.color; }

        internal static Color? SignB { get => ChromaConfig.NeonSignTop.color; }

        internal static Color? Platform { get => ChromaConfig.PlatformAccoutrements.color; }

        private static LightSwitchEventEffect[] LightSwitches
        {
            get
            {
                if (_lightSwitches == null) _lightSwitches = Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>();
                return _lightSwitches;
            }
        }

        private static LightSwitchEventEffect[] _lightSwitches = null;

        private static ParticleSystemEventEffect[] ParticleSystems
        {
            get
            {
                if (_particleSystems == null) _particleSystems = Resources.FindObjectsOfTypeAll<ParticleSystemEventEffect>();
                return _particleSystems;
            }
        }

        private static ParticleSystemEventEffect[] _particleSystems = null;

        private static void ResetAllLights()
        {
            foreach (LightSwitchEventEffect light in LightSwitches) light.Reset();
            foreach (ParticleSystemEventEffect particle in ParticleSystems) particle.Reset();
            _lightSwitches = null;
            _particleSystems = null;
        }

        internal static void RecolourAllLights(Color? red, Color? blue)
        {
            RecolourLights(LightSwitches, red, blue);
            RecolourLights(ParticleSystems, red, blue);
        }

        private static void RecolourLights(MonoBehaviour[] lights, Color? red, Color? blue)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].SetLightingColours(red, blue);
            }
        }

        internal static Dictionary<BeatmapEventType, LightSwitchEventEffect> LightSwitchDictionary
        {
            get
            {
                if (_lightSwitchDictionary == null)
                {
                    _lightSwitchDictionary = new Dictionary<BeatmapEventType, LightSwitchEventEffect>();
                    foreach (LightSwitchEventEffect l in Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>())
                    {
                        _lightSwitchDictionary.Add(l.GetPrivateField<BeatmapEventType>("_event"), l);
                    }
                }
                return _lightSwitchDictionary;
            }
        }

        private static Dictionary<BeatmapEventType, LightSwitchEventEffect> _lightSwitchDictionary;

        internal static void ClearLightSwitches()
        {
            _lightSwitchDictionary = null;
        }

        internal static void RecolourMenuStuff(Color? platformLight, Color? laser)
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
        }

        internal static void RecolourNeonSign(Color? colorA, Color? colorB)
        {
            bool Aclear = (colorA == null);
            bool Bclear = (colorB == null);
            TubeBloomPrePassLight[] _prePassLights = UnityEngine.Object.FindObjectsOfType<TubeBloomPrePassLight>();
            foreach (var prePassLight in _prePassLights)
            {
                if (prePassLight != null)
                {
                    if (prePassLight.name == "SaberNeon")
                        prePassLight.color = Aclear ? new Color(0.188f, 0.62f, 1f, 0.8f) : colorA.Value.ColorWithAlpha(0.8f);
                    if (prePassLight.name == "BATNeon" || prePassLight.name == "ENeon")
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
        }

        internal static void RefreshLights()
        {
            ChromaLogger.Log("RERESFHIESRHIDGNDSGI");
            ResetAllLights();
            RecolourNeonSign(SignA, SignB);
            RecolourMenuStuff(Platform, LaserPointerColour);
        }
    }
}