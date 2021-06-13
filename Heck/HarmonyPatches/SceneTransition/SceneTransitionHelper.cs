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
                // Possibly don't need with CJD 2.0.0?
                Dictionary<string, Track> tracks = customBeatmapData.customData.Get<Dictionary<string, Track>>("tracks");
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
