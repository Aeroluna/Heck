using System.Collections.Generic;
using Heck.Animation.Transform;
using SiraUtil.Affinity;
using UnityEngine;

namespace Chroma.HarmonyPatches.EnvironmentComponent
{
    internal class BeatmapObjectsAvoidanceTransformOverride : IAffinity
    {
        private readonly Dictionary<BeatmapObjectsAvoidance, Vector3> _positions = new();
        private readonly Dictionary<BeatmapObjectsAvoidance, Quaternion> _rotations = new();

        internal void SetTransform(BeatmapObjectsAvoidance beatmapObjectsAvoidance, TransformData transformData)
        {
            if (transformData.Position.HasValue || transformData.LocalPosition.HasValue)
            {
                UpdatePosition(beatmapObjectsAvoidance);
            }

            if (transformData.Rotation.HasValue || transformData.LocalRotation.HasValue)
            {
                UpdateRotation(beatmapObjectsAvoidance);
            }
        }

        internal void UpdatePosition(BeatmapObjectsAvoidance beatmapObjectsAvoidance)
        {
            _positions[beatmapObjectsAvoidance] = beatmapObjectsAvoidance.transform.localPosition;
        }

        internal void UpdateRotation(BeatmapObjectsAvoidance beatmapObjectsAvoidance)
        {
            _rotations[beatmapObjectsAvoidance] = beatmapObjectsAvoidance.transform.localRotation;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectsAvoidance), nameof(BeatmapObjectsAvoidance.Update))]
        private void Postfix(BeatmapObjectsAvoidance __instance)
        {
            if (_positions.TryGetValue(__instance, out Vector3 position))
            {
                __instance.transform.localPosition = position;
            }

            if (_rotations.TryGetValue(__instance, out Quaternion rotation))
            {
                __instance.transform.localRotation = rotation;
            }
        }
    }
}
