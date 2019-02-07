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
    
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(BeatmapDataLoader))]
    [HarmonyPatch("GetBeatmapDataFromJson")]
    class BeatmapDataLoaderGetBeatmapDataFromJson {
        
        public static void Postfix(ref BeatmapData __result, ref string json, ref float beatsPerMinute, ref float shuffle, ref float shufflePeriod) {


            try {
                JSONNode node = JSONNode.Parse(json);
                JSONNode eventsNode = node["_chromaEvents"];
                ChromaJSONBeatmap chromaMap = new ChromaJSONBeatmap(__result);
                if (eventsNode != null) {
                    ChromaJSONEventData.ParseJSONNoteData(eventsNode, ref chromaMap.chromaEvents, ref beatsPerMinute, ref shuffle, ref shufflePeriod);
                }
                /*JSONNode notesNode = node["_chromaEvents"];
                if (notesNode != null) {
                    BeatmapLineData[] linesData = ChromaJSONNoteData.ParseJSONNoteData(notesNode, __result.beatmapLinesData);
                    __result.SetProperty("beatmapLinesData", linesData);
                }*/
                chromaMap.Register();
            } catch (Exception e) {
                ChromaLogger.Log(e);
            }

            

        }

    }
    
}