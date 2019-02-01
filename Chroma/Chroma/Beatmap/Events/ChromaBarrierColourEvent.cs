using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Beatmap.Events {

    public class ChromaBarrierColourEvent : ChromaColourEvent {

        public ChromaBarrierColourEvent(BeatmapEventData data) : base(data, true, false, new Color[] { }) { }

        public override bool Activate(ref LightSwitchEventEffect light, ref BeatmapEventData data, ref BeatmapEventType eventType) {
            //ColourManager.BarrierColour = A;
            return true;
        }

        private static Dictionary<float, Color> barrierTimeToColour = new Dictionary<float, Color>();

        public override void OnEventSet(BeatmapEventData lightmapEvent) {
            barrierTimeToColour.Add(data.time, A);
            base.OnEventSet(lightmapEvent);
        }

        public static void Clear() {
            barrierTimeToColour.Clear();
        }

        public static Color GetColor(float time) {
            Color c = Color.clear;
            foreach (KeyValuePair<float, Color> keyv in barrierTimeToColour.OrderBy(i => i.Key)) {
                if (time >= keyv.Key) c = keyv.Value;
            }
            return c;
        }

    }

}
