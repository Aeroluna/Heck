using Chroma.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Beatmap.ChromaEvents {
    public static class LightsSmoothTransition {

        //Fade from COLOURFROM to COLOURTO over DURATION seconds, at a rate of 1/FREQ per second.
        //If COLOURFROM is black, that causes a fade-in effect.
        //Test command fades in at a random value between 0-3 seconds, with 0.1 (10x a second) freq
        public static void Activate(LightSwitchEventEffect lse, BeatmapEventType type, Color colourFrom, Color colourTo, MonoBehaviour lightObject, float duration, float freq) {
            lightObject.StartCoroutine(Routine(lse, type, colourFrom, colourTo, duration, freq));
        }

        internal static IEnumerator Routine(LightSwitchEventEffect lse, BeatmapEventType type, Color colourFrom, Color colourTo, float duration, float freq) {
            ChromaTesting.isTransitioning = true;
            BloomPrePassLight[] lights = lse.GetField<BloomPrePassLight[]>("_lights");
            
            float time = 0;
            while (time < duration) {
                for (int i = 0; i < lights.Length; i++) lights[i].color = Color.Lerp(colourFrom, colourTo, time / duration);
                yield return new WaitForSeconds(freq);
                time += freq;
            }
            for (int i = 0; i < lights.Length; i++) lights[i].color = colourTo;
            ChromaTesting.isTransitioning = false;
        }
        
    }
}
