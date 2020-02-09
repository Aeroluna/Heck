using Chroma.Settings;
using Chroma.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using IPA.Utilities;

namespace Chroma.VFX {

    public class TechnicolourController : MonoBehaviour {

        private const float rainbowUpdateInterval = 0.04f;

        public static bool Instantiated() {
            return _instance != null;
        }

        public static TechnicolourController Instance {
            get {
                if (_instance == null) {
                    GameObject gameObject = new GameObject("Chroma_TechnicolourController");
                    _instance = gameObject.AddComponent<TechnicolourController>();
                    _instance.bpm = ChromaBehaviour.songBPM;
                    ChromaLogger.Log("Technicolour Controller Created");

                    _instance.match = ChromaConfig.MatchTechnicolourSabers;
                    _instance.mismatchSpeedOffset = ChromaConfig.MatchTechnicolourSabers ? 0 : 0.5f;
                }
                return _instance;
            }
        }
        private static TechnicolourController _instance;

        public static void Clear() {
            if (_instance != null) Destroy(_instance.gameObject);
            _instance = null;
        }

        private event TechnicolourUpdateDelegate UpdateTechnicolourEvent;
        private delegate void TechnicolourUpdateDelegate();

        private float bpm;
        private float secondsPerBeat = 0.5f;

        private Color gradientColor;
        private Color gradientLeftColor;
        private Color gradientRightColor;

        public Color?[] rainbowSaberColours = new Color?[] { null, null };

        public List<ColorNoteVisuals> _colorNoteVisuals = new List<ColorNoteVisuals>();
        public List<StretchableObstacle> _stretchableObstacles = new List<StretchableObstacle>();
        public Dictionary<LightSwitchEventEffect, int> _lightSwitchLastValue = new Dictionary<LightSwitchEventEffect, int>();
        public List<NoteController> _bombControllers = new List<NoteController>();

        public static void InitializeGradients() {
            if (ColourManager.TechnicolourLights && (ChromaConfig.TechnicolourLightsStyle == ColourManager.TechnicolourStyle.GRADIENT))
                Instance.UpdateTechnicolourEvent += Instance.RainbowLights;
            if (ColourManager.TechnicolourBlocks && (ChromaConfig.TechnicolourBlocksStyle == ColourManager.TechnicolourStyle.GRADIENT))
                Instance.UpdateTechnicolourEvent += Instance.RainbowNotes;
            if (ColourManager.TechnicolourBarriers && (ChromaConfig.TechnicolourWallsStyle == ColourManager.TechnicolourStyle.GRADIENT))
                Instance.UpdateTechnicolourEvent += Instance.RainbowWalls;
            if (ColourManager.TechnicolourBombs && (ChromaConfig.TechnicolourBombsStyle == ColourManager.TechnicolourStyle.GRADIENT))
                Instance.UpdateTechnicolourEvent += Instance.RainbowBombs;
        }

        public static void InitializeSabers(Saber[] sabers) {
            Instance.saberColourizers = new SaberColourizer[sabers.Length];
            for (int i = 0; i < sabers.Length; i++) {
                Instance.saberColourizers[i] = new SaberColourizer(sabers[i]);
            }
            Instance.SabersInit();
        }

        void Update() {
            secondsPerBeat = (60f / bpm);
            try {
                float timeMult = 0.1f;
                float timeGlobalMult = 0.2f;
                gradientColor = Color.HSVToRGB(Mathf.Repeat((Time.time * timeGlobalMult) / secondsPerBeat, 1f), 1f, 1f);
                gradientLeftColor = Color.HSVToRGB(Mathf.Repeat(((Time.time * timeMult) / secondsPerBeat) + mismatchSpeedOffset, 1f), 1f, 1f);
                gradientRightColor = Color.HSVToRGB(Mathf.Repeat((Time.time * timeMult) / secondsPerBeat, 1f), 1f, 1f);

                UpdateTechnicolourEvent?.Invoke();
            }
            catch (Exception e) {
                ChromaLogger.Log(e);
                StopAllCoroutines();
            }
        }

        SaberColourizer[] saberColourizers;
        private bool match;
        private float mismatchSpeedOffset = 0;

        Color[] leftSaberPalette;
        Color[] rightSaberPalette;

        void SabersInit() {
            switch (ChromaConfig.TechnicolourSabersStyle) {
                case ColourManager.TechnicolourStyle.GRADIENT:
                    UpdateTechnicolourEvent += GradientTick;
                    break;
                case ColourManager.TechnicolourStyle.ANY_PALETTE:
                    SetupEither();
                    UpdateTechnicolourEvent += PaletteTick;
                    break;
                case ColourManager.TechnicolourStyle.PURE_RANDOM:
                    SetupRandom();
                    UpdateTechnicolourEvent += RandomTick;
                    break;
                default:
                    SetupWarmCold();
                    UpdateTechnicolourEvent += PaletteTick;
                    break;
            }
            UpdateTechnicolourEvent += RainbowSabers;
        }

        private void RainbowLights() {
            foreach (KeyValuePair<LightSwitchEventEffect, int> n in _lightSwitchLastValue) {

                if (n.Value == 0) continue;

                String warm;
                switch (n.Value) {
                    case 1: case 2: case 3:
                    default:
                        warm = "0";
                        break;
                    case 5: case 6: case 7:
                        warm = "1";
                        break;
                }
                float _offColorIntensity = n.Key.GetPrivateField<float>("_offColorIntensity");
                ColourManager.RecolourAllLights(gradientLeftColor, gradientRightColor);

                Color c;
                switch (n.Value) {
                    case 1: case 5:
                    default:
                        c = n.Key.GetPrivateField<MultipliedColorSO>("_lightColor" + warm).color;
                        break;
                    case 2: case 6: case 3: case 7:
                        c = n.Key.GetPrivateField<MultipliedColorSO>("_highlightColor" + warm).color;
                        break;
                }

                if (n.Key.enabled) {
                    n.Key.SetPrivateField("_highlightColor", c);

                    if (n.Value == 3 || n.Value == 7) {
                        n.Key.SetPrivateField("_afterHighlightColor", c.ColorWithAlpha(_offColorIntensity));
                    }
                    else {
                        n.Key.SetPrivateField("_afterHighlightColor", c);
                    }
                }
                else {
                    if (n.Value == 1 || n.Value == 5 || n.Value == 2 || n.Value == 6) {
                        n.Key.SetColor(c);
                    }
                }
                n.Key.SetPrivateField("_offColor", c.ColorWithAlpha(_offColorIntensity));
                
            }
        }

        private void RainbowNotes() {
            foreach (ColorNoteVisuals n in _colorNoteVisuals) {
                Color color;
                try {
                    color = n.GetPrivateField<NoteController>("_noteController").noteData.noteType == NoteType.NoteA ? gradientLeftColor : gradientRightColor;
                }
                catch {
                    color = gradientColor;
                }

                SpriteRenderer _arrowGlowSpriteRenderer = n.GetPrivateField<SpriteRenderer>("_arrowGlowSpriteRenderer");
                SpriteRenderer _circleGlowSpriteRenderer = n.GetPrivateField<SpriteRenderer>("_circleGlowSpriteRenderer");
                MaterialPropertyBlockController[] _materialPropertyBlockControllers = n.GetPrivateField<MaterialPropertyBlockController[]>("_materialPropertyBlockControllers");

                n.SetPrivateField("_noteColor", color);
                _arrowGlowSpriteRenderer.color = color.ColorWithAlpha(n.GetPrivateField<float>("_arrowGlowIntensity"));
                _circleGlowSpriteRenderer.color = color;
                foreach (MaterialPropertyBlockController materialPropertyBlockController in _materialPropertyBlockControllers) {
                    materialPropertyBlockController.materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), color.ColorWithAlpha(1f));
                    materialPropertyBlockController.ApplyChanges();
                }
            }
            ColourManager.SetNoteTypeColourOverride(NoteType.NoteA, gradientLeftColor);
            ColourManager.SetNoteTypeColourOverride(NoteType.NoteB, gradientRightColor);
        }

        private void RainbowWalls() {
            foreach (StretchableObstacle n in _stretchableObstacles) {
                ParametricBoxFrameController _obstacleFrame = n.GetPrivateField<ParametricBoxFrameController>("_obstacleFrame");
                ParametricBoxFakeGlowController _obstacleFakeGlow = n.GetPrivateField<ParametricBoxFakeGlowController>("_obstacleFakeGlow");
                MaterialPropertyBlockController[] _materialPropertyBlockControllers = n.GetPrivateField<MaterialPropertyBlockController[]>("_materialPropertyBlockControllers");
                float _addColorMultiplier = n.GetPrivateField<float>("_addColorMultiplier");

                _obstacleFrame.color = gradientColor;
                _obstacleFrame.Refresh();
                _obstacleFakeGlow.color = gradientColor;
                _obstacleFakeGlow.Refresh();
                Color value = gradientColor * _addColorMultiplier;
                value.a = 0f;
                foreach (MaterialPropertyBlockController materialPropertyBlockController in _materialPropertyBlockControllers) {
                    materialPropertyBlockController.materialPropertyBlock.SetColor(Shader.PropertyToID("_AddColor"), value);
                    materialPropertyBlockController.materialPropertyBlock.SetColor(Shader.PropertyToID("_TintColor"), gradientColor);
                    materialPropertyBlockController.ApplyChanges();
                }
            }
        }

        private void RainbowBombs() {
            foreach (NoteController n in _bombControllers) {
                Material mat = n.noteTransform.gameObject.GetComponent<Renderer>().material;
                mat.SetColor("_SimpleColor", gradientColor);
            }
        }

        private void RainbowSabers() {
            foreach (SaberColourizer saber in saberColourizers) {
                saber.Colourize(saber.warm ? (Color)rainbowSaberColours[0] : (Color)rainbowSaberColours[1]);
            }
        }

        /*
         * PALETTED
         */

        private void PaletteTick() {
            rainbowSaberColours[0] = ColourManager.GetLerpedFromArray(leftSaberPalette, (Time.time + mismatchSpeedOffset) / secondsPerBeat);
            rainbowSaberColours[1] = ColourManager.GetLerpedFromArray(rightSaberPalette, (Time.time) / secondsPerBeat);
        }

        private void GradientTick() {
            rainbowSaberColours[0] = gradientLeftColor;
            rainbowSaberColours[1] = gradientRightColor;
        }

        private void SetupWarmCold() {
            leftSaberPalette = ColourManager.TechnicolourWarmPalette;
            rightSaberPalette = ColourManager.TechnicolourColdPalette;
        }

        private void SetupEither() {
            leftSaberPalette = ColourManager.TechnicolourCombinedPalette;
            rightSaberPalette = ColourManager.TechnicolourCombinedPalette;
        }

        /*
         * TRUE RANDOM
         */
         
        float lastTime = 0;
        float h = 0;
        Color[] randomCycleLeft = new Color[2];
        Color[] randomCycleRight = new Color[2];
        private void RandomTick() {
            h += (Time.time - lastTime) / secondsPerBeat;
            if (h > 1) {
                h = 0;
                RandomCycleNext();
            }
            rainbowSaberColours[0] = Color.Lerp(randomCycleLeft[0], randomCycleLeft[1], h);
            rainbowSaberColours[1] = Color.Lerp(randomCycleRight[0], randomCycleRight[1], h);
            lastTime = Time.time;
        }

        private void RandomCycleNext() {
            randomCycleLeft[0] = randomCycleLeft[1];
            randomCycleRight[0] = randomCycleRight[1];
            randomCycleLeft[1] = Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f);
            if (match) {
                randomCycleRight = randomCycleLeft;
            } else {
                randomCycleRight[1] = Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f);
            }
        }

        private void SetupRandom() {
            randomCycleLeft = new Color[] { Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f), Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f) };
            randomCycleRight = new Color[] { Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f), Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f) };
        }



        private class SaberColourizer {

            public bool warm;

            SetSaberGlowColor[] glowColors;
            MeshRenderer[] meshRenderers;
            MaterialPropertyBlock[] blocks;
            SetSaberGlowColor.PropertyTintColorPair[][] tintPairs;

            List<Material> customMats = new List<Material>();

            public SaberColourizer(Saber saber) {
                warm = saber.saberType == Saber.SaberType.SaberA;

                glowColors = saber.GetComponentsInChildren<SetSaberGlowColor>();
                meshRenderers = new MeshRenderer[glowColors.Length];
                blocks = new MaterialPropertyBlock[glowColors.Length];
                tintPairs = new SetSaberGlowColor.PropertyTintColorPair[glowColors.Length][];
                for (int i = 0; i < glowColors.Length; i++) {
                    meshRenderers[i] = glowColors[i].GetPrivateField<MeshRenderer>("_meshRenderer");

                    blocks[i] = glowColors[i].GetPrivateField<MaterialPropertyBlock>("_materialPropertyBlock");
                    if (blocks[i] == null) {
                        blocks[i] = new MaterialPropertyBlock();
                        glowColors[i].SetPrivateField("_materialPropertyBlock", blocks[i]);
                    }
                    tintPairs[i] = glowColors[i].GetPrivateField<SetSaberGlowColor.PropertyTintColorPair[]>("_propertyTintColorPairs");
                    meshRenderers[i].SetPropertyBlock(blocks[i], 0);
                }

                //Custom sabers??
                Renderer[] renderers = saber.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++) {
                    foreach (Material material in renderers[i].materials) {
                        if ((material.HasProperty("_Glow") && material.GetFloat("_Glow") > 0f) || (material.HasProperty("_Bloom") && material.GetFloat("_Bloom") > 0f)) {
                            customMats.Add(material);
                        }
                    }
                }
            }

            public void Colourize(Color color) {
                for (int i = 0; i < glowColors.Length; i++) {

                    for (int j = 0; j < tintPairs[i].Length; j++) {
                        blocks[i].SetColor(tintPairs[i][j].property, color * tintPairs[i][j].tintColor);
                    }

                    meshRenderers[i].SetPropertyBlock(blocks[i], 0);
                }

                foreach (Material material in customMats) {
                    material.SetColor("_Color", color);
                }
            }

        }

    }

}
