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

        private static IEnumerable<Material> coreObstacleMaterials;
        private static IEnumerable<ParametricBoxFrameController> wallFrame;
        //private static IEnumerable<Material> frameObstacleMaterials;

        private static Color coreColor;
        private static Color coreAddColor;
        private static Color frameColor;

        private float bpm;

        private static void GetWallColours() {
            coreObstacleMaterials = Resources.FindObjectsOfTypeAll<Material>().Where(m => m.name == "ObstacleCore" || m.name == "ObstacleCoreInside");
            foreach (Material m in coreObstacleMaterials) {
                coreColor = m.color;
                coreAddColor = m.GetColor("_AddColor");
            }
            /*frameObstacleMaterials = Resources.FindObjectsOfTypeAll<Material>().Where(m => m.name == "ObstacleFrame");
            foreach (Material m in frameObstacleMaterials) {
                frameColor = m.color;
            }*/
            wallFrame = Resources.FindObjectsOfTypeAll<ParametricBoxFrameController>().ToList();
            foreach (ParametricBoxFrameController frame in wallFrame) {
                frameColor = frame.color;
            }
        }

        /// <summary>
        /// Sets global wall colours
        /// </summary>
        /// <param name="color">The colour desired.</param>
        public static void ApplyGlobalWallColours(Color color) {
            // From CC
            if (coreObstacleMaterials != null && wallFrame != null) {
                foreach (Material m in coreObstacleMaterials) {
                    m.color = color;
                    m.SetColor("_AddColor", (color / 4f).ColorWithAlpha(0f));
                }
                /*foreach (Material m in frameObstacleMaterials) {
                    m.color = color;
                }*/
                foreach (ParametricBoxFrameController frame in wallFrame) {
                    frame.color = color;
                    frame.Refresh();
                }
            }
        }

        /// <summary>
        /// Resets global wall colour to default.
        /// </summary>
        public static void ResetGlobalWallColours() {
            if (coreObstacleMaterials != null && wallFrame != null) {
                foreach (Material m in coreObstacleMaterials) {
                    m.color = coreColor;
                    m.SetColor("_AddColor", coreAddColor);
                }
                /*foreach (Material m in frameObstacleMaterials) {
                    m.color = frameColor;
                }*/
                foreach (ParametricBoxFrameController frame in wallFrame) {
                    frame.color = frameColor;
                    frame.Refresh();
                }
            }
        }

        /*
         * RAINBOW
         */

        void Init() {
            GetWallColours();

            if (IsRainbowWalls()) {
                StartCoroutine(RainbowWalls());
                ChromaLogger.Log("Rainbow Walls!", ChromaLogger.Level.DEBUG);
            } else if (ColourManager.BarrierColour == Color.clear) {
                ResetGlobalWallColours(); //We have default colours, boys!
                ChromaLogger.Log("Default Walls", ChromaLogger.Level.DEBUG);
            }/* else {
                ApplyGlobalWallColours(ColourManager.BarrierColour);
                ChromaLogger.Log(ColourManager.BarrierColour.ToString() + " Walls", ChromaLogger.Level.DEBUG);
            }*/

        }

        Color color;
        public IEnumerator RainbowWalls() {
            float secondsPerBeat = (60f / bpm);
            while (true) {
                yield return new WaitForSeconds(rainbowWallUpdateRate);
                try {
                    //color = ColourManager.GetTechnicolour(Time.time * RainbowWallCycleSpeed, ColourManager._technicolourWalls, ColourManager.TechnicolourTransition.SMOOTH);
                    color = ColourManager.GetLerpedFromArray(ColourManager.TechnicolourCombinedPalette, Time.time / secondsPerBeat);
                    //ChromaLogger.Log("Rainbow Walls " + color);
                    ApplyGlobalWallColours(color);
                } catch (Exception e) {
                    ChromaLogger.Log(e);
                    this.StopAllCoroutines();
                }
            }
        }

        public static bool IsRainbowWalls() {
            return ColourManager.TechnicolourBarriers && ChromaConfig.TechnicolourWallsStyle == ColourManager.TechnicolourStyle.ANY_PALETTE;
        }

    }

}
