namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using Heck;
    using Heck.Animation;
    using UnityEngine;
    using static Chroma.ChromaObjectDataManager;

    [HeckPatch(typeof(ObstacleController))]
    [HeckPatch("Init")]
    internal static class ObstacleControllerInit
    {
        private static void Postfix(ObstacleController __instance, ObstacleData obstacleData)
        {
            if (!(__instance is MultiplayerConnectedPlayerObstacleController))
            {
                ChromaObjectData? chromaData = TryGetObjectData<ChromaObjectData>(obstacleData);
                if (chromaData == null)
                {
                    return;
                }

                __instance.ColorizeObstacle(chromaData.Color);
            }
        }
    }

    [HeckPatch(typeof(ObstacleController))]
    [HeckPatch("Update")]
    internal static class ObstacleControllerUpdate
    {
        private static void Postfix(ObstacleController __instance, ObstacleData ____obstacleData, AudioTimeSyncController ____audioTimeSyncController, float ____startTimeOffset, float ____move1Duration, float ____move2Duration, float ____obstacleDuration)
        {
            ChromaObjectData? chromaData = TryGetObjectData<ChromaObjectData>(____obstacleData);
            if (chromaData == null)
            {
                return;
            }

            Track? track = chromaData.Track;
            PointDefinition? pathPointDefinition = chromaData.LocalPathColor;
            if (track != null || pathPointDefinition != null)
            {
                float jumpDuration = ____move2Duration;
                float elapsedTime = ____audioTimeSyncController.songTime - ____startTimeOffset;
                float normalTime = (elapsedTime - ____move1Duration) / (jumpDuration + ____obstacleDuration);

                Chroma.AnimationHelper.GetColorOffset(pathPointDefinition, track, normalTime, out Color? colorOffset);

                if (colorOffset.HasValue)
                {
                    __instance.ColorizeObstacle(colorOffset.Value);
                }
            }
        }
    }
}
