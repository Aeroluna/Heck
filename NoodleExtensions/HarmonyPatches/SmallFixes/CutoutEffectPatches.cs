using System;
using HarmonyLib;
using Heck;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    // Do not run SetCutout if the new value is the same as old.
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(CutoutEffect))]
    internal static class CutoutEffectPatches
    {
        private static readonly BoolSO _permanentTrue = CreatePermanentTrueBoolSO();

        private static BoolSO CreatePermanentTrueBoolSO()
        {
            BoolSO so = ScriptableObject.CreateInstance<BoolSO>();
            so.value = true;
            return so;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CutoutEffect.SetCutout), typeof(float), typeof(Vector3))]
        private static bool CheckDifference(float cutout, float ____cutout)
        {
            return Math.Abs(cutout - ____cutout) > 0.01;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(CutoutEffect.Start))]
        private static void ForceRandom(ref BoolSO ____useRandomCutoutOffset)
        {
            ____useRandomCutoutOffset = _permanentTrue;
        }
    }
}
