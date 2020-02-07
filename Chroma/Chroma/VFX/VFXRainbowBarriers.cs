using Chroma.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.VFX {

    public class VFXRainbowBarriers : MonoBehaviour {

        public static void Instantiate(float bpm) {
            GameObject gameObject = new GameObject("Chroma_VFXRainbowBarriers");
            VFXRainbowBarriers vfx = gameObject.AddComponent<VFXRainbowBarriers>();
            vfx.bpm = bpm;
            vfx.Init();
        }

        private const float rainbowWallUpdateRate = 0.08f;

        private float bpm;

        /*
         * RAINBOW
         */

        void Init() {
            if (IsRainbowWalls()) {
                StartCoroutine(RainbowWalls());
                //ChromaLogger.Log("Gradient Walls!", ChromaLogger.Level.DEBUG);
            }
        }

        Color color;
        public IEnumerator RainbowWalls() {
            float secondsPerBeat = (60f / bpm);
            while (true) {
                yield return new WaitForSeconds(rainbowWallUpdateRate);
                try {
                    color = ColourManager.GetLerpedFromArray(ColourManager.TechnicolourCombinedPalette, Time.time / secondsPerBeat);
                    ColourManager.BarrierColour = color;
                } catch (Exception e) {
                    ChromaLogger.Log(e);
                    this.StopAllCoroutines();
                }
            }
        }

        public static bool IsRainbowWalls() {
            return (ChromaConfig.TechnicolourWallsStyle == ColourManager.TechnicolourStyle.GRADIENT) && ColourManager.TechnicolourBarriers;
        }

    }

}
