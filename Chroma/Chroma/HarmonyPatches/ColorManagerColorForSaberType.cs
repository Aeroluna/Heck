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
    [HarmonyPatch("ColorForSaberType")]
    class ColorManagerColorForSaberType {

        public static bool Prefix(ref Saber.SaberType type, ref Color __result) {

            if (ColourManager.TechnicolourSabers) {
                __result = ColourManager.GetTechnicolour(type == Saber.SaberType.SaberA, Time.time, ChromaConfig.TechnicolourSabersStyle);
                return false;
            }

            if (type == Saber.SaberType.SaberA) {
                if (ColourManager.A != Color.clear) {
                    __result = ColourManager.A;
                    return false;
                }
            } else {
                if (ColourManager.B != Color.clear) {
                    __result = ColourManager.B;
                    return false;
                }
            }

            return true;
        }

    }

}