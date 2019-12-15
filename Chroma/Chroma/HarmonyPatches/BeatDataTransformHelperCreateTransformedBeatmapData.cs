using Chroma.Settings;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.HarmonyPatches {
    
    /*[HarmonyPatch(typeof(BeatDataTransformHelper))]
    [HarmonyPatch("CreateTransformedBeatmapData")]
    class BeatDataTransformHelperCreateTransformedBeatmapData {

        public static void Postfix(ref BeatmapData __result, ref BeatmapData beatmapData) {
            ChromaJSONBeatmap cMap = ChromaJSONBeatmap.GetChromaBeatmap(beatmapData);
            if (cMap != null) ChromaJSONBeatmap.copiedMap = new Tuple<BeatmapData, ChromaJSONBeatmap>(__result, cMap);
        }

    }*/

}