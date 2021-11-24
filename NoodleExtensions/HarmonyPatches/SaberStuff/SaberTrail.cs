using Heck;
using JetBrains.Annotations;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SaberStuff
{
    [HeckPatch(typeof(SaberTrail))]
    [HeckPatch("Init")]
    internal static class SaberTrailAwake
    {
        [UsedImplicitly]
        private static void Postfix(SaberTrail __instance, TrailRenderer ____trailRenderer)
        {
            // Parent to VRGameCore
            ____trailRenderer.transform.SetParent(__instance.transform.parent.parent.parent, false);
        }
    }
}
