using Chroma.Lighting.EnvironmentEnhancement;
using SiraUtil.Affinity;
using UnityEngine;

namespace Chroma.HarmonyPatches.EnvironmentComponent
{
    internal class BeatmapObjectsAvoidanceTransformOverride : IAffinity
    {
        private readonly EnvironmentEnhancementManager _environmentManager;

        private BeatmapObjectsAvoidanceTransformOverride(EnvironmentEnhancementManager environmentManager)
        {
            _environmentManager = environmentManager;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectsAvoidance), nameof(BeatmapObjectsAvoidance.Update))]
        private void Postfix(BeatmapObjectsAvoidance __instance)
        {
            if (_environmentManager.AvoidancePosition.TryGetValue(__instance, out Vector3 position))
            {
                __instance.transform.localPosition = position;
            }

            if (_environmentManager.AvoidanceRotation.TryGetValue(__instance, out Quaternion rotation))
            {
                __instance.transform.localRotation = rotation;
            }
        }
    }
}
