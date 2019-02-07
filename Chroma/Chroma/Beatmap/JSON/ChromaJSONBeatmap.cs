using Chroma.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma.Beatmap.JSON {

    class ChromaJSONBeatmap {

        public static Dictionary<BeatmapData, ChromaJSONBeatmap> chromaBeatmaps = new Dictionary<BeatmapData, ChromaJSONBeatmap>();

        public static Tuple<BeatmapData, ChromaJSONBeatmap> copiedMap = null;

        public static ChromaJSONBeatmap GetChromaBeatmap(BeatmapData data) {
            if (chromaBeatmaps.TryGetValue(data, out ChromaJSONBeatmap chromaBeatmap)) return chromaBeatmap;
            if (copiedMap != null && copiedMap.Item1 == data) return copiedMap.Item2;
            return null;
        }

        public BeatmapData beatmap;
        public List<ChromaJSONNoteData> chromaNotes = new List<ChromaJSONNoteData>();
        public List<ChromaJSONEventData> chromaEvents = new List<ChromaJSONEventData>();
        
        public bool HasData {
            get { return chromaNotes.Count > 0 || chromaEvents.Count > 0; }
        }

        public ChromaJSONBeatmap(BeatmapData beatmap) {
            this.beatmap = beatmap;
        }

        public void Register() {
            if (HasData) {
                chromaBeatmaps.Add(beatmap, this);
                ChromaLogger.Log("Registered JSONBeatmap for " + beatmap.ToString());
            }
        }

        public void Inject(BeatmapData beatmap) {

            List<BeatmapEventData> eventData = beatmap.beatmapEventData.ToList();
            foreach (ChromaJSONEventData cev in chromaEvents) {
                BeatmapEventData baseData = cev.RegisterLink();
                eventData.Add(baseData);
            }
            eventData = eventData.OrderBy(x => x.time).ToList();
            beatmap.SetProperty("beatmapEventData", eventData.ToArray());

            ChromaLogger.Log("***");
            ChromaLogger.Log("***");
            ChromaLogger.Log("Injected ChromaJSONBeatmap");
            ChromaLogger.Log("***");
            ChromaLogger.Log("***");
        }

    }

}
