using Chroma.Utils;
using HarmonyLib;
using HMUI;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(ReleaseInfoViewController))]
    [HarmonyPatch("DidActivate")]
    internal class ReleaseInfoViewControllerDidActivate
    {
        private static void Postfix(ReleaseInfoViewController __instance, MainSettingsModelSO ____mainSettingsModel, bool firstActivation, TextPageScrollView ____textPageScrollView, TextAsset ____releaseNotesTextAsset, TextAsset ____firstTextAsset)
        {
            if (firstActivation)
            {
                SidePanelUtil.ReleaseInfoEnabled(__instance, ____textPageScrollView, ____mainSettingsModel.playingForTheFirstTime ? ____firstTextAsset.text : ____releaseNotesTextAsset.text);
            }
        }
    }
}