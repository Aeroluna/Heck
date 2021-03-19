namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;
    using NoodleExtensions.Animation;
    using UnityEngine;
    using static ChromaObjectDataManager;
    using static Plugin;

    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Init")]
    internal static class ObstacleControllerInitColorizer
    {
        [HarmonyPriority(Priority.High)]
        private static void Prefix(ObstacleController __instance, ColorManager ____colorManager)
        {
            ObstacleColorizer.OCStart(__instance, ____colorManager.obstaclesColor);
        }

        private static void Postfix(ObstacleController __instance)
        {
            __instance.SetActiveColors();
        }
    }

    [ChromaPatch(typeof(ObstacleController))]
    [ChromaPatch("Init")]
    internal static class ObstacleControllerInit
    {
        private static void Prefix(ObstacleController __instance, ObstacleData obstacleData)
        {
            ChromaObjectData chromaData = ChromaObjectDatas[obstacleData];
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

    [ChromaPatch(typeof(ObstacleController))]
    [ChromaPatch("Update")]
    internal static class ObstacleControllerUpdate
    {
        private static void Postfix(ObstacleController __instance, ObstacleData ____obstacleData, AudioTimeSyncController ____audioTimeSyncController, float ____startTimeOffset, float ____move1Duration, float ____move2Duration, float ____obstacleDuration)
        {
            if (NoodleExtensionsInstalled)
            {
                TrackColorize(__instance, ____obstacleData, ____audioTimeSyncController, ____startTimeOffset, ____move1Duration, ____move2Duration, ____obstacleDuration);
            }
        }

        private static void TrackColorize(ObstacleController obstacleController, ObstacleData obstacleData, AudioTimeSyncController audioTimeSyncController, float startTimeOffset, float move1Duration, float move2Duration, float obstacleDuration)
        {
            if (NoodleExtensions.NoodleController.NoodleExtensionsActive)
            {
                ChromaNoodleData chromaData = ChromaNoodleDatas[obstacleData];

                Track track = chromaData.Track;
                PointDefinition pathPointDefinition = chromaData.LocalPathColor;
                if (track != null || pathPointDefinition != null)
                {
                    float jumpDuration = move2Duration;
                    float elapsedTime = audioTimeSyncController.songTime - startTimeOffset;
                    float normalTime = (elapsedTime - move1Duration) / (jumpDuration + obstacleDuration);

                    Chroma.AnimationHelper.GetColorOffset(pathPointDefinition, track, normalTime, out Color? colorOffset);

                    if (colorOffset.HasValue)
                    {
                        obstacleController.SetObstacleColor(colorOffset.Value);
                        obstacleController.SetActiveColors();
                    }
                }
            }
        }
    }
}
