namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using Heck;
    using UnityEngine;

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
