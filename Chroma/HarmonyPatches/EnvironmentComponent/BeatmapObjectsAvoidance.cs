namespace Chroma.HarmonyPatches
{
    using Heck;
    using UnityEngine;

    [HeckPatch(typeof(BeatmapObjectsAvoidance))]
    [HeckPatch("Update")]
    internal static class BeatmapObjectsAvoidanceUpdate
    {
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
