using Chroma.Beatmap.ChromaEvents;
using Chroma.Beatmap.Z_Testing.ChromaEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma {
    
    [Obsolete("Science purposes only")]
    public static class ChromaTesting {

        public static LightSwitchEventEffect lse;
        public static BeatmapEventType type;

        public static Color lastColour = Color.black;

        public static void Test() {
            //if (lse != null) {
            //Propagation testing
            //Beatmap.ChromaEvents.RingEvents.RingLightsPropagateColour.Activate(lse, type, UnityEngine.Random.ColorHSV().ColorWithValue(1f), lse, UnityEngine.Random.value * 0.5f);

            //Fade testing
            //Color nextColour = UnityEngine.Random.ColorHSV().ColorWithValue(1f);
            //Beatmap.ChromaEvents.LightsSmoothTransition.Activate(lse, type, lastColour, nextColour, lse, UnityEngine.Random.value * 3f, 0.1f);
            //lastColour = nextColour;

            //Total randomization testing
            //LightsIsolatedTechnicolour.Activate(lse, type, ColourManager.TechnicolourStyle.PURE_RANDOM, true, Time.time);
            //}

            //LightSwitchEventEffect[] sprites = Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>();
            //foreach (LightSwitchEventEffect sprite in sprites)
            //{
            //    if (sprite != null)
            //    {
            //        ChromaLogger.Log(sprite.name + sprite.LightsID);
            //    }
            //}

            VFX.VFXLightning.InstanceOrDefault.TriggerLightning(false);
            VFX.VFXLightning.InstanceOrDefault.AmbientLightning = true;
        }

    }

}
