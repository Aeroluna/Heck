using Chroma.Lighting;
using HarmonyLib;
using JetBrains.Annotations;

namespace Chroma.HarmonyPatches.ScenesTransition
{
    [HarmonyPatch(
        typeof(MultiplayerLevelScenesTransitionSetupDataSO),
        new[] { typeof(string), typeof(IPreviewBeatmapLevel), typeof(BeatmapDifficulty), typeof(BeatmapCharacteristicSO), typeof(IDifficultyBeatmap), typeof(ColorScheme), typeof(GameplayModifiers), typeof(PlayerSpecificSettings), typeof(PracticeSettings), typeof(bool) })]
    [HarmonyPatch("Init")]
    internal static class MultiplayerLevelScenesTransitionSetupDataSOInit
    {
        [UsedImplicitly]
        private static void Postfix(IDifficultyBeatmap difficultyBeatmap, EnvironmentInfoSO ____multiplayerEnvironmentInfo)
        {
            LightIDTableManager.SetEnvironment(____multiplayerEnvironmentInfo.serializedName);

            SceneTransitionHelper.Patch(difficultyBeatmap);
        }
    }
}
