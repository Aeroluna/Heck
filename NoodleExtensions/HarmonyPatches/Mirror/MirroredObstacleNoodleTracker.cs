using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.Mirror
{
    internal class MirroredObstacleNoodleTracker : IAffinity
    {
        private readonly CutoutManager _cutoutManager;

        private MirroredObstacleNoodleTracker(CutoutManager cutoutManager)
        {
            _cutoutManager = cutoutManager;
        }

        // Must be overwritten to compensate for rotation
        [AffinityPrefix]
        [AffinityPatch(typeof(MirroredObstacleController), nameof(MirroredObstacleController.UpdatePositionAndRotation))]
        private bool MirrorObstacleUpdate(
            MirroredObstacleController __instance,
            ObstacleController ____followedObstacle,
            Transform ____transform,
            Transform ____followedTransform)
        {
            // do not reflection walls above the mirror
            if (____followedTransform.position.y < 0)
            {
                // idk how to hide it without disabling the update
                ____transform.position = new Vector3(0, 100, 0);
                return false;
            }

            Vector3 position = ____followedTransform.position;
            Quaternion quaternion = ____followedTransform.rotation;
            position.y = -position.y;
            quaternion = quaternion.Reflect(Vector3.up);
            ____transform.SetPositionAndRotation(position, quaternion);

            if (____transform.localScale != ____followedTransform.localScale)
            {
                ____transform.localScale = ____followedTransform.localScale;
            }

            _cutoutManager.ObstacleCutoutEffects[__instance].SetCutout(_cutoutManager.ObstacleCutoutEffects[____followedObstacle].Cutout);

            return false;
        }
    }
}
