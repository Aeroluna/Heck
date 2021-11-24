using HarmonyLib;
using JetBrains.Annotations;

namespace Chroma.HarmonyPatches.ScenesTransition
{
    [HarmonyPatch(
        typeof(StandardLevelScenesTransitionSetupDataSO),
        new[] { typeof(string), typeof(IDifficultyBeatmap), typeof(IPreviewBeatmapLevel), typeof(OverrideEnvironmentSettings), typeof(ColorScheme), typeof(GameplayModifiers), typeof(PlayerSpecificSettings), typeof(PracticeSettings), typeof(string), typeof(bool) })]
    [HarmonyPatch("Init")]
    internal static class StandardLevelScenesTransitionSetupDataSOInit
    {
        [UsedImplicitly]
        private static void Prefix(IDifficultyBeatmap difficultyBeatmap, ref OverrideEnvironmentSettings? overrideEnvironmentSettings)
        {
            SceneTransitionHelper.Patch(difficultyBeatmap, ref overrideEnvironmentSettings);
        }
    }
}
