namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using Heck;
    using UnityEngine;

    // Do not run SetCutout if the new value is the same as old.
    [HeckPatch(typeof(CutoutEffect))]
    [HeckPatch("SetCutout")]
    [HeckPatch(new Type[] { typeof(float), typeof(Vector3) })]
    internal static class CutoutEffectSetCutout
    {
        private static bool Prefix(float cutout, float ____cutout)
        {
            return !(cutout == ____cutout);
        }
    }
}
