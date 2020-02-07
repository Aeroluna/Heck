using Chroma.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using IPA.Utilities;

namespace Chroma.VFX {

    public class VFXRainbowNotes : MonoBehaviour {

        public static void Instantiate(float bpm) {
            GameObject gameObject = new GameObject("Chroma_VFXRainbowNotes");
            VFXRainbowNotes vfx = gameObject.AddComponent<VFXRainbowNotes>();
            vfx.bpm = bpm;
            vfx.Init();
        }

        private const float rainbowNoteUpdateRate = 0.08f;

        private float bpm;

        public static List<ColorNoteVisuals> _colorNoteVisuals = new List<ColorNoteVisuals>();

        /*
         * RAINBOW
         */

        void Init() {
            StartCoroutine(RainbowNotes());
        }

        Color color;
        public IEnumerator RainbowNotes() {
            float secondsPerBeat = (60f / bpm);
            while (true) {
                yield return new WaitForSeconds(rainbowNoteUpdateRate);
                try {
                    color = ColourManager.GetLerpedFromArray(ColourManager.TechnicolourCombinedPalette, Time.time / secondsPerBeat);
                    foreach (ColorNoteVisuals n in _colorNoteVisuals) {
                        n.SetPrivateField("_noteColor", color);
                        n.GetPrivateField<SpriteRenderer>("_arrowGlowSpriteRenderer").color = color.ColorWithAlpha(n.GetPrivateField<float>("_arrowGlowIntensity"));
                        n.GetPrivateField<SpriteRenderer>("_circleGlowSpriteRenderer").color = color;
                        foreach (MaterialPropertyBlockController materialPropertyBlockController in n.GetPrivateField<MaterialPropertyBlockController[]>("_materialPropertyBlockControllers")) {
                            materialPropertyBlockController.materialPropertyBlock.SetColor(Shader.PropertyToID("_Color"), color.ColorWithAlpha(1f));
                            materialPropertyBlockController.ApplyChanges();
                        }
                    }
                } catch (Exception e) {
                    ChromaLogger.Log(e);
                    StopAllCoroutines();
                }
            }
        }

    }

}
