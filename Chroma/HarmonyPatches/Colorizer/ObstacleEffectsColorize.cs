#if LATEST
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Chroma.Colorizer;
using IPA.Utilities;
using SiraUtil.Affinity;
using UnityEngine;

namespace Chroma.HarmonyPatches.Colorizer;

internal class ObstacleEffectsColorize : IAffinity
{
    private static readonly FieldAccessor<ObstacleSaberSparkleEffectManager, Action<SaberType>>.Accessor
        _sparkleEffectDidEndEventAccessor =
            FieldAccessor<ObstacleSaberSparkleEffectManager, Action<SaberType>>.GetAccessor(
                nameof(ObstacleSaberSparkleEffectManager.sparkleEffectDidEndEvent));

    private static readonly FieldAccessor<ObstacleSaberSparkleEffectManager, Action<SaberType>>.Accessor
        _sparkleEffectDidStartEventAccessor =
            FieldAccessor<ObstacleSaberSparkleEffectManager, Action<SaberType>>.GetAccessor(
                nameof(ObstacleSaberSparkleEffectManager.sparkleEffectDidStartEvent));

    private readonly ObstacleColorizerManager _manager;

    private ObstacleEffectsColorize(ObstacleColorizerManager manager)
    {
        _manager = manager;
    }

    private static bool IntersectSaberWithObstacles(
        Saber saber,
        List<ObstacleController> obstacles,
        ref Pose hit,
        [NotNullWhen(true)] ref ObstacleController? hitObstacle)
    {
        if (!saber.isActiveAndEnabled)
        {
            return false;
        }

        foreach (ObstacleController obstacle in obstacles)
        {
            Bounds bounds = obstacle.bounds;
            Transform transform = obstacle.transform;
            Vector3 start = transform.InverseTransformPoint(saber.saberBladeBottomPos);
            Vector3 end = transform.InverseTransformPoint(saber.saberBladeTopPos);
            if (!ObstacleSaberSparkleEffectManager.IntersectBoxSurfacePose(in bounds, start, end, ref hit))
            {
                continue;
            }

            hit.position = transform.TransformPoint(hit.position);
            hit.rotation *= transform.rotation;
            hitObstacle = obstacle;
            return true;
        }

        return false;
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(ObstacleSaberSparkleEffectManager), nameof(ObstacleSaberSparkleEffectManager.Update))]
    private bool SetObstacleSparksColorReplace(
        ObstacleSaberSparkleEffectManager __instance,
        Saber[] ____sabers,
        ObstacleSaberSparkleEffect[] ____effects)
    {
        List<ObstacleController> activeObstacleControllers = __instance._beatmapObjectManager.activeObstacleControllers;
        Pose identity = Pose.identity;
        ObstacleController? hitObstacle = null;
        for (int i = 0; i < ____sabers.Length; i++)
        {
            bool emitting = ____effects[i].IsEmitting();
            if (IntersectSaberWithObstacles(____sabers[i], activeObstacleControllers, ref identity, ref hitObstacle))
            {
                ____effects[i].SetPositionAndRotation(identity.position, identity.rotation);
                __instance._hapticFeedbackManager.PlayHapticFeedback(
                    ____sabers[i].saberType.Node(),
                    __instance._rumblePreset);

                Color.RGBToHSV(_manager.GetColorizer(hitObstacle).Color, out float h, out float s, out _);
                ____effects[i].color = Color.HSVToRGB(h, s, 1);

                if (emitting)
                {
                    continue;
                }

                ____effects[i].StartEmission();
                _sparkleEffectDidStartEventAccessor(ref __instance)?.DynamicInvoke(____sabers[i].saberType);
            }
            else if (emitting)
            {
                ____effects[i].StopEmission();
                _sparkleEffectDidEndEventAccessor(ref __instance)?.DynamicInvoke(____sabers[i].saberType);
            }
        }

        return false;
    }
}
#endif
