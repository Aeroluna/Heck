using System.Collections.Generic;
using Chroma.Animation;
using Chroma.Colorizer;
using Heck;
using Heck.Animation;
using IPA.Utilities;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace Chroma.HarmonyPatches.Colorizer
{
    internal class ObjectColorize : IAffinity
    {
        private readonly ObstacleColorizerManager _obstacleManager;
        private readonly SliderColorizerManager _sliderManager;
        private readonly DeserializedData _deserializedData;

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
        [AffinityPatch(typeof(SliderController), nameof(SliderController.Init))]
        private void SliderColorize(SliderController __instance, SliderData sliderData)
        {
            if (_deserializedData.Resolve(sliderData, out ChromaObjectData? chromaData))
            {
                _sliderManager.Colorize(__instance, chromaData.Color);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(ObstacleController), nameof(ObstacleController.GetPosForTime))]
        private void ObstacleUpdateColorize(
            ObstacleController __instance,
            ObstacleData ____obstacleData,
            float ____startTimeOffset,
            float ____move1Duration,
            float ____move2Duration,
            float ____obstacleDuration,
            float time)
        {
            if (!_deserializedData.Resolve(____obstacleData, out ChromaObjectData? chromaData))
            {
                return;
            }

            List<Track>? tracks = chromaData.Track;
            PointDefinition<Vector4>? pathPointDefinition = chromaData.LocalPathColor;
            if (tracks == null && pathPointDefinition == null)
            {
                return;
            }

            float normalTime = (time - ____move1Duration) / (____move2Duration + ____obstacleDuration);

            AnimationHelper.GetColorOffset(pathPointDefinition, tracks, normalTime, out Color? colorOffset);

            if (colorOffset.HasValue)
            {
                _obstacleManager.Colorize(__instance, colorOffset.Value);
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

            List<Track>? tracks = chromaData.Track;
            PointDefinition<Vector4>? pathPointDefinition = chromaData.LocalPathColor;
            if (tracks == null && pathPointDefinition == null)
            {
                return;
            }

            float jumpDuration = ____sliderMovement.jumpDuration;
            float duration = (jumpDuration * 0.75f) + (____sliderData.tailTime - ____sliderData.time);
            float normalTime = ____sliderMovement.timeSinceHeadNoteJump / (jumpDuration + duration);

            AnimationHelper.GetColorOffset(pathPointDefinition, tracks, normalTime, out Color? colorOffset);

            if (colorOffset.HasValue)
            {
                _sliderManager.Colorize(__instance, colorOffset.Value);
            }
        }
    }
}
