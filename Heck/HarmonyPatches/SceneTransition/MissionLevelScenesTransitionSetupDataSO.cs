using HarmonyLib;
using JetBrains.Annotations;

namespace Heck.HarmonyPatches.SceneTransition
{
    [HarmonyPatch(
        typeof(MissionLevelScenesTransitionSetupDataSO),
        new[] { typeof(string), typeof(IDifficultyBeatmap), typeof(IPreviewBeatmapLevel), typeof(MissionObjective[]), typeof(ColorScheme), typeof(GameplayModifiers), typeof(PlayerSpecificSettings), typeof(string) })]
    [HarmonyPatch("Init")]
    internal static class MissionLevelScenesTransitionSetupDataSOInit
    {
        [UsedImplicitly]
        private static void Postfix(PlayerSpecificSettings playerSpecificSettings)
        {
            SceneTransitionHelper.Patch(playerSpecificSettings);
        }
    }
}
