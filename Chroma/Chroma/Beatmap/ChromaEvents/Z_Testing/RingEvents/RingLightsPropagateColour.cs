using Chroma.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Beatmap.ChromaEvents.Z_Testing.RingEvents {

    [Obsolete("Science purposes only", true)]
    public static class RingLightsPropagateColour {

        public static void Activate(LightSwitchEventEffect lse, BeatmapEventType type, Color colour, MonoBehaviour lightObject, float delay) {
            lightObject.StartCoroutine(Routine(lse, type, colour, delay));
        }

        internal static IEnumerator Routine(LightSwitchEventEffect lse, BeatmapEventType type, Color colour, float delay) {
            LightWithId[] lights = lse.GetField<LightWithIdManager>("_lightManager").GetField<List<LightWithId>[]>("_lights")[lse.LightsID].ToArray();
            Dictionary<int, List<LightWithId>> lightWavesByPosition = new Dictionary<int, List<LightWithId>>();

            for (int i = 0; i < lights.Length; i++) {
                List<LightWithId> wave;
                if (!lightWavesByPosition.TryGetValue(Mathf.FloorToInt(lights[i].transform.position.z), out wave)) {
                    wave = new List<LightWithId>();
                    lightWavesByPosition.Add(Mathf.FloorToInt(lights[i].transform.position.z), wave);
                }
                wave.Add(lights[i]);
            }
            
            ChromaLogger.Log("Found " + lightWavesByPosition.Count + " waves!");

            List<List<LightWithId>> lightWaves = new List<List<LightWithId>>();
            foreach (KeyValuePair<int, List<LightWithId>> kv in lightWavesByPosition) {
                lightWaves.Add(kv.Value);
            }

            for (int i = 0; i < lightWaves.Count; i++) {
                for (int j = 0; j < lightWaves[i].Count; j++) lightWaves[i][j].ColorWasSet(colour);
                yield return new WaitForSeconds(delay);
            }
        }

    }

}
