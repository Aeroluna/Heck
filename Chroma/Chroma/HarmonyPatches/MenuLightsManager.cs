using Chroma.Settings;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.HarmonyPatches {
    //TODO: Find a way to not make this run a hundred times
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(MenuLightsManager))]
    [HarmonyPatch("SetColorsFromPreset")]
    class MenuLightsManagerSetColorsFromPreset {

        public static void Postfix() {
            ColourManager.RefreshLights();
        }

    }

}