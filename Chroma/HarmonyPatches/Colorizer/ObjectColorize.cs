using System.Collections.Generic;
using Chroma.Animation;
using Chroma.Colorizer;
using Heck.Animation;
using Heck.Deserialize;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace Chroma.HarmonyPatches.Colorizer;

internal class ObjectColorize : IAffinity
{
    private readonly DeserializedData _deserializedData;
    private readonly ObstacleColorizerManager _obstacleManager;
    private readonly SliderColorizerManager _sliderManager;

    private ObjectColorize(
        ObstacleColorizerManager obstacleManager,
        SliderColorizerManager sliderManager,
        [Inject(Id = ChromaController.ID)] DeserializedData deserializedData)
    {
        _obstacleManager = obstacleManager;
        _sliderManager = sliderManager;
        _deserializedData = deserializedData;
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.Init))]
    private void ObstacleColorize(ObstacleController __instance, ObstacleData obstacleData)
    {
        if (_deserializedData.Resolve(obstacleData, out ChromaObjectData? chromaData))
        {
            _obstacleManager.Colorize(__instance, chromaData.Color);
        }
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.GetPosForTime))]
    private void ObstacleUpdateColorize(
        ObstacleController __instance,
        ObstacleData ____obstacleData,
        float ____startTimeOffset,
#if LATEST
        IVariableMovementDataProvider ____variableMovementDataProvider,
#else
        float ____move1Duration,
        float ____move2Duration,
        float ____obstacleDuration,
#endif
        float time)
    {
        if (!_deserializedData.Resolve(____obstacleData, out ChromaObjectData? chromaData))
        {
            return;
        }

        IReadOnlyList<Track>? tracks = chromaData.Track;
        PointDefinition<Vector4>? pathPointDefinition = chromaData.LocalPathColor;
        if (tracks == null && pathPointDefinition == null)
        {
            return;
        }

#if LATEST
        float moveDuration = ____variableMovementDataProvider.moveDuration;
        float jumpDuration = ____variableMovementDataProvider.jumpDuration;
        float obstacleDuration = ____obstacleData.duration;
#else
        float moveDuration = ____move1Duration;
        float jumpDuration = ____move2Duration;
        float obstacleDuration = ____obstacleDuration;
#endif

        float normalTime = (time - moveDuration) / (jumpDuration + obstacleDuration);

        AnimationHelper.GetColorOffset(pathPointDefinition, tracks, normalTime, out Color? colorOffset);

        if (colorOffset.HasValue)
        {
            _obstacleManager.Colorize(__instance, colorOffset.Value);
        }
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(SliderController), nameof(SliderController.Init))]
    private void SliderColorize(SliderController __instance, SliderData sliderData)
    {
        if (_deserializedData.Resolve(sliderData, out ChromaObjectData? chromaData))
        {
            _sliderManager.Colorize(__instance, chromaData.Color);
        }
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(SliderController), nameof(SliderController.ManualUpdate))]
    private void SliderUpdateColorize(
        SliderController __instance,
        SliderData ____sliderData,
        SliderMovement ____sliderMovement)
    {
        if (!_deserializedData.Resolve(____sliderData, out ChromaObjectData? chromaData))
        {
            return;
        }

        IReadOnlyList<Track>? tracks = chromaData.Track;
        PointDefinition<Vector4>? pathPointDefinition = chromaData.LocalPathColor;
        if (tracks == null && pathPointDefinition == null)
        {
            return;
        }

#if LATEST
        float jumpDuration = __instance._variableMovementDataProvider.jumpDuration;
#else
        float jumpDuration = ____sliderMovement.jumpDuration;
#endif
        float duration = (jumpDuration * 0.75f) + (____sliderData.tailTime - ____sliderData.time);
        float normalTime = ____sliderMovement.timeSinceHeadNoteJump / (jumpDuration + duration);

        AnimationHelper.GetColorOffset(pathPointDefinition, tracks, normalTime, out Color? colorOffset);

        if (colorOffset.HasValue)
        {
            _sliderManager.Colorize(__instance, colorOffset.Value);
        }
    }
}
