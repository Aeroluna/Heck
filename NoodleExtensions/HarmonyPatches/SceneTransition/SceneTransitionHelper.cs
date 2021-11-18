namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using static NoodleExtensions.Plugin;

    internal static class SceneTransitionHelper
    {
        internal static void Patch(IDifficultyBeatmap difficultyBeatmap)
        {
            if (difficultyBeatmap.beatmapData is CustomBeatmapData customBeatmapData)
            {
                IEnumerable<string>? requirements = customBeatmapData.beatmapCustomData.Get<List<object>>("_requirements")?.Cast<string>();
                bool noodleRequirement = requirements?.Contains(CAPABILITY) ?? false;
                NoodleController.ToggleNoodlePatches(noodleRequirement);
            }
        }
    }
}
