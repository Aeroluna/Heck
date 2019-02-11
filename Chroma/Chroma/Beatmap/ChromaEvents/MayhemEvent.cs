using Chroma.Beatmap.JSON;
using Chroma.Settings;
using Chroma.Utils;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma.Beatmap.ChromaEvents {

    class MayhemEvent : ChromaJSONEventData {

        public override void ParseNode(string key, JSONNode node) {
            
        }

        public override void Activate(BeatmapEventData baseData, LightSwitchEventEffect lse, BeatmapEventType type) {
            BloomPrePassLight[] lights = lse.GetField<BloomPrePassLight[]>("_lights");
            for (int i = 0; i < lights.Length; i++) lights[i].color = UnityEngine.Random.ColorHSV().ColorWithValue(1f); //ColourManager.GetTechnicolour(true, time + lights[i].GetInstanceID(), ColourManager.TechnicolourStyle.PURE_RANDOM);
        }

        public override BeatmapEventData CreateBaseGameBeatmapEvent() {
            return CreateDoNothingEvent(time, type);
        }

        public static void ActivateTechnicolour(BeatmapEventData baseData, LightSwitchEventEffect lse, BeatmapEventType type) {
            BloomPrePassLight[] lights = lse.GetField<BloomPrePassLight[]>("_lights");
            for (int i = 0; i < lights.Length; i++) lights[i].color = ColourManager.GetTechnicolour(baseData.value > 3, baseData.time + lights[i].GetInstanceID(), ColourManager.TechnicolourStyle.PURE_RANDOM);
        }

    }

}
