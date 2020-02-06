using Chroma.Settings;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("ColorForNoteType")]
    class ColorManagerColorForNoteType {

        public static bool Prefix(ref Color __result, ref NoteType type) {
            Color? c = ColourManager.GetNoteTypeColourOverride(type);
            if (c != null) {
                __result = (Color)c;
                return false;
            }
            return true;
        }

    }

}
