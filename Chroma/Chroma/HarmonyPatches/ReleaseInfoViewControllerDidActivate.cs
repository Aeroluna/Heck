using Chroma.Utils;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ReleaseInfoViewController))]
    [HarmonyPatch("DidActivate")]
    class ReleaseInfoViewControllerDidActivate {

        static void Postfix(ReleaseInfoViewController __instance, ref MainSettingsModel ____mainSettingsModel, ref bool firstActivation, ref TextPageScrollView ____textPageScrollView, ref TextAsset ____releaseNotesTextAsset, ref TextAsset ____firstTextAsset) {
            if (firstActivation) {
                SidePanelUtil.ReleaseInfoEnabled(__instance, ____textPageScrollView, ____mainSettingsModel.playingForTheFirstTime ? ____firstTextAsset.text : ____releaseNotesTextAsset.text);
            }
        }

    }

}
