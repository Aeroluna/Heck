namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using UnityEngine;

    [NoodlePatch(typeof(CutoutEffect))]
    [NoodlePatch("SetCutout")]
    [NoodlePatch(new Type[] { typeof(float), typeof(Vector3) })]
    internal static class CutoutEffectSetCutout
    {
        private static bool Prefix(float cutout, float ____cutout)
        {
            return !(cutout == ____cutout);
        }
    }
}
