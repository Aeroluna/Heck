using Chroma.Settings;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using IPA.Utilities;
using Chroma.Utils;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Init")]
    class ObstacleControllerInit {

        public static void Prefix(ref SimpleColorSO ____color) {
            if (ColourManager.TechnicolourBarriers && ((int)ChromaConfig.TechnicolourWallsStyle == 2)) {
                ColourManager.BarrierColour = ColourManager.GetTechnicolour(true, Time.time, ColourManager.TechnicolourStyle.PURE_RANDOM);
            }

            if (ColourManager.BarrierColour != Color.clear) ____color.SetColor(ColourManager.BarrierColour);
        }

    }

}