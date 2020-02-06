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
using Chroma.Settings;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(NoteController))]
    [HarmonyPatch("Init")]
    class NoteControllerInit {

        static void Prefix(NoteController __instance, NoteData noteData) {
            // They said it couldn't be done, they called me a madman
            if (noteData.noteType == NoteType.Bomb) {
                Color? c = null;

                // Technicolour
                if (ColourManager.TechnicolourBombs && ((int)ChromaConfig.TechnicolourBombsStyle == 2)) {
                    c = ColourManager.GetTechnicolour(true, Time.time + __instance.GetInstanceID(), ColourManager.TechnicolourStyle.PURE_RANDOM);
                }

                // NoteScales
                if (ChromaBombColourEvent.CustomBombColours.Count > 0) {
                    foreach (KeyValuePair<float, Color> d in ChromaBombColourEvent.CustomBombColours) {
                        if (d.Key <= noteData.time) c = d.Value;
                    }
                }

                // CustomJSONData _customData individual scale override
                try {
                    if (noteData is CustomNoteData customData && ChromaUtils.CheckLightingEventRequirement()) {
                        dynamic dynData = customData.customData;
                        if (dynData != null) {
                            float? r = (float?)Trees.at(dynData, "_bombR");
                            float? g = (float?)Trees.at(dynData, "_bombG");
                            float? b = (float?)Trees.at(dynData, "_bombB");
                            if (r != null && g != null && b != null) {
                                c = new Color(r.Value, g.Value, b.Value);
                                //ChromaLogger.Log("Single bomb colour changed to " + c.ToString());
                            }
                        }
                    }
                }
                catch (Exception e) {
                    ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                    ChromaLogger.Log(e);
                }

                if (c != null) {
                    Material mat = __instance.noteTransform.gameObject.GetComponent<Renderer>().material;
                    mat.SetColor("_SimpleColor", (Color)c);
                }
            }
        }

    }

}
