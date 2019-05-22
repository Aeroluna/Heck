using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma.Beatmap.JSON {

    public abstract class ChromaJSONNoteData : ChromaJSONBeatmapObject {


        public static BeatmapLineData[] ParseJSONNoteData(JObject node, BeatmapLineData[] data) {

            return data;
        }

    }

}
