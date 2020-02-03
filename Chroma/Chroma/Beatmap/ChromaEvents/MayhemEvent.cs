using Chroma.Utils;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using IPA.Utilities;
using UnityEngine;
using System.Linq;
using System;

namespace Chroma.Beatmap.ChromaEvents {

    class MayhemEvent {

        internal static List<LightWithId>[] manager;

        public static void ActivateTechnicolour(BeatmapEventData baseData, LightSwitchEventEffect lse, BeatmapEventType type) {
            if (manager == null) manager = GameObject.Find("LightWithIdManager").GetComponent<LightWithIdManager>().GetPrivateField<List<LightWithId>[]>("_lights");
            LightWithId[] lights = manager[lse.LightsID].ToArray();
            for (int i = 0; i < lights.Length; i++) lights[i].ColorWasSet(ColourManager.GetTechnicolour(baseData.value > 3, baseData.time + lights[i].GetInstanceID(), Settings.ChromaConfig.TechnicolourLightsStyle));
        }

    }

}
