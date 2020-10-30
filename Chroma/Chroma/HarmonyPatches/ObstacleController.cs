namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using Chroma.Utils;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using NoodleExtensions.Animation;
    using UnityEngine;

    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Init")]
    internal static class ObstacleControllerInitColorizer
    {
        [HarmonyPriority(Priority.High)]
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Prefix(ObstacleController __instance)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            ObstacleColorizer.OCStart(__instance);
        }
    }

    [ChromaPatch(typeof(ObstacleController))]
    [ChromaPatch("Init")]
    internal static class ObstacleControllerInit
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Prefix(ObstacleController __instance, ObstacleData obstacleData)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;

                Color? color = ChromaUtils.GetColorFromData(dynData);

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

    [ChromaPatch(typeof(ObstacleController))]
    [ChromaPatch("Update")]
    internal static class ObstacleControllerUpdate
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(ObstacleController __instance, ObstacleData ____obstacleData, AudioTimeSyncController ____audioTimeSyncController, float ____startTimeOffset, float ____move1Duration, float ____move2Duration, float ____obstacleDuration)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (Chroma.Plugin.NoodleExtensionsInstalled)
            {
                TrackColorize(__instance, ____obstacleData, ____audioTimeSyncController, ____startTimeOffset, ____move1Duration, ____move2Duration, ____obstacleDuration);
            }
        }

        private static void TrackColorize(ObstacleController obstacleController, ObstacleData obstacleData, AudioTimeSyncController audioTimeSyncController, float startTimeOffset, float move1Duration, float move2Duration, float obstacleDuration)
        {
            if (NoodleExtensions.NoodleController.NoodleExtensionsActive && obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                Track track = AnimationHelper.GetTrack(dynData);
                dynamic animationObject = Trees.at(dynData, "_animation");

                if (track != null || animationObject != null)
                {
                    float jumpDuration = move2Duration;
                    float elapsedTime = audioTimeSyncController.songTime - startTimeOffset;
                    float normalTime = (elapsedTime - move1Duration) / (jumpDuration + obstacleDuration);

                    Chroma.AnimationHelper.GetColorOffset(animationObject, track, normalTime, out Color? colorOffset);

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
