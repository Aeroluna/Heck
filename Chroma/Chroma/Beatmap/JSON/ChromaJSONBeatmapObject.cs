using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma.Beatmap.JSON {

    public abstract class ChromaJSONBeatmapObject {

        public float time;
        public BeatmapEventType type;

        public T ParseJSON<T>(JObject eventNode, ref float beatsPerMinute, ref float shuffle, ref float shufflePeriod) where T : ChromaJSONBeatmapObject {
            
            IEnumerator<KeyValuePair<string, JToken>> nodeEnum = eventNode.GetEnumerator();
            while (nodeEnum.MoveNext()) {
                string key = nodeEnum.Current.Key;
                JToken value = nodeEnum.Current.Value;

                switch (key) {
                    case "_time":
                        time = GetRealTimeFromBPMTime(value.Value<float>(), ref beatsPerMinute, ref shuffle, ref shufflePeriod);
                        break;
                    case "_type":
                        type = (BeatmapEventType) value.Value<int>();
                        break;
                    default:
                        ParseNode(key, value.Value<JObject>());
                        break;
                }
            }

            return this as T;
        }

        public abstract void ParseNode(string key, JObject node);

        private static float GetRealTimeFromBPMTime(float bmpTime, ref float beatsPerMinute, ref float shuffle, ref float shufflePeriod) {
            float num = bmpTime;
            if (shufflePeriod > 0f) {
                bool flag = (int)(num * (1f / shufflePeriod)) % 2 == 1;
                if (flag) {
                    num += shuffle * shufflePeriod;
                }
            }
            if (beatsPerMinute > 0f) {
                num = num / beatsPerMinute * 60f;
            }
            return num;
        }

    }

}
