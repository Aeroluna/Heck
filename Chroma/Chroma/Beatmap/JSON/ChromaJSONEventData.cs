using Chroma.Beatmap.ChromaEvents;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma.Beatmap.JSON {

    public abstract class ChromaJSONEventData : ChromaJSONBeatmapObject {

        public const int GLOBAL_DO_NOTHING_VALUE = 1800000000;

        public static Dictionary<BeatmapEventData, ChromaJSONEventData> chromaEvents = new Dictionary<BeatmapEventData, ChromaJSONEventData>();

        public static void RegisterListeners() {
            ChromaPlugin.MainMenuLoadedEvent += ResetEvents;
        }

        private static void ResetEvents() {
            chromaEvents.Clear();
        }

        public static ChromaJSONEventData GetChromaEvent(BeatmapEventData data) {
            if (chromaEvents.TryGetValue(data, out ChromaJSONEventData chromaData)) return chromaData;
            return null;
        }




        public static void ParseJSONNoteData(JSONNode mainNode, ref List<ChromaJSONEventData> dataList, ref float beatsPerMinute, ref float shuffle, ref float shufflePeriod) {

            foreach (JSONNode node in mainNode) {

                try {

                    switch (node["_cType"].Value) {
                        case "mayhem":
                            dataList.Add(new MayhemEvent().ParseJSON<MayhemEvent>(node, ref beatsPerMinute, ref shuffle, ref shufflePeriod));
                            break;
                        default:
                            ChromaLogger.Log("Invalid _chromaEvent type " + node["_cType"].Value, ChromaLogger.Level.WARNING);
                            break;
                    }

                } catch (Exception e) {
                    ChromaLogger.Log(e);
                }

            }

            /*if (chromaEvent != null) {
                BeatmapEventData baseData = chromaEvent.CreateBaseGameBeatmapEvent();
                LinkEvents(baseData, chromaEvent);
                dataList.Add(baseData);
                ChromaLogger.Log("Added event " + baseData.ToString());
            }*/
            
        }

        public abstract void Activate(BeatmapEventData baseData, LightSwitchEventEffect lse, BeatmapEventType type);

        public abstract BeatmapEventData CreateBaseGameBeatmapEvent();

        protected BeatmapEventData CreateDoNothingEvent(float time, BeatmapEventType type) {
            return new BeatmapEventData(time, type, GLOBAL_DO_NOTHING_VALUE);
        }

        internal BeatmapEventData RegisterLink() {
            BeatmapEventData baseData = CreateBaseGameBeatmapEvent();
            chromaEvents.Add(baseData, this);
            return baseData;
        }

    }

}
