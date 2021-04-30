namespace Heck.HarmonyPatches
{
    using System.Collections.Generic;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using Heck.Animation;

    internal static class SceneTransitionHelper
    {
        internal static void Patch(IDifficultyBeatmap difficultyBeatmap, PlayerSpecificSettings playerSpecificSettings)
        {
            if (difficultyBeatmap.beatmapData is CustomBeatmapData customBeatmapData)
            {
                // Reset tracks when entering game scene
                Dictionary<string, Track> tracks = Trees.at(customBeatmapData.customData, "tracks");
                if (tracks != null)
                {
                    foreach (KeyValuePair<string, Track> track in tracks)
                    {
                        track.Value.ResetVariables();
                    }
                }
            }

            AnimationHelper.LeftHandedMode = playerSpecificSettings.leftHanded;
        }
    }
}
