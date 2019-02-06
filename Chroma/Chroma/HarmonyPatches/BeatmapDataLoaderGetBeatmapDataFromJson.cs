using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chroma.Beatmap.JSON;
using Chroma.Utils;
using Harmony;
using SimpleJSON;
using UnityEngine;

namespace Chroma.HarmonyPatches {
    /*
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(BeatmapDataLoader))]
    [HarmonyPatch("GetBeatmapDataFromJson")]
    class BeatmapDataLoaderGetBeatmapDataFromJson {
        
        public static void Postfix(ref BeatmapData __result, ref string json, ref float beatsPerMinute, ref float shuffle, ref float shufflePeriod) {


            try {
                JSONNode node = JSONNode.Parse(json);
                JSONNode eventsNode = node["_chromaEvents"];
                if (eventsNode != null) {
                    BeatmapEventData[] eventData = ChromaJSONEventData.ParseJSONNoteData(eventsNode, __result.beatmapEventData);
                    __result.SetProperty("beatmapEventData", eventData);
                }
                JSONNode notesNode = node["_chromaEvents"];
                if (notesNode != null) {
                    BeatmapLineData[] linesData = ChromaJSONNoteData.ParseJSONNoteData(notesNode, __result.beatmapLinesData);
                    __result.SetProperty("beatmapLinesData", linesData);
                }
            } catch (Exception e) {
                ChromaLogger.Log(e);
            }

            

        }

    }
    */
}