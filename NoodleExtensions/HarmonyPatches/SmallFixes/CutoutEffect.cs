using System;
using Heck;
using JetBrains.Annotations;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    // Do not run SetCutout if the new value is the same as old.
    [HeckPatch(typeof(CutoutEffect))]
    [HeckPatch("SetCutout")]
    [HeckPatch(new[] { typeof(float), typeof(Vector3) })]
    internal static class CutoutEffectSetCutout
    {
        [UsedImplicitly]
        private static bool Prefix(float cutout, float ____cutout)
        {
            return Math.Abs(cutout - ____cutout) > 0.01;
        }
    }
}
