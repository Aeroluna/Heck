using HarmonyLib;

namespace Heck.HarmonyPatches
{
    [HeckPatch]
    internal static class SceneTransitionModuleActivator
    {
        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(MissionLevelScenesTransitionSetupDataSO),
            nameof(MissionLevelScenesTransitionSetupDataSO.Init))]
        private static void MissionPrefix(IDifficultyBeatmap difficultyBeatmap, IPreviewBeatmapLevel previewBeatmapLevel)
        {
            OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
            ModuleManager.Activate(difficultyBeatmap, previewBeatmapLevel, LevelType.Mission, ref overrideEnvironmentSettings);
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(MultiplayerLevelScenesTransitionSetupDataSO),
            nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
        private static void MultiplayerPrefix(IDifficultyBeatmap difficultyBeatmap, IPreviewBeatmapLevel previewBeatmapLevel)
        {
            OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
            ModuleManager.Activate(difficultyBeatmap, previewBeatmapLevel, LevelType.Multiplayer, ref overrideEnvironmentSettings);
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(StandardLevelScenesTransitionSetupDataSO),
            nameof(StandardLevelScenesTransitionSetupDataSO.Init))]
        private static void StandardPrefix(
            IDifficultyBeatmap difficultyBeatmap,
            IPreviewBeatmapLevel previewBeatmapLevel,
            ref OverrideEnvironmentSettings? overrideEnvironmentSettings)
        {
            ModuleManager.Activate(difficultyBeatmap, previewBeatmapLevel, LevelType.Standard, ref overrideEnvironmentSettings);
        }

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(TutorialScenesTransitionSetupDataSO),
            nameof(TutorialScenesTransitionSetupDataSO.Init))]
        private static void TutorialPrefix()
        {
            OverrideEnvironmentSettings? overrideEnvironmentSettings = null;
            ModuleManager.Activate(null, null, LevelType.Standard, ref overrideEnvironmentSettings);
        }
    }
}
