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
    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("Awake")]
    class ColorNoteVisualsAwake {

        static void Postfix(ColorNoteVisuals __instance) {
            VFX.VFXRainbowNotes._colorNoteVisuals.Add(__instance);
        }

    }

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("OnDestroy")]
    class ColorNoteVisualsOnDestroy {

        static void Postfix(ColorNoteVisuals __instance) {
            VFX.VFXRainbowNotes._colorNoteVisuals.Remove(__instance);
        }

    }

}
