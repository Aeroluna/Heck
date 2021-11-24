using Heck;
using JetBrains.Annotations;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.Mirror
{
    [HeckPatch(typeof(MirroredObstacleController))]
    [HeckPatch("UpdatePositionAndRotation")]
    internal static class MirroredObstacleControllerUpdatePositionAndRotation
    {
        // Must be overwritten to compensate for rotation
        [UsedImplicitly]
        private static bool Prefix(MirroredObstacleController __instance, ObstacleController ____followedObstacle, Transform ____transform, Transform ____followedTransform)
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

            if (!CutoutManager.ObstacleCutoutEffects.TryGetValue(__instance, out CutoutAnimateEffectWrapper cutoutAnimateEffect))
            {
                return false;
            }

            if (CutoutManager.ObstacleCutoutEffects.TryGetValue(____followedObstacle, out CutoutAnimateEffectWrapper followedCutoutAnimateEffect))
            {
                cutoutAnimateEffect.SetCutout(followedCutoutAnimateEffect.Cutout);
            }

            return false;
        }
    }
}
