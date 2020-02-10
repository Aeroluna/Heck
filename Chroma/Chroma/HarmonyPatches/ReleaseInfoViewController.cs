using Chroma.Utils;
using Harmony;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ReleaseInfoViewController))]
    [HarmonyPatch("DidActivate")]
    internal class ReleaseInfoViewControllerDidActivate
    {
        private static void Postfix(ReleaseInfoViewController __instance, ref MainSettingsModelSO ____mainSettingsModel, ref bool firstActivation, ref TextPageScrollView ____textPageScrollView, ref TextAsset ____releaseNotesTextAsset, ref TextAsset ____firstTextAsset)
        {
            if (firstActivation)
            {
                SidePanelUtil.ReleaseInfoEnabled(__instance, ____textPageScrollView, ____mainSettingsModel.playingForTheFirstTime ? ____firstTextAsset.text : ____releaseNotesTextAsset.text);
            }
        }
    }
}