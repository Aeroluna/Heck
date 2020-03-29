using Chroma.Extensions;
using Chroma.Settings;
using IPA.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.VFX
{
    internal class TechnicolourController : MonoBehaviour
    {
        internal static bool Instantiated()
        {
            return _instance != null;
        }

        internal static TechnicolourController Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject gameObject = new GameObject("Chroma_TechnicolourController");
                    _instance = gameObject.AddComponent<TechnicolourController>();
                    _instance.bpm = ChromaBehaviour.songBPM;

                    _instance.match = ChromaConfig.MatchTechnicolourSabers;
                    _instance.mismatchSpeedOffset = ChromaConfig.MatchTechnicolourSabers ? 0 : 0.5f;
                }
                return _instance;
            }
        }

        private static TechnicolourController _instance;

        internal static void Clear()
        {
            if (_instance != null) Destroy(_instance.gameObject);
            _instance = null;
        }

        private event TechnicolourUpdateDelegate UpdateTechnicolourEvent;

        private delegate void TechnicolourUpdateDelegate();

        private float bpm;
        private float secondsPerBeat = 0.5f;

        internal Color gradientColor { get; private set; }
        internal Color gradientLeftColor { get; private set; }
        internal Color gradientRightColor { get; private set; }

        internal Color?[] rainbowSaberColours = new Color?[] { null, null };

        internal List<ColorNoteVisuals> _colorNoteVisuals = new List<ColorNoteVisuals>();
        internal List<StretchableObstacle> _stretchableObstacles = new List<StretchableObstacle>();
        internal List<NoteController> _bombControllers = new List<NoteController>();

        internal static void InitializeGradients()
        {
            if (ColourManager.TechnicolourLights && (ChromaConfig.TechnicolourLightsStyle == ColourManager.TechnicolourStyle.GRADIENT))
                Instance.UpdateTechnicolourEvent += Instance.RainbowLights;
            if (ColourManager.TechnicolourBlocks && (ChromaConfig.TechnicolourBlocksStyle == ColourManager.TechnicolourStyle.GRADIENT))
                Instance.UpdateTechnicolourEvent += Instance.RainbowNotes;
            if (ColourManager.TechnicolourBarriers && (ChromaConfig.TechnicolourWallsStyle == ColourManager.TechnicolourStyle.GRADIENT))
                Instance.UpdateTechnicolourEvent += Instance.RainbowWalls;
            if (ColourManager.TechnicolourBombs && (ChromaConfig.TechnicolourBombsStyle == ColourManager.TechnicolourStyle.GRADIENT))
                Instance.UpdateTechnicolourEvent += Instance.RainbowBombs;

            // sabers use this script regardless of technicolour style
            if (ColourManager.TechnicolourSabers)
            {
                switch (ChromaConfig.TechnicolourSabersStyle)
                {
                    case ColourManager.TechnicolourStyle.GRADIENT:
                        Instance.UpdateTechnicolourEvent += Instance.GradientTick;
                        break;

                    case ColourManager.TechnicolourStyle.ANY_PALETTE:
                        Instance.SetupEither();
                        Instance.UpdateTechnicolourEvent += Instance.PaletteTick;
                        break;

                    case ColourManager.TechnicolourStyle.PURE_RANDOM:
                        Instance.SetupRandom();
                        Instance.UpdateTechnicolourEvent += Instance.RandomTick;
                        break;

                    default:
                        Instance.SetupWarmCold();
                        Instance.UpdateTechnicolourEvent += Instance.PaletteTick;
                        break;
                }
                Instance.UpdateTechnicolourEvent += Instance.RainbowSabers;
            }
        }

        private void Update()
        {
            secondsPerBeat = (60f / bpm);

            float timeMult = 0.1f;
            float timeGlobalMult = 0.2f;
            gradientColor = Color.HSVToRGB(Mathf.Repeat((Time.time * timeGlobalMult) / secondsPerBeat, 1f), 1f, 1f);
            gradientLeftColor = Color.HSVToRGB(Mathf.Repeat(((Time.time * timeMult) / secondsPerBeat) + mismatchSpeedOffset, 1f), 1f, 1f);
            gradientRightColor = Color.HSVToRGB(Mathf.Repeat((Time.time * timeMult) / secondsPerBeat, 1f), 1f, 1f);

            UpdateTechnicolourEvent?.Invoke();
        }

        private bool match;
        private float mismatchSpeedOffset = 0;

        private Color[] leftSaberPalette;
        private Color[] rightSaberPalette;

        private void RainbowLights()
        {
            LightSwitchEventEffectExtensions.SetAllLightingColours(gradientLeftColor, gradientRightColor);
            LightSwitchEventEffectExtensions.SetAllActiveColours();
        }

        private void RainbowNotes()
        {
            foreach (ColorNoteVisuals n in _colorNoteVisuals)
            {
                Color color;
                try
                {
                    color = n.GetField<NoteController, ColorNoteVisuals>("_noteController").noteData.noteType == NoteType.NoteA ? gradientLeftColor : gradientRightColor;
                }
                catch
                {
                    color = gradientColor;
                }

                SpriteRenderer _arrowGlowSpriteRenderer = n.GetField<SpriteRenderer, ColorNoteVisuals>("_arrowGlowSpriteRenderer");
                SpriteRenderer _circleGlowSpriteRenderer = n.GetField<SpriteRenderer, ColorNoteVisuals>("_circleGlowSpriteRenderer");
                MaterialPropertyBlockController[] _materialPropertyBlockControllers = n.GetField<MaterialPropertyBlockController[], ColorNoteVisuals>("_materialPropertyBlockControllers");

                n.SetField("_noteColor", color);
                _arrowGlowSpriteRenderer.color = color.ColorWithAlpha(n.GetField<float, ColorNoteVisuals>("_arrowGlowIntensity"));
                _circleGlowSpriteRenderer.color = color;
                foreach (MaterialPropertyBlockController materialPropertyBlockController in _materialPropertyBlockControllers)
                {
                    materialPropertyBlockController.materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), color.ColorWithAlpha(1f));
                    materialPropertyBlockController.ApplyChanges();
                }
            }
            ColourManager.SetNoteTypeColourOverride(NoteType.NoteA, gradientLeftColor);
            ColourManager.SetNoteTypeColourOverride(NoteType.NoteB, gradientRightColor);
        }

        private void RainbowWalls()
        {
            foreach (StretchableObstacle n in _stretchableObstacles)
            {
                ParametricBoxFrameController _obstacleFrame = n.GetField<ParametricBoxFrameController, StretchableObstacle>("_obstacleFrame");
                ParametricBoxFakeGlowController _obstacleFakeGlow = n.GetField<ParametricBoxFakeGlowController, StretchableObstacle>("_obstacleFakeGlow");
                MaterialPropertyBlockController[] _materialPropertyBlockControllers = n.GetField<MaterialPropertyBlockController[], StretchableObstacle>("_materialPropertyBlockControllers");
                float _addColorMultiplier = n.GetField<float, StretchableObstacle>("_addColorMultiplier");

                _obstacleFrame.color = gradientColor;
                _obstacleFrame.Refresh();
                _obstacleFakeGlow.color = gradientColor;
                _obstacleFakeGlow.Refresh();
                Color value = gradientColor * _addColorMultiplier;
                value.a = 0f;
                foreach (MaterialPropertyBlockController materialPropertyBlockController in _materialPropertyBlockControllers)
                {
                    materialPropertyBlockController.materialPropertyBlock.SetColor(Shader.PropertyToID("_AddColor"), value);
                    materialPropertyBlockController.materialPropertyBlock.SetColor(Shader.PropertyToID("_TintColor"), gradientColor);
                    materialPropertyBlockController.ApplyChanges();
                }
            }
        }

        private void RainbowBombs()
        {
            foreach (NoteController n in _bombControllers)
            {
                Material mat = n.noteTransform.gameObject.GetComponent<Renderer>().material;
                mat.SetColor("_SimpleColor", gradientColor);
            }
        }

        private void RainbowSabers()
        {
            foreach (SaberColourizer saber in SaberColourizer.saberColourizers)
            {
                saber.Colourize(saber.warm ? rainbowSaberColours[0].Value : rainbowSaberColours[1].Value);
            }
        }

        /*
         * PALETTED
         */

        private void PaletteTick()
        {
            rainbowSaberColours[0] = ColourManager.GetLerpedFromArray(leftSaberPalette, (Time.time + mismatchSpeedOffset) / secondsPerBeat);
            rainbowSaberColours[1] = ColourManager.GetLerpedFromArray(rightSaberPalette, (Time.time) / secondsPerBeat);
        }

        private void GradientTick()
        {
            rainbowSaberColours[0] = gradientLeftColor;
            rainbowSaberColours[1] = gradientRightColor;
        }

        private void SetupWarmCold()
        {
            leftSaberPalette = ColourManager.TechnicolourWarmPalette;
            rightSaberPalette = ColourManager.TechnicolourColdPalette;
        }

        private void SetupEither()
        {
            leftSaberPalette = ColourManager.TechnicolourCombinedPalette;
            rightSaberPalette = ColourManager.TechnicolourCombinedPalette;
        }

        /*
         * TRUE RANDOM
         */

        private float lastTime = 0;
        private float h = 0;
        private Color[] randomCycleLeft = new Color[2];
        private Color[] randomCycleRight = new Color[2];

        private void RandomTick()
        {
            h += (Time.time - lastTime) / secondsPerBeat;
            if (h > 1)
            {
                h = 0;
                RandomCycleNext();
            }
            rainbowSaberColours[0] = Color.Lerp(randomCycleLeft[0], randomCycleLeft[1], h);
            rainbowSaberColours[1] = Color.Lerp(randomCycleRight[0], randomCycleRight[1], h);
            lastTime = Time.time;
        }

        private void RandomCycleNext()
        {
            randomCycleLeft[0] = randomCycleLeft[1];
            randomCycleRight[0] = randomCycleRight[1];
            randomCycleLeft[1] = Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f);
            if (match)
            {
                randomCycleRight = randomCycleLeft;
            }
            else
            {
                randomCycleRight[1] = Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f);
            }
        }

        private void SetupRandom()
        {
            randomCycleLeft = new Color[] { Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f), Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f) };
            randomCycleRight = new Color[] { Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f), Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f) };
        }
    }
}