using System.Collections.Generic;
using Chroma.Colorizer;
using Heck;
using Heck.Animation;
using JetBrains.Annotations;
using UnityEngine;
using static Chroma.ChromaCustomDataManager;

namespace Chroma.HarmonyPatches.Colorizer
{
    [HeckPatch(typeof(ObstacleController))]
    [HeckPatch("Init")]
    internal static class ObstacleControllerInit
    {
        [UsedImplicitly]
        private static void Postfix(ObstacleController __instance, ObstacleData obstacleData)
        {
            if (__instance is MultiplayerConnectedPlayerObstacleController)
            {
                return;
            }

            ChromaObjectData? chromaData = TryGetObjectData<ChromaObjectData>(obstacleData);
            if (chromaData == null)
            {
                return;
            }

            __instance.ColorizeObstacle(chromaData.Color);
        }
    }

    [HeckPatch(typeof(ObstacleController))]
    [HeckPatch("Update")]
    internal static class ObstacleControllerUpdate
    {
        [UsedImplicitly]
        private static void Postfix(ObstacleController __instance, ObstacleData ____obstacleData, AudioTimeSyncController ____audioTimeSyncController, float ____startTimeOffset, float ____move1Duration, float ____move2Duration, float ____obstacleDuration)
        {
            ChromaObjectData? chromaData = TryGetObjectData<ChromaObjectData>(____obstacleData);
            if (chromaData == null)
            {
                return;
            }

            List<Track>? tracks = chromaData.Track;
            PointDefinition? pathPointDefinition = chromaData.LocalPathColor;
            if (tracks == null && pathPointDefinition == null)
            {
                return;
            }

            float elapsedTime = ____audioTimeSyncController.songTime - ____startTimeOffset;
            float normalTime = (elapsedTime - ____move1Duration) / (____move2Duration + ____obstacleDuration);

            AnimationHelper.GetColorOffset(pathPointDefinition, tracks, normalTime, out Color? colorOffset);

            if (colorOffset.HasValue)
            {
                __instance.ColorizeObstacle(colorOffset.Value);
            }
        }
    }
}
