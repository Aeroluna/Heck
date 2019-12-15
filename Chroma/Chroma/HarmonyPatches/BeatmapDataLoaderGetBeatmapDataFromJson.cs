using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chroma.Utils;
using Harmony;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Chroma.HarmonyPatches {
    
    /*[HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(BeatmapDataLoader))]
    [HarmonyPatch("GetBeatmapDataFromJson")]
    class BeatmapDataLoaderGetBeatmapDataFromJson {
        
        public static void Postfix(ref BeatmapData __result, ref string json, ref float beatsPerMinute, ref float shuffle, ref float shufflePeriod) {


            try {
                //TODO unbreak this
                JObject node = JObject.Parse(json);
                JObject eventsNode = node["_chromaEvents"].Value<JObject>();
                ChromaJSONBeatmap chromaMap = new ChromaJSONBeatmap(__result);
                if (eventsNode != null) {
                    ChromaJSONEventData.ParseJSONNoteData(eventsNode, ref chromaMap.chromaEvents, ref beatsPerMinute, ref shuffle, ref shufflePeriod);
                }
                chromaMap.Register();
            } catch (Exception e) {
                ChromaLogger.Log(e);
            }

            

        }

    }*/
    
}