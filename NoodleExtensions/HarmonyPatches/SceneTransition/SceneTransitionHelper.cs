using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.SceneTransition
{
    internal static class SceneTransitionHelper
    {
        internal static void Patch(IDifficultyBeatmap difficultyBeatmap)
        {
            if (difficultyBeatmap.beatmapData is not CustomBeatmapData customBeatmapData)
            {
                return;
            }

            IEnumerable<string>? requirements = customBeatmapData.beatmapCustomData.Get<List<object>>("_requirements")?.Cast<string>();
            bool noodleRequirement = requirements?.Contains(CAPABILITY) ?? false;
            ToggleNoodlePatches(noodleRequirement);
        }
    }
}
