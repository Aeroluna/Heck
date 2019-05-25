using Chroma.Colours;
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

    public class VFXRainbowSaberModule : MonoBehaviour {

        private const float RainbowSaberUpdateRate = 0.12f;

        public static void Rainbowify(Saber saber, ColourController controller, float bpm) {
            GameObject gameObject = new GameObject("Chroma_VFXRainbowSaber");
            gameObject.transform.SetParent(saber.transform, false);
            VFXRainbowSaberModule vfx = gameObject.AddComponent<VFXRainbowSaberModule>();
            vfx.controller = controller;
            vfx.bpm = bpm;
            vfx.saberColourizer = new SaberColourizer(saber);
            vfx.Init();
        }

        SaberColourizer saberColourizer;
        ColourController controller;

        private float bpm;
        private bool match;
        private float matchOffset = 0f;
        
        float secondsPerBeat = 0.5f;

        void Init() {
            if (!ChromaConfig.MatchTechnicolourSabers) matchOffset = UnityEngine.Random.value * 80f;
        }

        private IEnumerator SmoothRainbowSaber(Action action) {

            secondsPerBeat = (60f / bpm);
            while (true) {
                yield return new WaitForSeconds(RainbowSaberUpdateRate);
                try {

                    action();

                    saberColourizer.Colourize(controller.GetColor((Time.time + matchOffset) / secondsPerBeat));
                    
                } catch (Exception e) {
                    ChromaLogger.Log(e);
                    this.StopAllCoroutines();
                }
            }
        }

        private class SaberColourizer {
            
            SetSaberGlowColor[] glowColors;
            MeshRenderer[] meshRenderers;
            MaterialPropertyBlock[] blocks;
            SetSaberGlowColor.PropertyTintColorPair[][] tintPairs;

            List<Material> customMats = new List<Material>();

            public SaberColourizer(Saber saber) {

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
