using Chroma.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Chroma.ColourManager;

namespace Chroma.Beatmap.Z_Testing.ChromaEvents {
    
    public static class LightsIsolatedTechnicolour {

        public static void Activate(LightSwitchEventEffect lse, BeatmapEventType type, TechnicolourStyle style, bool warm, float time) {
            BloomPrePassLight[] lights = lse.GetField<BloomPrePassLight[]>("_lights");
            for (int i = 0; i < lights.Length; i++) lights[i].color = ColourManager.GetTechnicolour(warm, time + lights[i].GetInstanceID(), style); //UnityEngine.Random.ColorHSV().ColorWithValue(1f);
        }

    }

}
