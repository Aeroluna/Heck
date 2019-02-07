using Chroma.Settings;
using Chroma.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.VFX {

    public class VFXRainbowSabers : MonoBehaviour {

        public static Color[] rainbowSaberColours = null;

        internal static void RegisterListeners() {
            ChromaPlugin.MainMenuLoadedEvent += MainMenuLoaded;
        }

        private static void MainMenuLoaded() {
            rainbowSaberColours = null;
        }

        private const float rainbowSabersUpdate = 0.12f;

        public static void Instantiate(Saber[] sabers, float bpm, bool smooth, bool match, float mismatchSpeedMult) {
            rainbowSaberColours = new Color[] { Color.white, Color.white };
            GameObject gameObject = new GameObject("Chroma_VFXRainbowSabers");
            VFXRainbowSabers vfx = gameObject.AddComponent<VFXRainbowSabers>();
            vfx.bpm = bpm;
            vfx.smooth = smooth;
            vfx.match = match;
            vfx.mismatchSpeedMult = match ? 1f : mismatchSpeedMult;
            vfx.saberColourizers = new SaberColourizer[sabers.Length];
            for (int i = 0; i < sabers.Length; i++) {
                vfx.saberColourizers[i] = new SaberColourizer(sabers[i]);
            }
            vfx.Init();
        }

        SaberColourizer[] saberColourizers;

        private float bpm;
        private bool smooth;
        private bool match;
        private float mismatchSpeedMult = 1f;
        private float matchOffset;

        Color[] leftPalette;
        Color[] rightPalette;
        float secondsPerBeat = 0.5f;

        void Init() {
            matchOffset = ColourManager.TechnicolourCombinedPalette.Length / 2f;
            switch (ChromaConfig.TechnicolourSabersStyle) {
                case ColourManager.TechnicolourStyle.ANY_PALETTE:
                    SetupEither();
                    StartCoroutine(SmoothRainbowSabers(PaletteTick));
                    break;
                case ColourManager.TechnicolourStyle.PURE_RANDOM:
                    SetupRandom();
                    StartCoroutine(SmoothRainbowSabers(RandomTick));
                    break;
                default:
                    SetupWarmCold();
                    StartCoroutine(SmoothRainbowSabers(PaletteTick));
                    break;
            }
        }




        private IEnumerator SmoothRainbowSabers(Action action) {

            secondsPerBeat = (60f / bpm);
            while (true) {
                yield return new WaitForSeconds(rainbowSabersUpdate);
                try {

                    action();

                    foreach (SaberColourizer saber in saberColourizers) {
                        saber.Colourize(saber.warm ? rainbowSaberColours[0] : rainbowSaberColours[1]);
                    }
                } catch (Exception e) {
                    ChromaLogger.Log(e);
                    this.StopAllCoroutines();
                }
            }
        }

        /*
         * PALETTED
         */

        private void PaletteTick() {
            rainbowSaberColours[0] = ColourManager.GetLerpedFromArray(leftPalette, (Time.time * mismatchSpeedMult) / secondsPerBeat);
            rainbowSaberColours[1] = ColourManager.GetLerpedFromArray(rightPalette, (Time.time) / secondsPerBeat);
        }

        private void SetupWarmCold() {
            leftPalette = ColourManager.TechnicolourWarmPalette;
            rightPalette = ColourManager.TechnicolourColdPalette;
        }

        private void SetupEither() {
            leftPalette = ColourManager.TechnicolourCombinedPalette;
            rightPalette = ColourManager.TechnicolourCombinedPalette;
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

        //Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f);
        //UnityEngine.Random.ColorHSV().ColorWithValue(1);



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
                    meshRenderers[i] = glowColors[i].GetField<MeshRenderer>("_meshRenderer");

                    blocks[i] = glowColors[i].GetField<MaterialPropertyBlock>("_materialPropertyBlock");
                    if (blocks[i] == null) {
                        blocks[i] = new MaterialPropertyBlock();
                        glowColors[i].SetField("_materialPropertyBlock", blocks[i]);
                    }
                    tintPairs[i] = glowColors[i].GetField<SetSaberGlowColor.PropertyTintColorPair[]>("_propertyTintColorPairs");
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
