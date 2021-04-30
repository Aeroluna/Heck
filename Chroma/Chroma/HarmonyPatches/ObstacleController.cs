namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;
    using Heck.Animation;
    using UnityEngine;
    using Heck;
    using static ChromaObjectDataManager;
    using static Plugin;

    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Init")]
    internal static class ObstacleControllerInitColorizer
    {
        [HarmonyPriority(Priority.High)]
        private static void Prefix(ObstacleController __instance, ColorManager ____colorManager)
        {
            if (!(__instance is MultiplayerConnectedPlayerObstacleController))
            {
                ObstacleColorizer.OCStart(__instance, ____colorManager.obstaclesColor);
            }
        }

        private static void Postfix(ObstacleController __instance)
        {
            if (!(__instance is MultiplayerConnectedPlayerObstacleController))
            {
                __instance.SetActiveColors();
            }
        }
    }

    [HeckPatch(typeof(ObstacleController))]
    [HeckPatch("Init")]
    internal static class ObstacleControllerInit
    {
        private static void Prefix(ObstacleController __instance, ObstacleData obstacleData)
        {
            if (!(__instance is MultiplayerConnectedPlayerObstacleController))
            {
                ChromaObjectData chromaData = TryGetObjectData<ChromaObjectData>(obstacleData);
                if (chromaData == null)
                {
                    return;
                }

                Color? color = chromaData.Color;

                if (color.HasValue)
                {
                    __instance.SetObstacleColor(color.Value);
                }
                else
                {
                    __instance.Reset();
                }
            }
        }
    }

    [HeckPatch(typeof(ObstacleController))]
    [HeckPatch("Update")]
    internal static class ObstacleControllerUpdate
    {
        private static void Postfix(ObstacleController __instance, ObstacleData ____obstacleData, AudioTimeSyncController ____audioTimeSyncController, float ____startTimeOffset, float ____move1Duration, float ____move2Duration, float ____obstacleDuration)
        {
            ChromaObjectData chromaData = TryGetObjectData<ChromaObjectData>(____obstacleData);
            if (chromaData == null)
            {
                return;
            }

            Track track = chromaData.Track;
            PointDefinition pathPointDefinition = chromaData.LocalPathColor;
            if (track != null || pathPointDefinition != null)
            {
                float jumpDuration = ____move2Duration;
                float elapsedTime = ____audioTimeSyncController.songTime - ____startTimeOffset;
                float normalTime = (elapsedTime - ____move1Duration) / (jumpDuration + ____obstacleDuration);

                Chroma.AnimationHelper.GetColorOffset(pathPointDefinition, track, normalTime, out Color? colorOffset);

                if (colorOffset.HasValue)
                {
                    __instance.SetObstacleColor(colorOffset.Value);
                    __instance.SetActiveColors();
                }
            }
        }
    }
}
