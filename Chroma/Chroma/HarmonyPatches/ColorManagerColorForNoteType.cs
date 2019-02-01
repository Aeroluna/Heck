using Chroma.Settings;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("ColorForNoteType")]
    class ColorManagerColorForNoteType {

        public static bool Prefix(ref Color __result, ref NoteType type) {
            Color c = ColourManager.GetNoteTypeColourOverride(type);
            if (c != Color.clear) {
                __result = c;
                return false;
            }
            return true;
        }

    }

}


/*

[HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("ColorForNoteType")]
    class ColorManagerColorForNoteType {

        public static bool Prefix(ref Color __result, ref NoteType type) {
            //if (ColourManager.TechnicolourBlocks) {
            //    __result = ColourManager.GetTechnicolour(type == NoteType.NoteA, Time.time, ChromaConfig.TechnicolourBlocksStyle);
            //    return false;
            //}
            try {
                Color c = ColourManager.GetNoteTypeColourOverride(type);
                if (c != Color.clear) {
                    ChromaLogger.Log("Got colour " + c);
                    __result = c;
                    return false;
                }
            } catch (Exception e) {
                ChromaLogger.Log(e);
            }
            return true;
        }

    }

}*/
