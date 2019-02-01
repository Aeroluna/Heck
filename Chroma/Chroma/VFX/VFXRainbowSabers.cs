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

        void Init() {
            matchOffset = ColourManager.TechnicolourCombinedPalette.Length / 2f;
            switch (ChromaConfig.TechnicolourSabersStyle) {
                case ColourManager.TechnicolourStyle.ANY_PALETTE:
                    StartCoroutine(SmoothRainbowSabers(Either));
                    break;
                case ColourManager.TechnicolourStyle.PURE_RANDOM:
                    StartCoroutine(SmoothRainbowSabers(PureRandom));
                    break;
                default:
                    StartCoroutine(SmoothRainbowSabers(WarmCold));
                    break;
            }
        }


        float secondsPerBeat = 0.5f;
        private IEnumerator SmoothRainbowSabers(Action<bool> action) {
            secondsPerBeat = (60f / bpm);
            action(true);
            while (true) {
                yield return new WaitForSeconds(rainbowSabersUpdate);
                try {

                    DoTick(action);

                    foreach (SaberColourizer saber in saberColourizers) {
                        saber.Colourize(saber.warm ? rainbowSaberColours[0] : rainbowSaberColours[1]);
                    }
                } catch (Exception e) {
                    ChromaLogger.Log(e);
                    this.StopAllCoroutines();
                }
            }
        }

        Color[] colorCycleLeft = new Color[2];
        Color[] colorCycleRight = new Color[2];
        float h = 0;
        float h2 = 0;
        private void DoTick(Action<bool> action) {
            h += (Time.time / secondsPerBeat) * 45f;
            if (h > 1) {
                h = 0;
                CycleColours(action);
            }
            rainbowSaberColours[0] = Color.Lerp(colorCycleLeft[0], colorCycleLeft[1], h2);
            rainbowSaberColours[1] = Color.Lerp(colorCycleLeft[0], colorCycleLeft[1], h);
        }

        private void CycleColours(Action<bool> action) {
            colorCycleLeft[0] = colorCycleLeft[1];
            colorCycleRight[0] = colorCycleRight[1];
            action(false);
        }

        private void WarmCold(bool initial) {
            if (initial) {
                colorCycleLeft[0] = ColourManager.A;
                colorCycleRight[0] = ColourManager.B;
            }
            colorCycleLeft[1] = ColourManager.GetTechnicolour(true, (((Time.time * mismatchSpeedMult) / secondsPerBeat) * 45f) + secondsPerBeat, ColourManager.TechnicolourStyle.WARM_COLD);
            colorCycleRight[1] = ColourManager.GetTechnicolour(false, ((Time.time / secondsPerBeat) * 45f) + secondsPerBeat, ColourManager.TechnicolourStyle.WARM_COLD);

            //rainbowSaberColours[0] = ColourManager.GetLerpedFromArray(ColourManager.TechnicolourWarmPalette, (Time.time / secondsPerBeat) * mismatchSpeedMult);
            //rainbowSaberColours[1] = ColourManager.GetLerpedFromArray(ColourManager.TechnicolourColdPalette, Time.time / secondsPerBeat);
        }

        private void Either(bool initial) {
            if (initial) {
                colorCycleLeft[0] = ColourManager.A;
                colorCycleRight[0] = ColourManager.B;
            }
            colorCycleLeft[1] = ColourManager.GetTechnicolour(true, (((Time.time * mismatchSpeedMult) / secondsPerBeat) * 45f) + secondsPerBeat, ColourManager.TechnicolourStyle.ANY_PALETTE);
            colorCycleRight[1] = ColourManager.GetTechnicolour(false, ((Time.time / secondsPerBeat) * 45f) + secondsPerBeat, ColourManager.TechnicolourStyle.ANY_PALETTE);

            //rainbowSaberColours[0] = ColourManager.GetLerpedFromArray(ColourManager.TechnicolourCombinedPalette, (Time.time / secondsPerBeat) * mismatchSpeedMult);
            //rainbowSaberColours[1] = ColourManager.GetLerpedFromArray(ColourManager.TechnicolourCombinedPalette, (Time.time / secondsPerBeat) + (match ? matchOffset : 0));
        }
        
        private void PureRandom(bool initial) {
            colorCycleLeft = new Color[] { UnityEngine.Random.ColorHSV(), UnityEngine.Random.ColorHSV() };
            if (match) {
                colorCycleRight = colorCycleLeft;
            } else {
                colorCycleRight = new Color[] { UnityEngine.Random.ColorHSV(), UnityEngine.Random.ColorHSV() };
            }
            /*h += (Time.time / secondsPerBeat) * 45f;
            rainbowSaberColours[0] = Color.HSVToRGB(Mathf.Repeat(h * mismatchSpeedMult, 360f) / 360f, 1f, 1f);
            rainbowSaberColours[1] = Color.HSVToRGB(Mathf.Repeat(h + (match ? matchOffset : 0), 360f) / 360f, 1f, 1f);*/
        }

        private void NextRandoms() {

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

            /*MaterialPropertyBlock block;
            SetSaberGlowColor.PropertyTintColorPair[] glowTintPairs;
            MeshRenderer meshRenderer;

            List<Material> customSaberMaterials = new List<Material>();

            public SaberColourizer(Saber saber) {

                warm = saber.saberType == Saber.SaberType.SaberA;

                SetSaberGlowColor[] glowColors = saber.GetComponentsInChildren<SetSaberGlowColor>();
                foreach (SetSaberGlowColor glowColor in glowColors) {
                    
                    meshRenderer = glowColor.GetField<MeshRenderer>("_meshRenderer");

                    block = glowColor.GetField<MaterialPropertyBlock>("_materialPropertyBlock");
                    if (block == null) {
                        block = new MaterialPropertyBlock();
                        glowColor.SetField("_materialPropertyBlock", block);
                    }
                    glowTintPairs = glowColor.GetField<SetSaberGlowColor.PropertyTintColorPair[]>("_propertyTintColorPairs");
                    meshRenderer.SetPropertyBlock(block, 0);
                }

                //Custom sabers??
                Renderer[] renderers = saber.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++) {
                    foreach (Material material in renderers[i].materials) {
                        if ((material.HasProperty("_Glow") && material.GetFloat("_Glow") > 0f) || (material.HasProperty("_Bloom") && material.GetFloat("_Bloom") > 0f)) {
                            customSaberMaterials.Add(material);
                        }
                    }
                }

            }

            public void Colourize(Color color) {
                foreach (SetSaberGlowColor.PropertyTintColorPair propertyTintColorPair in glowTintPairs) {
                    block.SetColor(propertyTintColorPair.property, color * propertyTintColorPair.tintColor);
                }
                foreach (Material m in customSaberMaterials) m.SetColor("_Color", color);
                meshRenderer.SetPropertyBlock(block, 0);
            }*/

        }

    }

}
