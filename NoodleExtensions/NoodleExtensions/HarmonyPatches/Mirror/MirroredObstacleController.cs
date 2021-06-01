namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;
    using UnityEngine;

    [HeckPatch(typeof(MirroredObstacleController))]
    [HeckPatch("UpdatePositionAndRotation")]
    internal static class MirroredObstacleControllerUpdatePositionAndRotation
    {
        // Must be overwritten to compensate for rotation
        private static void Postfix(MirroredObstacleController __instance, ObstacleController ____followedObstacle, Transform ____transform, Transform ____followedTransform)
        {
            Vector3 position = ____followedTransform.position;
            Quaternion quaternion = ____followedTransform.rotation;
            position.y = -position.y;
            quaternion = quaternion.Reflect(Vector3.up);
            ____transform.SetPositionAndRotation(position, quaternion);

            if (____transform.localScale != ____followedTransform.localScale)
            {
                ____transform.localScale = ____followedTransform.localScale;
            }

            if (CutoutManager.ObstacleCutoutEffects.TryGetValue(__instance, out CutoutAnimateEffectWrapper cutoutAnimateEffect))
            {
                if (CutoutManager.ObstacleCutoutEffects.TryGetValue(____followedObstacle, out CutoutAnimateEffectWrapper followedCutoutAnimateEffect))
                {
                    cutoutAnimateEffect.SetCutout(followedCutoutAnimateEffect.Cutout);
                }
            }
        }
    }
}
