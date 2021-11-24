using HarmonyLib;
using JetBrains.Annotations;

namespace Heck.HarmonyPatches.SceneTransition
{
    [HarmonyPatch(
        typeof(MultiplayerLevelScenesTransitionSetupDataSO),
        new[] { typeof(string), typeof(IPreviewBeatmapLevel), typeof(BeatmapDifficulty), typeof(BeatmapCharacteristicSO), typeof(IDifficultyBeatmap), typeof(ColorScheme), typeof(GameplayModifiers), typeof(PlayerSpecificSettings), typeof(PracticeSettings), typeof(bool) })]
    [HarmonyPatch("Init")]
    internal static class MultiplayerLevelScenesTransitionSetupDataSOInit
    {
        [UsedImplicitly]
        private static void Postfix(PlayerSpecificSettings playerSpecificSettings)
        {
            SceneTransitionHelper.Patch(playerSpecificSettings);
        }
    }
}
