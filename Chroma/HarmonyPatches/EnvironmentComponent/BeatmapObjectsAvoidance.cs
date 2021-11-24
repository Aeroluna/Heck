using Chroma.Lighting.EnvironmentEnhancement;
using Heck;
using JetBrains.Annotations;
using UnityEngine;

namespace Chroma.HarmonyPatches.EnvironmentComponent
{
    [HeckPatch(typeof(BeatmapObjectsAvoidance))]
    [HeckPatch("Update")]
    internal static class BeatmapObjectsAvoidanceUpdate
    {
        [UsedImplicitly]
        private static void Postfix(BeatmapObjectsAvoidance __instance)
        {
            if (EnvironmentEnhancementManager.AvoidancePosition.TryGetValue(__instance, out Vector3 position))
            {
                __instance.transform.localPosition = position;
            }

            if (EnvironmentEnhancementManager.AvoidanceRotation.TryGetValue(__instance, out Quaternion rotation))
            {
                __instance.transform.localRotation = rotation;
            }
        }
    }
}
