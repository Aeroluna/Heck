using Chroma.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Beatmap.ChromaEvents.RingEvents {

    public static class RingLightsPropagateColour {

        public static void Activate(LightSwitchEventEffect lse, BeatmapEventType type, Color colour, MonoBehaviour lightObject, float delay) {
            lightObject.StartCoroutine(Routine(lse, type, colour, delay));
        }

        internal static IEnumerator Routine(LightSwitchEventEffect lse, BeatmapEventType type, Color colour, float delay) {
            BloomPrePassLight[] lights = lse.GetField<BloomPrePassLight[]>("_lights");
            Dictionary<int, List<BloomPrePassLight>> lightWavesByPosition = new Dictionary<int, List<BloomPrePassLight>>();

            for (int i = 0; i < lights.Length; i++) {
                List<BloomPrePassLight> wave;
                if (!lightWavesByPosition.TryGetValue(Mathf.FloorToInt(lights[i].transform.position.z), out wave)) {
                    wave = new List<BloomPrePassLight>();
                    lightWavesByPosition.Add(Mathf.FloorToInt(lights[i].transform.position.z), wave);
                }
                wave.Add(lights[i]);
            }
            
            ChromaLogger.Log("Found " + lightWavesByPosition.Count + " waves!");

            List<List<BloomPrePassLight>> lightWaves = new List<List<BloomPrePassLight>>();
            foreach (KeyValuePair<int, List<BloomPrePassLight>> kv in lightWavesByPosition) {
                lightWaves.Add(kv.Value);
            }

            for (int i = 0; i < lightWaves.Count; i++) {
                for (int j = 0; j < lightWaves[i].Count; j++) lightWaves[i][j].color = colour;
                yield return new WaitForSeconds(delay);
            }
        }

    }

}
