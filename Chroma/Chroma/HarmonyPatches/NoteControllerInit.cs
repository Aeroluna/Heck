using Chroma.Beatmap.Events;
using Chroma.Misc;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Chroma.Utils;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(NoteController))]
    [HarmonyPatch("Init")]
    class NoteControllerInit {

        static void Postfix(NoteController __instance, NoteData ____noteData) {
            float? c = null;

            // NoteScales
            if (ChromaNoteScaleEvent.NoteScales.Count > 0) {
                foreach (KeyValuePair<float, float> d in ChromaNoteScaleEvent.NoteScales) {
                    if (d.Key <= ____noteData.time) c = d.Value;
                }
            }

            // CustomJSONData _customData individual scale override
            try {
                if (____noteData is CustomNoteData customData && ChromaUtils.CheckSpecialEventRequirement()) {
                    dynamic dynData = customData.customData;
                    if (dynData != null) {
                        float? s = (float?)Trees.at(dynData, "_noteScale");
                        if (s != null) {
                            c = s;
                            //ChromaLogger.Log("Single note scale changed to " + c.ToString());
                        }
                    }
                }
            }
            catch (Exception e) {
                ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            if (c == null) return;
            __instance.noteTransform.localScale = Vector3.one * (float)c;
        }

    }

}
