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

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("ColorForSaberType")]
    class ColorManagerColorForSaberType {

        public static bool Prefix(ref Saber.SaberType type, ref Color __result) {

            if (ColourManager.TechnicolourSabers) {
                __result = ColourManager.GetTechnicolour(type == Saber.SaberType.SaberA, Time.time, ChromaConfig.TechnicolourSabersStyle);
                return false;
            }

            if (type == Saber.SaberType.SaberA) {
                if (ColourManager.A != null) {
                    __result = (Color)ColourManager.A;
                    return false;
                }
            }
            else {
                if (ColourManager.B != null) {
                    __result = (Color)ColourManager.B;
                    return false;
                }
            }

            return true;
        }

    }

}
