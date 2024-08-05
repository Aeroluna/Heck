using HarmonyLib;
using Heck;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes;

[HeckPatch(PatchType.Features)]
internal static class CutoutEffectPatches
{
    // Do not run SetCutout if the new value is the same as old.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CutoutEffect), nameof(CutoutEffect.SetCutout), typeof(float), typeof(Vector3))]
    private static bool CheckDifference(CutoutEffect __instance, float cutout, float ____cutout)
    {
        return !Mathf.Approximately(cutout, ____cutout);
    }

    // A new notecontroller can have its dissolve updated before this runs, causing this to override the cutouteffect
    // Thus setting the cutout effect while the _prevArrowTransparency has a different value.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CutoutAnimateEffect), nameof(CutoutAnimateEffect.Start))]
    private static bool SkipStart()
    {
        return false;
    }
}
