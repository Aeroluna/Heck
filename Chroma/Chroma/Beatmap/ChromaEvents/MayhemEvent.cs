using Chroma.Utils;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using IPA.Utilities;

namespace Chroma.Beatmap.ChromaEvents {

    /*class MayhemEvent : ChromaJSONEventData {

        public override void ParseNode(string key, JObject node) {
            
        }

        public override void Activate(BeatmapEventData baseData, LightSwitchEventEffect lse, BeatmapEventType type) {
            LightWithId[] lights = lse.GetField<List<LightWithId>[]>("_lights")[lse.LightsID];
            for (int i = 0; i < lights.Length; i++) lights[i].color = UnityEngine.Random.ColorHSV().ColorWithValue(1f); //ColourManager.GetTechnicolour(true, time + lights[i].GetInstanceID(), ColourManager.TechnicolourStyle.PURE_RANDOM);
        }

        public override BeatmapEventData CreateBaseGameBeatmapEvent() {
            return CreateDoNothingEvent(time, type);
        }

        public static void ActivateTechnicolour(BeatmapEventData baseData, LightSwitchEventEffect lse, BeatmapEventType type) {
            LightWithId[] lights = lse.GetField<List<LightWithId>[]>("_lights")[lse.LightsID];
            for (int i = 0; i < lights.Length; i++) lights[i].color = ColourManager.GetTechnicolour(baseData.value > 3, baseData.time + lights[i].GetInstanceID(), ColourManager.TechnicolourStyle.PURE_RANDOM);
        }

    }*/

    class MayhemEvent {

        public static void ActivateTechnicolour(BeatmapEventData baseData, LightSwitchEventEffect lse, BeatmapEventType type) {
            LightWithId[] lights = lse.GetPrivateField<List<LightWithId>[]>("_lights")[lse.LightsID].ToArray();
            for (int i = 0; i < lights.Length; i++) lights[i].ColorWasSet(ColourManager.GetTechnicolour(baseData.value > 3, baseData.time + lights[i].GetInstanceID(), ColourManager.TechnicolourStyle.PURE_RANDOM));
        }

    }

}
