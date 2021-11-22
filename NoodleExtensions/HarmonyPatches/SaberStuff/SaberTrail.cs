namespace NoodleExtensions.HarmonyPatches
{
    using Heck;
    using UnityEngine;

    [HeckPatch(typeof(SaberTrail))]
    [HeckPatch("Init")]
    internal static class SaberTrailAwake
    {
        private static void Postfix(SaberTrail __instance, TrailRenderer ____trailRenderer)
        {
            // Parent to VRGameCore
            ____trailRenderer.transform.SetParent(__instance.transform.parent.parent.parent, false);
        }
    }
}
