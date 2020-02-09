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
            if (ColourManager.TechnicolourBlocks && (ChromaConfig.TechnicolourBlocksStyle == ColourManager.TechnicolourStyle.GRADIENT))
                VFX.TechnicolourController.Instance._colorNoteVisuals.Add(__instance);
        }

    }

}
